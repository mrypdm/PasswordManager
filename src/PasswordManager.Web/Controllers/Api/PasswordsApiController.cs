using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Services;
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
public class PasswordsApiController(IPasswordService passwordService) : Controller
{
    /// <summary>
    /// Verify password strength and compomistaion
    /// </summary>
    [HttpPost("verify")]
    [AllowAnonymous]
    public async Task<ActionResult<VerifyPasswordReponse>> CheckPasswordAsync(
        [FromBody] VerifyPasswordRequest request, CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        var result = await passwordService.CheckPasswordAsync(request.Password, Alphabet.Empty, token);
        return ToVerifyResponse(result);
    }

    /// <summary>
    /// Generate password
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<GeneratePasswordResponse>> GeneratePasswordAsync(
        [FromBody] GeneratePasswordRequest request, CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        var alphabet = SetupAlphabet(request);
        var password = passwordService.GeneratePassword(request.Length, alphabet);
        var checkResult = await passwordService.CheckPasswordAsync(password, alphabet, token);

        return new GeneratePasswordResponse
        {
            Password = password,
            CheckStatus = ToVerifyResponse(checkResult)
        };
    }

    private static VerifyPasswordReponse ToVerifyResponse(PasswordCheckStatus checkStatus)
    {
        return new VerifyPasswordReponse
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
