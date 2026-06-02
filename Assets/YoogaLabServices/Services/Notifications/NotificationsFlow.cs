using System.Collections;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

using UnityEngine;

namespace ServicesPackage
{
    public class NotificationsFlow
    {
        private readonly NotificationsContext ctx;
        private MonoBehaviour runner;

        public NotificationsFlow(NotificationsContext context)
        {
            ctx = context;
            SignalBusService.Subscribe<ScheduleNotificationSignal>(OnScheduleNotification);
        }

        private bool CheckAndroidPermissionGranted()
        {
#if UNITY_ANDROID
            if (Application.platform != RuntimePlatform.Android)
                return false;

            int sdkInt = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
            if (sdkInt < 33)
                return true; // Automatically granted pre-Android 13

            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass contextCompat = new AndroidJavaClass("androidx.core.content.ContextCompat"))
            {
                string permission = "android.permission.POST_NOTIFICATIONS";
                int permissionCheck = contextCompat.CallStatic<int>("checkSelfPermission", currentActivity, permission);
                return permissionCheck == 0;
            }
#else
    return false;
#endif
        }        

        public void Initialize(SessionManagerContext sessionManagerContext)
        {
            if (sessionManagerContext.IsFirstSession) return;
#if UNITY_ANDROID
            if (PlayerPrefs.GetInt(NotificationsContext.ANDROID_PERMISSION_KEY, 0) == 1)
            {
                ServicesLogger.Log("[NotificationPermissionManager] Android permission already requested. Skipping.");
                return;
            }
#elif UNITY_IOS
            if (PlayerPrefs.GetInt(NotificationsContext.IOS_PERMISSION_KEY, 0) == 1)
            {
                ServicesLogger.Log("[NotificationPermissionManager] iOS permission already requested. Skipping.");
                return;
            }
#endif
            RequestNotificationPermission();
        }

        private void OnScheduleNotification(ScheduleNotificationSignal sig)
        {
            ScheduleLocalNotification(sig._name, sig._description, sig._fireInterval);
        }

        private void RequestNotificationPermission()
        {
#if UNITY_ANDROID
            RequestAndroidNotificationPermission();
#elif UNITY_IOS
           ServiceCoroutineRunner.StartSafe(RequestiOSNotificationPermission());
#endif
        }

        private void RequestAndroidNotificationPermission()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    int sdkInt = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");

                    if (sdkInt >= 33)
                    {
                        string permission = "android.permission.POST_NOTIFICATIONS";

                        using (AndroidJavaClass contextCompat = new AndroidJavaClass("androidx.core.content.ContextCompat"))
                        {
                            int permissionCheck = contextCompat.CallStatic<int>("checkSelfPermission", currentActivity, permission);

                            if (permissionCheck != 0)
                            {
                                using (AndroidJavaClass activityCompat = new AndroidJavaClass("androidx.core.app.ActivityCompat"))
                                {
                                    activityCompat.CallStatic(
                                        "requestPermissions",
                                        currentActivity,
                                        new string[] { permission },
                                        101
                                    );
                                }
                            }
                        }
                    }
                }

                PlayerPrefs.SetInt(NotificationsContext.ANDROID_PERMISSION_KEY, 1);
                PlayerPrefs.Save();

                ctx.permissionGranted = CheckAndroidPermissionGranted();
            }
        }

#if UNITY_IOS
        private IEnumerator RequestiOSNotificationPermission()
        {
            using (var req = new AuthorizationRequest(
                AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound,
                true))
            {
                while (!req.IsFinished)
                    yield return null;

                var settings = iOSNotificationCenter.GetNotificationSettings();

                ctx.permissionGranted =
          settings.AuthorizationStatus == AuthorizationStatus.Authorized ||
          settings.AuthorizationStatus == AuthorizationStatus.Provisional;

                ServicesLogger.Log($"[iOS] Permission granted: {req.Granted}");
                ServicesLogger.Log($"[iOS] Authorization status: {settings.AuthorizationStatus}");

                PlayerPrefs.SetInt(NotificationsContext.IOS_PERMISSION_KEY, 1);
                PlayerPrefs.Save();
            }
        }
#endif

        public void ScheduleLocalNotification(string title, string message, int delaySeconds)
        {
#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel()
            {
                Id = "local_channel",
                Name = "Local Notifications",
                Importance = Importance.High,
                Description = "Local reminders"
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);

            var notification = new AndroidNotification()
            {
                Title = title,
                Text = message,
                FireTime = System.DateTime.Now.AddSeconds(delaySeconds)
            };
            AndroidNotificationCenter.SendNotification(notification, "local_channel");
#endif

#if UNITY_IOS
            ServiceCoroutineRunner.StartSafe(ScheduleIOSNotification(title, message, delaySeconds));
#endif
        }

#if UNITY_IOS
        private IEnumerator ScheduleIOSNotification(string title, string message, int delaySeconds)
        {
            using (var req = new AuthorizationRequest(
                AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound,
                false))
            {
                while (!req.IsFinished)
                    yield return null;

                if (req.Granted)
                {
                    var trigger = new iOSNotificationTimeIntervalTrigger()
                    {
                        TimeInterval = System.TimeSpan.FromSeconds(delaySeconds),
                        Repeats = false
                    };

                    var notification = new iOSNotification()
                    {
                        Identifier = System.Guid.NewGuid().ToString(),
                        Title = title,
                        Body = message,
                        Trigger = trigger,
                        ShowInForeground = true,
                        ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound
                    };

                    iOSNotificationCenter.ScheduleNotification(notification);
                }
            }
        }
#endif
    }
}