using System;

namespace PasswordManager.SecureData.Exceptions;

/// <summary>
/// Storage not initialized exception
/// </summary>
public class StorageNotInitializedException(string message) : Exception(message);
