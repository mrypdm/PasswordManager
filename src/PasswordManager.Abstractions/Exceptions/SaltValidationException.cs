using System;

namespace PasswordManager.Abstractions.Exceptions;

/// <summary>
/// Salt is invalid exception
/// </summary>
public class SaltValidationException(string message) : Exception(message);
