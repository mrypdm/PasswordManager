using System.Threading;
using System.Threading.Tasks;

namespace PasswordManager.SecureData.Services;

/// <summary>
/// Service for master key
/// </summary>
public interface IMasterKeyService
{
    /// <summary>
    /// Creates master key from <paramref name="masterPassword"/> and validates it with current master key data.
    /// If not master key data exists, then creates master key data with new master key
    /// </summary>
    Task<byte[]> CreateMasterKeyAsync(string masterPassword, CancellationToken token);
}
