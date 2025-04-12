using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Flurl.Http;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Models;
using PasswordManager.External.Exceptions;

namespace PasswordManager.External.Checkers;

/// <summary>
/// Сheck password compromise with haveibeenpwned
/// </summary>
public sealed class PwnedPasswordChecker : IPasswordChecker
{
    public const string UrlPrefix = "https://api.pwnedpasswords.com/range";

    /// <inheritdoc/>
    public async Task<PasswordCheckStatus> CheckAsync(string password, CancellationToken token)
    {
        var hash = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(password)));

        try
        {
            var result = await GetCompomisedHashes(hash[..5], token);

            return result.Contains(hash[5..], StringComparison.OrdinalIgnoreCase)
                ? new PasswordCheckStatus(PasswordCompromisation.Compromised, PasswordStrength.VeryLow)
                : new PasswordCheckStatus(PasswordCompromisation.NotCompromised, PasswordStrength.Unknown);
        }
        catch (FlurlHttpException e)
        {
            if (e.StatusCode >= (int)HttpStatusCode.InternalServerError)
            {
                // Server side error. We can't do anything about it.
                return new PasswordCheckStatus(PasswordCompromisation.Unknown, PasswordStrength.Unknown);
            }

            throw new PwnedRequestException($"Error occured in HTTP request to {e.Call.Request.Url}", e);
        }
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
