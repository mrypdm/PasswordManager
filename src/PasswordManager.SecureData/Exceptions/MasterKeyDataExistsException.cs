using System;

namespace PasswordManager.SecureData.Exceptions;

/// <summary>
/// Exception that master key data has been already initialized
/// </summary>
public class MasterKeyDataExistsException() : Exception("Master key has been already initialized");
