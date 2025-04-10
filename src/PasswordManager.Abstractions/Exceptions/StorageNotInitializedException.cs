using System;

namespace PasswordManager.Abstractions.Exceptions;

/// <summary>
/// Storage not initialized exception
/// </summary>
public class StorageNotInitializedException(string message) : Exception(message);
