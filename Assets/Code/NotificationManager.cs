using UnityEngine;
using System;
using UnityEngine.Android;
using Unity.Notifications.Android;

public class NotificationScheduler : MonoBehaviour
{
    
    private void Start()
    {
        reqAuth();
        registerNotificationsChannel();
    }
    public void reqAuth()
    {
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
    }

    public void registerNotificationsChannel()
    {
        var channel = new AndroidNotificationChannel
        {
            Id = "default_channel",
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "Full Lives"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }
    public void sendNotification(string title, string text)
    {
        DateTime now = DateTime.Now;

        // Ertesi günün tarihi
        DateTime tomorrow = now.AddDays(1);

        // Ertesi günün saatini 00:00 olarak ayarla
        DateTime tomorrowMidnight = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 0, 0, 0);

        var notification = new AndroidNotification();
        notification.Title = title;
        notification.Text = text;
        notification.FireTime = tomorrowMidnight;
        notification.SmallIcon = "icon_0";
        notification.LargeIcon = "icon_1";
        AndroidNotificationCenter.SendNotification(notification, "default_channel");
    }
    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            AndroidNotificationCenter.CancelAllNotifications();
            sendNotification("YOUR LIVES ARE FULL!", "Your lives are full, match candies and earn TTcoin!");
        }
    }
    
}

