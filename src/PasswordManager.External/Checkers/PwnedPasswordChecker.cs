using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Flurl.Http;

using PasswordManager.Abstractions;

namespace PasswordManager.External.Checkers;

/// <summary>
/// Сheck password compromise with haveibeenpwned
/// </summary>
public class PwnedPasswordChecker : IPasswordChecker
{
    public const string UrlPrefix = "https://api.pwnedpasswords.com/range";

    /// <inheritdoc/>
    public async Task<PasswordCheckStatus> CheckPasswordAsync(string password, CancellationToken token)
    {
        var hash = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(password)));
        var result = await GetCompomisedHashes(hash[..5], token);

        return result.Contains(hash[5..], StringComparison.OrdinalIgnoreCase)
            ? new PasswordCheckStatus(PasswordCompromisation.Compromised, PasswordStrength.VeryLow)
            : new PasswordCheckStatus(PasswordCompromisation.NotCompromised, PasswordStrength.Unknown);
    }

    /// <summary>
    /// Returns suffixes of hashes (SHA1) of compomised passwords
    /// </summary>
    /// <param name="prefix">Prefix (5-digits) of hash</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Suffixes of compromised passwords</returns>
    private static Task<string> GetCompomisedHashes(string prefix, CancellationToken token)
    {
        return $"{UrlPrefix}/{prefix}".GetStringAsync(cancellationToken: token);
    }
}
