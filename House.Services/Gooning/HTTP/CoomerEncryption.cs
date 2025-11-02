using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace House.House.Services.Gooning.HTTP;

public static class CoomerEncryption
{
    private const int KeySize = 32;
    private const int IvSize = 16;
    private const int SaltSize = 16;
    private const int Iterations = 100_000;

    public static byte[] GenerateSalt()
    {
        return RandomNumberGenerator.GetBytes(SaltSize);
    }

    public static byte[] DeriveKey(string password, byte[] salt)
    {
        Rfc2898DeriveBytes rfc2898 = new(password, salt, Iterations, HashAlgorithmName.SHA256);

        return rfc2898.GetBytes(KeySize);
    }

    public static byte[] DeriveHMACKey(string password, byte[] salt)
    {
        using Rfc2898DeriveBytes rfc2898 = new(password, salt, Iterations, HashAlgorithmName.SHA256);

        byte[] fullKey = rfc2898.GetBytes(64);
        byte[] hmacKey = fullKey.AsSpan(KeySize, KeySize).ToArray();

        Array.Clear(fullKey, 0, fullKey.Length);

        return hmacKey;
    }

    public static byte[] ComputeHMAC(byte[] hmacKey, byte[] iv, byte[] salt, byte[] cipherText)
    {
        using HMACSHA256 hmac = new(hmacKey);

        byte[] combined = new byte[iv.Length + salt.Length + cipherText.Length];

        Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
        Buffer.BlockCopy(salt, 0, combined, iv.Length, salt.Length);
        Buffer.BlockCopy(cipherText, 0, combined, iv.Length + salt.Length, cipherText.Length);

        return hmac.ComputeHash(combined);
    }

    public static string Encrypt<T>(T data, byte[] key, out byte[] iv)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        iv = RandomNumberGenerator.GetBytes(IvSize);

        using var aes = Aes.Create();

        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var json = JsonSerializer.Serialize(data);
        var plainBytes = Encoding.UTF8.GetBytes(json);

        using var encryptor = aes.CreateEncryptor();
        byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(cipherBytes);
    }

    public static T Decrypt<T>(string base64String, byte[] key, byte[] iv)
    {
        var cipherBytes = Convert.FromBase64String(base64String);

        using var aes = Aes.Create();

        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using ICryptoTransform decryptor = aes.CreateDecryptor();
        byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        var json = Encoding.UTF8.GetString(plainBytes);
        return JsonSerializer.Deserialize<T>(json)!;
    }
}

public class EncryptedDocument
{
    [BsonId]
    public ulong AuthorID { get; set; }

    public string EncryptedData { get; set; } = string.Empty;
    public string IV { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string HMAC { get; set; } = string.Empty;

    public int SchemaVersion { get; set; } = 1;
}


public sealed class EncryptedStorageService<T>
{
    private readonly IMongoCollection<EncryptedDocument> collection;

    public EncryptedStorageService(IMongoDatabase database, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        collection = database.GetCollection<EncryptedDocument>(name);
    }

    public async Task StoreEncryptedAsync(ulong ID, T data, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        }

        byte[] salt = CoomerEncryption.GenerateSalt();
        byte[] aesKey = CoomerEncryption.DeriveKey(password, salt);
        byte[] hmacKey = CoomerEncryption.DeriveHMACKey(password, salt);

        try
        {
            string encryptedData = CoomerEncryption.Encrypt(data, aesKey, out byte[] iv);

            byte[] cipherBytes = Convert.FromBase64String(encryptedData);
            byte[] hmacBytes = CoomerEncryption.ComputeHMAC(hmacKey, iv, salt, cipherBytes);

            EncryptedDocument document = new()
            {
                AuthorID = ID,
                EncryptedData = encryptedData,
                Salt = Convert.ToBase64String(salt),
                IV = Convert.ToBase64String(iv),
                HMAC = Convert.ToBase64String(hmacBytes),
                SchemaVersion = 1
            };

            var filter = Builders<EncryptedDocument>.Filter.Eq(d => d.AuthorID, ID);
            await collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true });
        }
        finally
        {
            Array.Clear(aesKey, 0, aesKey.Length);
            Array.Clear(hmacKey, 0, hmacKey.Length);
        }
    }

    public async Task<T?> LoadDecryptedAsync(ulong ID, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        }

        var filter = Builders<EncryptedDocument>.Filter.Eq(d => d.AuthorID, ID);
        var document = await collection.Find(filter).FirstOrDefaultAsync();

        if (document == null)
        {
            return default;
        }

        byte[] aesKey = [];
        byte[] hmacKey = [];

        try
        {
            byte[] salt = Convert.FromBase64String(document.Salt);
            byte[] iv = Convert.FromBase64String(document.IV);
            byte[] storedHmac = Convert.FromBase64String(document.HMAC);
            byte[] cipherBytes = Convert.FromBase64String(document.EncryptedData);

            aesKey = CoomerEncryption.DeriveKey(password, salt);
            hmacKey = CoomerEncryption.DeriveHMACKey(password, salt);

            byte[] computedHmac = CoomerEncryption.ComputeHMAC(hmacKey, iv, salt, cipherBytes);
            if (!CryptographicOperations.FixedTimeEquals(storedHmac, computedHmac))
            {
                return default;
            }

            return CoomerEncryption.Decrypt<T>(document.EncryptedData, aesKey, iv);
        }
        catch (FormatException)
        {
            return default;
        }
        catch (CryptographicException)
        {
            return default;
        }
        finally
        {
            Array.Clear(aesKey, 0, aesKey.Length);
            Array.Clear(hmacKey, 0, hmacKey.Length);
        }
    }
}
