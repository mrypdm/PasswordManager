using System;
using System.IO;
using System.Text.Json;
using PasswordManager.Abstractions.Options;

namespace PasswordManager.Core.Options;

/// <summary>
/// Writeable options in JSON format
/// </summary>
public sealed class JsonWriteableOptions<TOptions> : IWritableOptions<TOptions> where TOptions : class
{
    private readonly string _filePath;

    internal JsonWriteableOptions(TOptions settings, string settingsFilePath)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settingsFilePath);

        Value = settings;
        _filePath = settingsFilePath;
    }

    /// <inheritdoc />
    public TOptions Value { get; }

    /// <inheritdoc />
    public void Update(Action<TOptions> updateAction)
    {
        ArgumentNullException.ThrowIfNull(updateAction);
        updateAction(Value);
        Value.DumpToDisk(_filePath);
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
    public static JsonWriteableOptions<TOptions> Create<TOptions>(string settingsFilePath)
        where TOptions : class, new()
    {
        var settings = ReadSettingsFromFile<TOptions>(settingsFilePath);
        if (settings is null)
        {
            settings = new();
            settings.DumpToDisk(settingsFilePath);
        }

        return new JsonWriteableOptions<TOptions>(settings, settingsFilePath);
    }

    internal static void DumpToDisk<TOptions>(this TOptions options, string path)
    {
        using var fileStream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        JsonSerializer.Serialize(fileStream, options, JsonOptions);
    }

    private static TOptions ReadSettingsFromFile<TOptions>(string settingsFilePath)
    {
        try
        {
            using var fileStream = File.OpenRead(settingsFilePath);
            return JsonSerializer.Deserialize<TOptions>(fileStream, JsonOptions);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or JsonException)
        {
            return default;
        }
    }
}
