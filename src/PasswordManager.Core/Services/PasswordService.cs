using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Alphabets;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Services;

namespace PasswordManager.Core.Services;

/// <inheritdoc />
public class PasswordService(
    IPasswordGeneratorFactory passwordGeneratorFactory,
    IPasswordCheckerFactory passwordCheckerFactory) : IPasswordService
{
    /// <inheritdoc />
    public string GeneratePassword(int length, IAlphabet alphabet)
    {
        return passwordGeneratorFactory.Create(alphabet).Generate(length);
    }

    /// <inheritdoc />
    public async Task<PasswordCheckStatus> CheckPasswordAsync(string password, IAlphabet alphabet,
        CancellationToken token)
    {
        return await passwordCheckerFactory.Create(alphabet).CheckAsync(password, token);
    }
}
