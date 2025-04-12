using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using PasswordManager.Aes;
using PasswordManager.Core.Converters;

namespace PasswordManager.Web.Models.Requests;

/// <summary>
/// Request for changin key parameters
/// </summary>
public class ChangeKeySettingsRequest : IRequest
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
    /// Salt for key generation
    /// </summary>
    [JsonConverter(typeof(JsonStringBytesConverter))]
    public byte[] Salt { get; set; }

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

        if (Salt is not null && Salt.Length != AesConstants.BlockSize)
        {
            errorMessage = $"Salt parameter has wrong size {Salt.Length}. Must be {AesConstants.BlockSize}";
        }

        if (NewMasterPassword is not null && string.IsNullOrWhiteSpace(NewMasterPassword))
        {
            errorMessage = "New master password cannot be empty string";
        }

        return errorMessage is null;
    }

    /// <summary>
    /// If request can change something
    /// </summary>
    public bool HasSense()
    {
        return Salt is not null
            || Iterations is not null
            || (NewMasterPassword is not null && NewMasterPassword != MasterPassword);
    }
}
