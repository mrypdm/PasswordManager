﻿namespace PasswordManager.Abstractions;

/// <summary>
/// Password generator
/// </summary>
public interface IPasswordGenerator
{
    /// <summary>
    /// Generate password with <paramref name="length"/>
    /// </summary>
    string GeneratePassword(int length);
}
