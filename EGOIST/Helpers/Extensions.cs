using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
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

    public static void Notify(NotificationContent content, string areaName = "", TimeSpan? expirationTime = null, Action onClick = null, Action onClose = null, bool CloseOnClick = true, bool ShowXbtn = true, Exception ex = null, LogEventLevel logLevel = default)
    {
        Extensions.Notify(content, areaName, expirationTime, onClick, onClose, CloseOnClick, ShowXbtn);
        Log.Logger.Write(logLevel != default ? logLevel : NotifyToLog(), $"{content.Title}, {content.Message}", ex);

        LogEventLevel NotifyToLog()
        {
            switch (content.Type)
            {
                case NotificationType.None:
                    return LogEventLevel.Debug;
                case NotificationType.Information:
                    return LogEventLevel.Information;
                case NotificationType.Notification:
                    return LogEventLevel.Verbose;
                case NotificationType.Error:
                    return LogEventLevel.Error;
                case NotificationType.Warning:
                    return LogEventLevel.Warning;
            }

            return LogEventLevel.Fatal;
        }
    }
}