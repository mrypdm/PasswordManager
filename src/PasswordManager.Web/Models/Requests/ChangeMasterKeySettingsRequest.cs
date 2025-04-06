using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using PasswordManager.Aes;

namespace PasswordManager.Web.Models.Requests;

/// <summary>
/// Request for changin master key parameters
/// </summary>
public class ChangeMasterKeySettingsRequest : IRequest
{
    /// <summary>
    /// Current master password
    /// </summary>
    public string MasterPassword { get; set; }

    /// <summary>
    /// New master password
    /// </summary>
    public string NewMasterPassword { get; set; }

    /// <summary>
    /// Salt for master key generation
    /// </summary>
    public string Salt { get; set; }

    /// <summary>
    /// Salt for master key generation in bytes
    /// </summary>
    [JsonIgnore]
    public byte[] SaltBytes => Convert.FromHexString(Salt);

    /// <summary>
    /// Count of iterations for key generation
    /// </summary>
    public int? Iterations { get; set; }

    /// <inheritdoc />
    public bool Validate([NotNullWhen(false)] out string errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(MasterPassword))
        {
            errorMessage = "Master password is not provided";
        }

        if (Iterations <= 0)
        {
            errorMessage = "Iterations parameter cannot be zero or negative";
        }

        if (Salt is not null)
        {
            try
            {
                Convert.FromHexString(Salt);
            }
            catch (FormatException)
            {
                errorMessage = "Salt parameter is not HEX string";
            }

            if (Salt.Length != AesConstants.BlockSize * 2)
            {
                errorMessage = $"Salt parameters is wrong size {Salt.Length}. Must be {AesConstants.BlockSize * 2}";
            }
        }

        if (NewMasterPassword is not null && string.IsNullOrWhiteSpace(NewMasterPassword))
        {
            errorMessage = "New master password cannot be empty string";
        }

        return errorMessage is null;
    }
}
