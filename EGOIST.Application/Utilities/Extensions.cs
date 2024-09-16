using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using EGOIST.Application.DTOs.Management.Models.HuggingFace;
using EGOIST.Domain.Entities;
using Microsoft.Extensions.Logging;
using Serilog;


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
    
    public static bool TryDeserialize<T>(this string json, out T result, string? failureMessage = null)
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(json)!;
            return true;
        }
        catch (JsonException e)
        {
            result = default!;
            Log.Logger.Error(e, "Failed to deserialize json, {failureMessage}", failureMessage);
            return false;
        }
    }
    
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> items)
    {
        return new ObservableCollection<T>(items);
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

    public static double BytesToMb(this long bytes)
    {
        const double megabyte = 1024 * 1024;

        var megabytes = bytes / megabyte;
        return megabytes;
    }

    public static double BytesToGb(this long bytes)
    {
        const double gigabyte = 1024 * 1024 * 1024;

        var gigabytes = bytes / gigabyte;
        return gigabytes;
    }

    private static double ConvertToUnit(string sizeString, string targetUnit)
    {
        var (value, unit) = Regex.Match(sizeString, @"(\d+\.?\d*)\s*([a-zA-Z]+)?") is { Success: true } match
            ? (double.Parse(match.Groups[1].Value),
                match.Groups[2].Success ? match.Groups[2].Value.Trim().ToLower() : "b")
            : throw new ArgumentException("Invalid size string format.");

        // Convert to bytes first
        var bytes = unit switch
        {
            "gb" => value * 1024 * 1024 * 1024,
            "mb" => value * 1024 * 1024,
            "kb" => value * 1024,
            "b" => value,
            _ => throw new ArgumentException("Unsupported unit.")
        };

        return ConvertToUnit(bytes, "B", targetUnit);
    }

    private static double ConvertToUnit(double value, string sourceUnit, string targetUnit)
    {
        // Normalize units to lowercase
        sourceUnit = sourceUnit.ToLower();
        targetUnit = targetUnit.ToLower();

        // Handle cases where source and target units are the same
        if (sourceUnit == targetUnit)
            return value;

        // Convert everything to bytes first
        var bytes = sourceUnit switch
        {
            "gb" => value * 1024 * 1024 * 1024,
            "mb" => value * 1024 * 1024,
            "kb" => value * 1024,
            "b" => value,
            _ => throw new ArgumentException("Unsupported source unit.")
        };

        // Convert from bytes to the target unit
        return targetUnit switch
        {
            "gb" => bytes / (1024 * 1024 * 1024),
            "mb" => bytes / (1024 * 1024),
            "kb" => bytes / 1024,
            "b" => bytes,
            _ => throw new ArgumentException("Unsupported target unit.")
        };
    }

    public static int TextModelLayersCount(ModelInfo model, ModelInfoWeight weight, int freeVram)
    {
        // Parse parameters (e.g., "7B", "13B") and convert to a number
        var parametersCount = (model.Parameters.Length == 0
            ? (ExtractParametersCountFromModelName(weight.Weight) == 0
                ? ExtractParametersCountFromModelName(model.Name)
                : ExtractParametersCountFromModelName(weight.Weight))
            : double.Parse(model.Parameters.TrimEnd('B')) * 1e9) / 1e8; // Assuming 'B' stands for a billion

        // Convert size to MB
        var sizeInMb = ConvertToUnit(weight.Size.ToString(), "MB");

        // Get the weight type
        var weightType = weight.Weight.GetModelWeight();

        // Estimate memory usage per parameter based on weight type
        var bytesPerParameter = GetBytesPerParameter(weightType);

        // Calculate estimated memory for parameters
        var parameterMemory = parametersCount * 1e8 * bytesPerParameter;

        // Estimate overhead (more conservative for llama.cpp)
        var estimatedOverhead = parameterMemory * 1.0; // Increased overhead factor (3 , 1.5)

        // Total estimated memory usage in MB
        var totalEstimatedMemory = (parameterMemory + estimatedOverhead) / (1024 * 1024); // Convert bytes to MB

        // Very rough estimate of layers - assumes uniform memory usage per layer (not accurate)
        var totalLayers = sizeInMb / (totalEstimatedMemory / parametersCount); // Approximation
        var estimatedLayers = (int)Math.Floor(freeVram / totalEstimatedMemory * totalLayers);

        // Clamp the result to be within a reasonable range (e.g., 1 to totalLayers)
        estimatedLayers = Math.Max(1, estimatedLayers);
        estimatedLayers = Math.Min(estimatedLayers, (int)totalLayers);

        return estimatedLayers;
    }

    // Helper function to get bytes per parameter based on weight type
    private static double GetBytesPerParameter(string weightType)
    {
        return weightType switch
        {
            "FP32" => 4 // 32-bit floating point
            ,
            "FP16" => 2 // 16-bit floating point
            ,
            "Q8_0" or "Q8_1" or "Q8_K" => 1 // 8-bit quantized
            ,
            "Q6_K" or "Q6_S" or "Q6_K_M" or "Q6_K_S"  => 0.75 // 6-bit quantized
            ,
            "Q5_0" or "Q5_1" or "Q5_K" or "Q5_S" or "Q5_K_M" or "Q5_K_S" => 0.625 // 5-bit quantized
            ,
            "Q4_0" or "Q4_1" or "Q4_K" or "Q4_S" or "Q4_K_M" or "Q4_K_S"  or "IQ4_XS" or "IQ4_NL" => 0.5 // 4-bit quantized
            ,
            "Q3_K" or "IQ3_S" or "IQ3_XXS" => 0.375 // 3-bit quantized
            ,
            "Q2_K" or "IQ2_XXS" or "IQ2_S" or "IQ2_XS" => 0.25 // 2-bit quantized
            ,
            "IQ1_S" => 0.125 // 1-bit quantized (rare)
            ,
            _ => 2
        };
    }

    private static double ExtractParametersCountFromModelName(string modelName)
    {
        // Match patterns like "7B", "1.9B", "300M", "47.5M"
        var match = Regex.Match(modelName, @"(\d+(\.\d+)?)([bBmM])");
        if (!match.Success) return 0; // Return 0 if no parameter count is found in the name
        var value = double.Parse(match.Groups[1].Value);
        var unit = match.Groups[3].Value.ToUpper();

        return unit switch
        {
            // Scale based on the unit (B for billion, M for million)
            "B" => value * 1e9,
            "M" => value * 1e6,
            _ => 0 // Return 0 if no parameter count is found in the name
        };
    }

    #endregion

    #region SecurityMethods

    public static string CalculateMd5Hash(this Stream stream)
    {
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(stream);
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

    public static T? Decrypt<T>(byte[] obj, string secretKey)
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