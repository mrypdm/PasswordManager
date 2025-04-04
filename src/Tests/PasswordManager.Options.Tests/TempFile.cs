using System;
using System.IO;

namespace PasswordManager.Options.Tests;

/// <summary>
/// Temporary file which be deleted after dispose
/// </summary>
public sealed class TempFile(string path = null) : IDisposable
{
    public string FilePath { get; } = path ?? Path.GetTempFileName();

    public bool Exists => Path.Exists(FilePath);

    public void Write(string content)
    {
        File.WriteAllText(FilePath, content);
    }

    public string Read()
    {
        return File.ReadAllText(FilePath);
    }

    public void Dispose()
    {
        File.Delete(FilePath);
    }
}
