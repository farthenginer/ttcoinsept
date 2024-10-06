using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using Google;
using Firebase.Extensions;
using GameVanilla.Core;
using GameVanilla.Game.Popups;
using GameVanilla.Game.UI;
public class MenuManager : MonoBehaviour
{
    public List<GameObject> allPanels;
    public GameObject gamePanel, walletPanel, profilePanel, scoreboardPanel, shopPanel, withdrawalPanel, alertPanel, quitPanel, internetPanel;
    public GameObject normalCanvas, LoadCanvas;
    public TMPro.TMP_Text walletTTCoin;
    public Button QuitAccesButton;
    public string walletHelpUrl = "https://guide.tscscan.com/sweet/";
    private void Awake()
    {
        closeAllPanel();
        openPanel(gamePanel);
        //InvokeRepeating("checkReachable", 0, 3);
    }
    public void closeAllPanel()
    {
        foreach (var item in allPanels)
        {
            item.SetActive(false);
        }
    }
    public void openPanel(GameObject panel)
    {
        panel.SetActive(true);
    }
    
    #region buttons
    public async void WalletButton()
    {
        bool status = await GoogleAuthentication.Instance.CheckWalletStatus();
        int level = UserData.Instance.UserLevel;
        await GoogleAuthentication.Instance.FetchTTCoinFromFirestore();
        int ttcoin = UserData.Instance.TTCoin;
        if (status && level > 100)
        {
            closeAllPanel();
            openPanel(walletPanel);
            walletTTCoin.text = "TTCoin: " + ttcoin;
        }
        else
        {
            closeAllPanel();
            openPanel(alertPanel);
        }
    }
    
    public async void profileButton()
    {
        LoadCanvas.SetActive(true);
        normalCanvas.SetActive(false);
        var dui = FindObjectOfType<DisplayUserInfo>().gameObject;
        await dui.GetComponent<DisplayUserInfo>().FetchUserData();
        LoadCanvas.SetActive(false);
        normalCanvas.SetActive(true);
        closeAllPanel();
        openPanel(profilePanel);
    }
    public void scoreBoardButton()
    {
       
        closeAllPanel();
        openPanel(scoreboardPanel);
    }
    public void exitButton()
    {
        closeAllPanel();
        openPanel(gamePanel);
    }
    public void Logout()
    {
        // Firebase Sign-Out
        
        closeAllPanel();
        openPanel(quitPanel);
        setQuitAlerProperties("Are you sure you want to log out? (After this process, please restart the game!)");
        var pop = FindObjectOfType<quitter>().GetComponent<quitter>();
        QuitAccesButton.onClick.RemoveAllListeners();
        QuitAccesButton.onClick.AddListener(pop.SignOut);
    }
    public void outOturum()
    {
        Debug.Log("Firebase SignOut completed");
        GoogleSignIn.DefaultInstance.SignOut();

        // Google Sign-Out
        PlayerPrefs.DeleteKey("logined");
    }
    #endregion
    #region wallet
    public TMPro.TMP_InputField walletAdressField;
    public TMPro.TMP_Text walletAdressError;
    bool sendPer = true;
    public async void SendWalletAdressAndSpentTTCoin()
    {
        if (sendPer)
        {
            sendPer = false;
            await GoogleAuthentication.Instance.FetchTTCoinFromFirestore();
            string walletAdress = walletAdressField.text;
            int ttc = UserData.Instance.TTCoin;
            if (walletAdress.Length > 0 && ttc >= 3000 && walletAdress.StartsWith("0x"))
            {
                await GoogleAuthentication.Instance.CreateWithdrawalRequestAsync(UserData.Instance.TTCoin, walletAdress, UserData.Instance.UserLevel.ToString());
                WalletButton();
            }
            else 
            {
                walletAdressError.text = "Invalid address or insufficient TTCoin (minimum amount to withdraw is 3000 TTC)";
                Invoke("ErrorReset", 2);
            }
            sendPer = true;
        }
    }
    void ErrorReset()
    {
        walletAdressError.text = "";
    }
    public void withdrawPanel()
    {
        closeAllPanel();
        openPanel(withdrawalPanel);
    }
    #endregion
    public void shopButton()
    {
        gamePanel.SetActive(false);
        shopPanel.SetActive(true);
    }
    public void exitShopButton()
    {
        gamePanel.SetActive(true);
        shopPanel.SetActive(false);
    }
    public void deleteAllUserData()
    {
        closeAllPanel();
        openPanel(quitPanel);
        setQuitAlerProperties("You will lose all your data! (This action cannot be undone.) (After this process, please restart the game!)");
        QuitAccesButton.onClick.RemoveAllListeners();
        var pop = FindObjectOfType<quitter>().GetComponent<quitter>();
        QuitAccesButton.onClick.AddListener(pop.DeleteUser);
    }
    public async void delete()
    {
        await GoogleAuthentication.Instance.DeleteUserAccount();
        PlayerPrefs.DeleteAll();
    }
    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("video");
    }
    public TMPro.TMP_Text alerTextQuit;
    void setQuitAlerProperties(string text)
    {
        alerTextQuit.text = text;
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void OpenEmailClient()
    {
        string email = "help@ttcoin.info"; // Gönderilecek e-posta adresi
        string subject = "Enter title"; // E-posta konusu
        string body = "Hello dear TTCoin officials,"; // E-posta gövdesi

        using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
        using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent", "android.intent.action.SEND"))
        {
            intentObject.Call<AndroidJavaObject>("setType", "text/plain");
            intentObject.Call<AndroidJavaObject>("putExtra", "android.intent.extra.EMAIL", new string[] { email });
            intentObject.Call<AndroidJavaObject>("putExtra", "android.intent.extra.SUBJECT", subject);
            intentObject.Call<AndroidJavaObject>("putExtra", "android.intent.extra.TEXT", body);

            // Gmail uygulamasını hedefleme
            string packageName = "com.google.android.gm"; // Gmail'in paket adı
            using (AndroidJavaClass packageManager = new AndroidJavaClass("android.content.pm.PackageManager"))
            using (AndroidJavaObject currentActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
            {
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.SEND");
                intent.Call<AndroidJavaObject>("setPackage", packageName);
                intent.Call<AndroidJavaObject>("setType", "text/plain");
                intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.EMAIL", new string[] { email });
                intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.SUBJECT", subject);
                intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.TEXT", body);

                currentActivity.Call("startActivity", intent);
            }
        }
    }
    public void openWalletHelp()
    {
        Application.OpenURL(walletHelpUrl);
    }
}
