using System;

namespace PasswordManager.SecureData.Exceptions;

/// <summary>
/// Exception that key data has been already initialized
/// </summary>
public class KeyDataExistsException() : Exception("Key has been already initialized");
