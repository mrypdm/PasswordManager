using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Abstractions.Alphabets;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Models;
using PasswordManager.Core.Alphabets;
using PasswordManager.Web.Filters;
using PasswordManager.Web.Models.Requests;
using PasswordManager.Web.Models.Responses;

namespace PasswordManager.Web.Controllers.Api;

/// <summary>
/// Controller for password manipulations
/// </summary>
[Route("api/password")]
[ValidateModelState]
[ValidateAntiForgeryToken]
public class PasswordsApiController(
    IPasswordGeneratorFactory passwordGeneratorFactory,
    IPasswordCheckerFactory passwordCheckerFactory) : Controller
{
    /// <summary>
    /// Verify password strength and compomistaion
    /// </summary>
    [HttpPost("verify")]
    [AllowAnonymous]
    public async Task<ActionResult<PasswordVerifyReponse>> VerifyPasswordAsync(
        [FromBody] VerifyPasswordRequest request, CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        var result = await VerifyPasswordAsync(request.Password, Alphabet.Empty, token);
        return ToVerifyResponse(result);
    }

    /// <summary>
    /// Generate password
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<PasswordGenerateResponse>> GeneratePasswordAsync(
        [FromBody] GeneratePasswordRequest request, CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        var alphabet = SetupAlphabet(request);
        var password = passwordGeneratorFactory.Create(alphabet).Generate(request.Length);
        var checkResult = await VerifyPasswordAsync(password, alphabet, token);

        return new PasswordGenerateResponse
        {
            Password = password,
            CheckStatus = ToVerifyResponse(checkResult)
        };
    }

    private async Task<PasswordCheckStatus> VerifyPasswordAsync(string password, IAlphabet alphabet,
        CancellationToken token)
    {
        return await passwordCheckerFactory.Create(alphabet).CheckAsync(password, token);
    }

    private static PasswordVerifyReponse ToVerifyResponse(PasswordCheckStatus checkStatus)
    {
        return new PasswordVerifyReponse
        {
            IsCompomised = checkStatus.IsCompromised == PasswordCompromisation.Compromised,
            Strength = checkStatus.Strength switch
            {
                PasswordStrength.VeryLow => "very low",
                PasswordStrength.VeryHigh => "very high",
                _ => checkStatus.Strength.ToString().ToLower()
            }
        };
    }

    private static Alphabet SetupAlphabet(GeneratePasswordRequest request)
    {
        var alphabet = new Alphabet();
        if (request.UseNumbers)
        {
            alphabet.WithNumbers();
        }

        if (request.UseLowerLetters)
        {
            alphabet.WithLowerLetters();
        }

        if (request.UseUpperLetters)
        {
            alphabet.WithUpperLetters();
        }

        alphabet.WithCharacters(request.SpecialSymbols);
        return alphabet;
    }
}
