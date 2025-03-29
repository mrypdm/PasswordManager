using System;

namespace PasswordManager.Abstractions.Exceptions;

/// <summary>
/// Key is invalid exception
/// </summary>
public class KeyValidationException(string message) : Exception(message);
