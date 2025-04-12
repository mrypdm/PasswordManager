using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Abstractions.Alphabets;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Models;
using PasswordManager.Core.Alphabets;
using PasswordManager.Web.Models.Requests;
using PasswordManager.Web.Models.Responses;

namespace PasswordManager.Web.Controllers.Api;

/// <summary>
/// Controller for password manipulations
/// </summary>
[ApiController]
[Route("api/password")]
[ValidateAntiForgeryToken]
public class PasswordsApiController(
    IPasswordGeneratorFactory passwordGeneratorFactory,
    IEnumerable<IPasswordCheckerFactory> passwordCheckerFactories) : Controller
{
    /// <summary>
    /// Verify password strength and compomistaion
    /// </summary>
    [HttpPost("verify")]
    [AllowAnonymous]
    public async Task<ActionResult<PasswordVerifyReponse>> VerifyPasswordAsync([FromBody] VerifyPasswordRequest request,
        CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        var result = await VerifyPasswordAsync(request.Password, Alphabet.Empty, token);
        return Map(result);
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
        var generator = passwordGeneratorFactory.Create(alphabet);

        var password = generator.Generate(request.Length);
        var checkResult = await VerifyPasswordAsync(password, alphabet, token);

        return new PasswordGenerateResponse
        {
            Password = password,
            CheckStatus = Map(checkResult)
        };
    }

    private async Task<PasswordCheckStatus> VerifyPasswordAsync(string password, IAlphabet alphabet,
        CancellationToken token)
    {
        var result = new PasswordCheckStatus(PasswordCompromisation.Unknown, PasswordStrength.Unknown);

        foreach (var factory in passwordCheckerFactories)
        {
            var checker = factory.Create(alphabet);
            var checkResult = await checker.CheckAsync(password, token);
            result = PasswordCheckStatus.MinOf(result, checkResult);
        }

        return result;
    }

    private static PasswordVerifyReponse Map(PasswordCheckStatus checkStatus)
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
