using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EGOIST.Application.DTOs.Management.Models.HuggingFace;
using EGOIST.Domain.Entities;


namespace EGOIST.Application.Utilities;
public static class Extensions
{
    #region Events
    public delegate void SaveDataEvent();
    public static SaveDataEvent onSaveData;

    public static void SaveData()
    {
        onSaveData?.Invoke();
    }

    public delegate void LoadDataEvent();
    public static LoadDataEvent onLoadData;

    public static void LoadData()
    {
        onLoadData?.Invoke();
    }

    #endregion

    #region DataHandlingMethods
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

        double megabytes = bytes / megabyte;
        return megabytes;
    }

    public static double BytesToGB(this long bytes)
    {
        const double gigabyte = 1024 * 1024 * 1024;

        double gigabytes = bytes / gigabyte;
        return gigabytes;
    }

    public static double ConvertToUnit(string sizeString, string targetUnit)
    {
        string[] parts = sizeString.Split(' ');
        double value = double.Parse(parts[0]);
        string unit = parts[1].Trim().ToLower();

        return unit switch
        {
            "gb" => ConvertToUnit(value * 1024 * 1024, "MB", targetUnit),
            "mb" => ConvertToUnit(value * 1024, "KB", targetUnit),
            "kb" => ConvertToUnit(value, "KB", targetUnit),
            _ => throw new ArgumentException("Unsupported unit.")
        };
    }

    public static double ConvertToUnit(double value, string sourceUnit, string targetUnit) =>
        (sourceUnit, targetUnit) switch
        {
            ("MB", "GB") => value / 1024,
            ("KB", "GB") => value / (1024 * 1024),
            ("KB", "MB") => value / 1024,
            _ => value
        };

    public static int TextModelLayersCount(string parameters, string size, int freeVram)
    {
        double Size = ConvertToUnit(size, "MB");
        double parameterCount = double.Parse(parameters);

        var layerSize = parameterCount switch
        {
            7 => Size / 32,
            13 => Size / 40,
            70 => Size / 80,
            _ => throw new ArgumentException("Unsupported parameter count.")
        };

        var layersCount = freeVram * (1 - 0.20) / layerSize;
        return (int)layersCount;
    }
    #endregion

    #region SecurityMethods
    public static string CalculateMD5Hash(this Stream? stream)
    {
        using var md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }


    public static byte[] Encrypt<T>(T obj, string secretKey)
    {
        byte[] data;
        using var stream = new MemoryStream();
        var json = JsonSerializer.Serialize(obj);
        stream.Write(Encoding.UTF8.GetBytes(json));
        data = stream.ToArray();

        var key = MD5.HashData(Encoding.UTF8.GetBytes(secretKey));
        using var des = new TripleDESCryptoServiceProvider();
        des.Key = key;
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.PKCS7;

        using var encryptedStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(encryptedStream, des.CreateEncryptor(), CryptoStreamMode.Write);
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();

        return encryptedStream.ToArray();
    }

    public static T Decrypt<T>(byte[] obj, string secretKey)
    {
        var key = MD5.HashData(Encoding.UTF8.GetBytes(secretKey));
        using var des = new TripleDESCryptoServiceProvider();
        des.Key = key;
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.PKCS7;

        using var decryptedStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(decryptedStream, des.CreateDecryptor(), CryptoStreamMode.Write);
        cryptoStream.Write(obj, 0, obj.Length);
        cryptoStream.FlushFinalBlock();

        decryptedStream.Position = 0;
        var json = Encoding.UTF8.GetString(decryptedStream.ToArray());

        return JsonSerializer.Deserialize<T>(json);
    }

    #endregion
}