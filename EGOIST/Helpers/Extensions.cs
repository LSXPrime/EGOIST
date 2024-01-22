using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using EGOIST.Data;
using Newtonsoft.Json;
using Notification.Wpf;
using Serilog;
using Serilog.Events;
using Wpf.Ui.Controls;

namespace EGOIST.Helpers;
public static class Extensions
{
    #region UIStatics

    public static SnackbarPresenter? SnackbarArea;
    public static ContentPresenter? ContentArea;
    private static readonly NotificationManager notification = new();

    #endregion

    #region Events
    public delegate void SaveDataEvent();
    public static SaveDataEvent onSaveData;

    public static void SaveData()
    {
        if (onSaveData != null)
            onSaveData();
    }

    public delegate void LoadDataEvent();
    public static LoadDataEvent onLoadData;

    public static void LoadData()
    {
        if (onLoadData != null)
            onLoadData();
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

        double megabytes = (double)bytes / megabyte;
        return megabytes;
    }

    public static double BytesToGB(this long bytes)
    {
        const double gigabyte = 1024 * 1024 * 1024;

        double gigabytes = (double)bytes / gigabyte;
        return gigabytes;
    }
    #endregion

    #region UIHandlingMethods
    public static void Notify(string title, string message, NotificationType type = NotificationType.None, TimeSpan? expirationTime = null, Action onClick = null, Action onClose = null, bool CloseOnClick = true, bool ShowXbtn = true, Exception ex = null, LogEventLevel logLevel = default)
    {
        Notify(new NotificationContent { Title = title, Message = message, Type = NotificationType.Information }, "NotificationArea", expirationTime, onClick, onClose, CloseOnClick, ShowXbtn);
    }

    public static void Notify(NotificationContent content, string areaName = "", TimeSpan? expirationTime = null, Action onClick = null, Action onClose = null, bool CloseOnClick = true, bool ShowXbtn = true, Exception ex = null, LogEventLevel logLevel = default)
    {
        notification.Show(content, areaName, expirationTime, onClick, onClose, CloseOnClick, ShowXbtn);
        Log.Logger.Write(logLevel != default ? logLevel : NotifyToLog(), $"{content.Title}, {content.Message}", ex);

        LogEventLevel NotifyToLog()
        {
            return content.Type switch
            {
                NotificationType.None => LogEventLevel.Debug,
                NotificationType.Information => LogEventLevel.Information,
                NotificationType.Notification => LogEventLevel.Verbose,
                NotificationType.Error => LogEventLevel.Error,
                NotificationType.Warning => LogEventLevel.Warning,
                _ => LogEventLevel.Fatal,
            };
        }
    }
    #endregion

    #region SecurityMethods
    public static string CalculateMD5Hash(this Stream stream)
    {
        using var md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }


    public static byte[] Encrypt<T>(T obj)
    {
        byte[] data;
        using var stream = new MemoryStream();
        var json = JsonConvert.SerializeObject(obj);
        stream.Write(Encoding.UTF8.GetBytes(json));
        data = stream.ToArray();

        var key = MD5.HashData(Encoding.UTF8.GetBytes(AppConfig.Instance.DataSecretKey));
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

    public static T Decrypt<T>(byte[] obj)
    {
        var key = MD5.HashData(Encoding.UTF8.GetBytes(AppConfig.Instance.DataSecretKey));
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

        return JsonConvert.DeserializeObject<T>(json);
    }

    #endregion
}