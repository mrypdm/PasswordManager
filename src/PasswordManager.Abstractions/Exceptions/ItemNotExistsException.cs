using System;

namespace PasswordManager.Abstractions.Exceptions;

/// <summary>
/// Item not exists exception
/// </summary>
public class ItemNotExistsException(string message) : Exception(message);
