using System.Collections.ObjectModel;
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

    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    public static string RemoveSpaces(this string text)
    {
        return text.Replace(" ", "");
    }

    public static double BytesToMB(this long bytes)
    {
        const double megabyte = 1024 * 1024;

        double megabytes = (double)bytes / megabyte;
        return megabytes;
    }

    public static double BytesToGB(this long bytes)
    {
        const double gigabyte = 1024 * 1024 * 1024;

        double gigabytes = (double)bytes / gigabyte;
        return gigabytes;
    }
}