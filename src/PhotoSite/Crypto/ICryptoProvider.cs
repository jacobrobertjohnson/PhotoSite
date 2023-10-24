namespace PhotoSite.Crypto;

public interface ICryptoProvider {
    string HashValue(string toHash, string salt, string machineKey);
    bool CompareHashes(string expected, string actual);
}