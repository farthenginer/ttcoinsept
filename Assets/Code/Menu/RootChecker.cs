using UnityEngine;
using System.Threading.Tasks;

public class RootChecker : MonoBehaviour
{
    public GameObject rootPanel;

    private void Start()
    {
        DetectRootStatus();
    }

    async void DetectRootStatus()
    {
        bool isRooted = IsDeviceRooted();
        try
        {
            int? bannedStatus = await GoogleAuthentication.Instance.GetBanStatus();
            if (isRooted || bannedStatus > 0)
            {
                rootPanel.SetActive(true);
                await GoogleAuthentication.Instance.Ban();
            }

        }
        catch (System.Exception ex)
        {
            Debug.Log("Root Verisi hata verdi" + ex);
            return;
        }
        
    }

    public bool IsDeviceRooted()
    {
        // SU binary kontrolü
        if (CheckSuBinary())
        {
            return true;
        }

        // Root yönetim uygulamalarının kontrolü
        if (CheckRootManagementApps())
        {
            return true;
        }

        // Common root dosyalarının kontrolü
        if (CheckCommonRootFiles())
        {
            return true;
        }

        // Root tespit edilmedi
        return false;
    }

    private bool CheckSuBinary()
    {
        try
        {
            using (AndroidJavaObject process = new AndroidJavaObject("java.lang.ProcessBuilder", new string[] { "sh", "-c", "type su" }))
            {
                process.Call("start");
                AndroidJavaObject inputStream = process.Get<AndroidJavaObject>("inputStream");
                string output = new AndroidJavaObject("java.io.BufferedReader", new AndroidJavaObject("java.io.InputStreamReader", inputStream)).Call<string>("readLine");
                if (!string.IsNullOrEmpty(output) && output.Contains("su"))
                {
                    return true;
                }
            }
        }
        catch (System.Exception)
        {
            // Hata oluştu, rootlu olmayabilir.
        }
        return false;
    }

    private bool CheckRootManagementApps()
    {
        string[] rootApps = {
            "com.noshufou.android.su",
            "com.noshufou.android.su.elite",
            "eu.chainfire.supersu",
            "com.koushikdutta.superuser",
            "com.zachspong.temprootremovejb",
            "com.ramdroid.appquarantine"
        };

        try
        {
            using (AndroidJavaObject packageManager = new AndroidJavaObject("android.content.pm.PackageManager"))
            {
                AndroidJavaObject context = GetUnityActivity().Call<AndroidJavaObject>("getApplicationContext");

                foreach (string app in rootApps)
                {
                    try
                    {
                        AndroidJavaObject appInfo = packageManager.Call<AndroidJavaObject>("getApplicationInfo", context, app, 0);
                        if (appInfo != null)
                        {
                            return true;
                        }
                    }
                    catch (System.Exception)
                    {
                        // Uygulama bulunamadı, rootlu olmayabilir.
                    }
                }
            }
        }
        catch (System.Exception)
        {
            // Hata oluştu, rootlu olmayabilir.
        }

        return false;
    }

    private bool CheckCommonRootFiles()
    {
        string[] rootFiles = {
            "/data/local/su",
            "/data/local/bin/su",
            "/data/local/xbin/su",
            "/sbin/su",
            "/su/bin/su",
            "/system/bin/su",
            "/system/sd/xbin/su",
            "/system/xbin/su",
            "/system/app/Superuser.apk",
            "/system/etc/init.d/99SuperSUDaemon"
        };

        foreach (string file in rootFiles)
        {
            try
            {
                AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", file);
                if (fileObject.Call<bool>("exists"))
                {
                    return true;
                }
            }
            catch (System.Exception)
            {
                // Hata oluştu, rootlu olmayabilir.
            }
        }
        return false;
    }

    private AndroidJavaObject GetUnityActivity()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
}
    