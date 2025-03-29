using System;

namespace PasswordManager.SecureData.Exceptions;

/// <summary>
/// Exception that master key is not initialized
/// </summary>
public class MasterKeyNotInitializedException() : Exception("Master key data is not present in DB");
