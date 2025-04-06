using System;

namespace PasswordManager.SecureData.Exceptions;

/// <summary>
/// Storage not initialized exception
/// </summary>
public class StorageIsNotInitializedException(string message) : Exception(message);
