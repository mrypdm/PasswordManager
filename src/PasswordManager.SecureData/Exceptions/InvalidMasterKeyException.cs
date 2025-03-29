using System;

namespace PasswordManager.SecureData.Exceptions;

/// <summary>
/// Exception that master key is invalid
/// </summary>
public class InvalidMasterKeyException() : Exception("Master key is invalid");
