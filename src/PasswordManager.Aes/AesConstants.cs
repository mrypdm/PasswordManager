namespace PasswordManager.Aes;

/// <summary>
/// Constants for AES encryption
/// </summary>
public static class AesConstants
{
    /// <summary>
    /// AES key size 256 bit, 32 bytes
    /// </summary>
    public const int KeySize = 256 / 8;

    /// <summary>
    /// AES block size 128 bit, 16 bytes
    /// </summary>
    public const int BlockSize = 128 / 8;
}
