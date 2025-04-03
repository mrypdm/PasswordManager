using System;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.SecureData.Exceptions;

namespace PasswordManager.SecureData.Services;

/// <summary>
/// Service for master key
/// </summary>
public interface IMasterKeyService
{
    /// <summary>
    /// Init master key from <paramref name="masterPassword"/> and validates it with current master key data.
    /// If not master key data exists, then creates master key data with new master key
    /// </summary>
    /// <exception cref="InvalidMasterKeyException">If master password is invalid</exception>
    Task InitMasterKeyAsync(string masterPassword, TimeSpan sessionTimeout, CancellationToken token);
}
