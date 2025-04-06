using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Models;
using PasswordManager.Core;
using PasswordManager.Web.Models.Requests;
using PasswordManager.Web.Models.Responses;

namespace PasswordManager.Web.Controllers;

/// <summary>
/// Controller for password manipulations
/// </summary>
[Route("api/password")]
[ValidateAntiForgeryToken]
public class PasswordsController(
    IEnumerable<IPasswordChecker> passwordCheckers,
    IPasswordCheckerFactory passwordCheckerFactory,
    IPasswordGeneratorFactory passwordGeneratorFactory) : Controller
{
    /// <summary>
    /// Verify password strength and compomistaion
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [Route("verify")]
    public async Task<ActionResult<PasswordVerifyReponse>> VerifyPasswordAsync([FromBody] PasswordVerifyRequest request,
        CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        var result = await VerifyPasswordAsync(request.Password, token);
        return Map(result);
    }

    /// <summary>
    /// Generate password
    /// </summary>
    [HttpPost]
    [Route("generate")]
    public async Task<ActionResult<PasswordGenerateResponse>> GeneratePasswordAsync(
        [FromBody] PassworgGenerateRequest request, CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        var alphabet = SetupAlphabet(request);
        var generator = passwordGeneratorFactory.Create(alphabet);
        var checker = passwordCheckerFactory.Create(alphabet);

        var password = generator.GeneratePassword(request.Length);
        var checkResult = PasswordCheckStatus.MinOf(
            await checker.CheckPasswordAsync(password, token),
            await VerifyPasswordAsync(password, token));

        return new PasswordGenerateResponse
        {
            Password = password,
            CheckStatus = Map(checkResult)
        };
    }

    private async Task<PasswordCheckStatus> VerifyPasswordAsync(string password, CancellationToken token)
    {
        var result = new PasswordCheckStatus(PasswordCompromisation.Unknown, PasswordStrength.Unknown, double.MaxValue);

        foreach (var checker in passwordCheckers)
        {
            var checkResult = await checker.CheckPasswordAsync(password, token);
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

    private static Alphabet SetupAlphabet(PassworgGenerateRequest request)
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
