using System;

namespace PasswordManager.Abstractions.Exceptions;

/// <summary>
/// Exception that key is not initialized
/// </summary>
public class KeyDataNotExistsException() : Exception("Key data is not initialized");
