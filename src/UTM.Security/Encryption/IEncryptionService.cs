namespace UTM.Security.Encryption;

public interface IEncryptionService
{
    string Encrypt(string plainText, string key);
    string Decrypt(string cipherText, string key);
    byte[] EncryptBytes(byte[] plainData, byte[] key, byte[] iv);
    byte[] DecryptBytes(byte[] cipherData, byte[] key, byte[] iv);
}
