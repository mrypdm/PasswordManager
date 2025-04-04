using System;

namespace PasswordManager.SecureData.Exceptions;

/// <summary>
/// Item not exists exception
/// </summary>
public class ItemNotExistsException(string message) : Exception(message);
