using System.Security.Cryptography;
using System.Text;

namespace PhotoSite.Crypto;

public class CryptoProvider : ICryptoProvider {
    public string HashValue(string toHash, string salt, string machineKey) {
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(toHash),
            Encoding.Default.GetBytes(salt),
            350000,
            HashAlgorithmName.SHA512,
            64
        );

        return Convert.ToHexString(hash);
    }

    public bool CompareHashes(string expected, string actual) {
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(expected),
            Convert.FromHexString(actual)
        );
    }
}