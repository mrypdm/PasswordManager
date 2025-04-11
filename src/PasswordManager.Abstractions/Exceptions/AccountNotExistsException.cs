using System;

namespace PasswordManager.Abstractions.Exceptions;

/// <summary>
/// Account not exists exception
/// </summary>
public class AccountNotExistsException(string message) : Exception(message);
