using System;

namespace PasswordManager.SecureData.Exceptions;

/// <summary>
/// Exception that key is invalid
/// </summary>
public class InvalidKeyException() : Exception("Key is invalid");
