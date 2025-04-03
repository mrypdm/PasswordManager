using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.UserSettings;

namespace PasswordManager.Options;

/// <summary>
/// Writeable options in JSON format
/// </summary>
public sealed class JsonWriteableOptions<TOptions> : IWritableOptions<TOptions> where TOptions : class, new()
{
    private readonly string _settingsFilePath;

    internal JsonWriteableOptions(TOptions settings, string settingsFilePath)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settingsFilePath);

        Value = settings;
        _settingsFilePath = settingsFilePath;
    }

    /// <inheritdoc />
    public TOptions Value { get; }

    /// <inheritdoc />
    public async Task UpdateAsync(Action<TOptions> updateAction, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(updateAction);
        updateAction(Value);

        using var fileStream = File.Open(_settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        await JsonSerializer.SerializeAsync(fileStream, Value, JsonWriteableOptions.JsonOptions, token);
    }
}

/// <summary>
/// Factory for <see cref="JsonWriteableOptions{TOptions}"/>
/// </summary>
public static class JsonWriteableOptions
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };

    /// <summary>
    /// Create options from settings file or create new if file does not exists
    /// </summary>
    public static async Task<JsonWriteableOptions<TOptions>> CreateAsync<TOptions>(string settingsFilePath,
        CancellationToken token)
        where TOptions : class, new()
    {
        var settings = await ReadSettingsFromFile<TOptions>(settingsFilePath, token) ?? new();
        return new JsonWriteableOptions<TOptions>(settings, settingsFilePath);
    }

    private static async Task<TOptions> ReadSettingsFromFile<TOptions>(string settingsFilePath,
        CancellationToken token)
    {
        try
        {
            using var fileStream = File.OpenRead(settingsFilePath);
            return await JsonSerializer.DeserializeAsync<TOptions>(fileStream, JsonOptions, token);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or JsonException)
        {
            return default;
        }
    }
}
