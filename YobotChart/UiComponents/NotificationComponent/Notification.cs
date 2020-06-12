using System;
using System.Collections.ObjectModel;
using System.Threading;
using YobotChart.Shared.Win32;

namespace YobotChart.UiComponents.NotificationComponent
{
    public static class Notification
    {
        public static ObservableCollection<NotificationOption> NotificationList { get; } =
            new ObservableCollection<NotificationOption>();

        public static void Show(string content, int seconds = 8, string title = null)
        {
            Execute.ToUiThread(() =>
            {
                NotificationList.Add(new NotificationOption
                {
                    Type = NotificationType.Alert,
                    FadeoutTime = TimeSpan.FromSeconds(seconds),
                    Content = content,
                    Title = title
                });
            });
        }

        public static void Warn(string content, int seconds = Timeout.Infinite, string title = null)
        {
            Execute.ToUiThread(() =>
            {
                NotificationList.Add(new NotificationOption
                {
                    Type = NotificationType.Alert,
                    Level = NotificationLevel.Warn,
                    FadeoutTime = TimeSpan.FromSeconds(seconds),
                    Content = content,
                    Title = title,
                    CloseExplicitly = true
                });
            });
        }

        public static void Error(string content, int seconds = Timeout.Infinite, string title = null)
        {
            Execute.ToUiThread(() =>
            {
                NotificationList.Add(new NotificationOption
                {
                    Type = NotificationType.Alert,
                    Level = NotificationLevel.Error,
                    FadeoutTime = TimeSpan.FromSeconds(seconds),
                    Content = content,
                    Title = title,
                    CloseExplicitly = true
                });
            });
        }

        public static void Push(string content, string title = null)
        {
            Execute.ToUiThread(() =>
            {
                NotificationList?.Add(new NotificationOption
                {
                    Content = content,
                    Title = title
                });
            });
        }

        public static void Push(NotificationOption notification)
        {
            Execute.ToUiThread(() => { NotificationList?.Add(notification); });
        }
    }
}
