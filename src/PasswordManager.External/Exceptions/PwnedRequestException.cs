using System;

namespace PasswordManager.External.Exceptions;

/// <summary>
/// Error in HTTP call to Pwned
/// </summary>
public class PwnedRequestException(string message, Exception innerException) : Exception(message, innerException);
