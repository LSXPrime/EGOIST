using System.IO;

namespace EGOIST.Helpers;
public static class Extensions
{
    public static string CalculateMD5Hash(this Stream stream)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        byte[] hashBytes = md5.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}