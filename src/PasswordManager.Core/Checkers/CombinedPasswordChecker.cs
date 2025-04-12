using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Models;

namespace PasswordManager.Core.Checkers;

/// <summary>
/// Combined password checker
/// </summary>
public class CombinedPasswordChecker(IEnumerable<IPasswordChecker> checkers) : IPasswordChecker
{
    /// <inheritdoc />
    public async Task<PasswordCheckStatus> CheckAsync(string password, CancellationToken token)
    {
        var result = new PasswordCheckStatus(PasswordCompromisation.Unknown, PasswordStrength.Unknown);

        foreach (var checker in checkers)
        {
            var checkResult = await checker.CheckAsync(password, token);
            result = PasswordCheckStatus.MinOf(result, checkResult);
        }

        return result;
    }
}
