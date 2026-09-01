using DevHabit.API.Settings;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace DevHabit.API.Services;

public sealed class EncryptionService(IOptions<EncryptionOptions> options)
{
    private readonly byte[] _masterKey = Convert.FromBase64String(options.Value.Key);
    private const int IvSize = 16; // AES block size in bytes
    public string Encrypt(string plainText)
    {
        try
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("The plain text cannot be null or empty.", nameof(plainText));
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _masterKey;
            aes.IV = RandomNumberGenerator.GetBytes(IvSize);

            using var memoryStream = new MemoryStream();
            memoryStream.Write(aes.IV, 0, IvSize);

            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText);
            }
            return Convert.ToBase64String(memoryStream.ToArray());
        }
        catch(CryptographicException ex)
        {
            throw new InvalidOperationException("An error occurred during encryption.", ex);
        }
    }
    public string Decrypt(string cipherText)
    {
        try
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            if(cipherBytes.Length < IvSize)
            {
                throw new InvalidOperationException("The cipher text is too short to contain a valid IV.");
            }
            byte[] iv = new byte[IvSize];
            byte[] encryptedData = new byte[cipherBytes.Length - IvSize];

            Buffer.BlockCopy(cipherBytes, 0, iv, 0, IvSize);
            Buffer.BlockCopy(cipherBytes, IvSize, encryptedData, 0, encryptedData.Length);

            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _masterKey;
            aes.IV = iv;

            using MemoryStream memoryStream = new MemoryStream(encryptedData);
            using ICryptoTransform decryptor = aes.CreateDecryptor();
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
        catch(CryptographicException ex)
        {
            throw new InvalidOperationException("An error occurred during decryption.", ex);
        }
    }
}
