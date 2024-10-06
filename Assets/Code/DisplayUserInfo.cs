using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System;
public class DisplayUserInfo : MonoBehaviour
{
    public TMP_Text userNameTxt;
    public TMP_Text totalCoinTxt;
    public TMP_Text highScoreTxt;
    public TMP_Text timeSpentText;
    public TMP_Text ttcoin;
    public Image profilePic;

    void OnEnable()
    {
        GoogleAuthentication.OnUserDataLoaded += UpdateUserInfo;
    }

    void OnDisable()
    {
        GoogleAuthentication.OnUserDataLoaded -= UpdateUserInfo;
    }

    void Start()
    {
        InvokeRepeating("UpdateUserInfo", 1, 5);
    }
    public async void UpdateUserInfo()
    {
        await FetchUserData();
        // Kullanıcı bilgilerini Singleton'dan çek ve UI'ı güncelle
        
    }
    public void updateLevel()
    {
        GoogleAuthentication.Instance.NextLevel(false,0);
    }
    public async Task FetchUserData()
    {
        await GoogleAuthentication.Instance.FetchTotalScoreFromFirestore();
        await GoogleAuthentication.Instance.FetchHighScoreFromFirestoreAsync();
        await GoogleAuthentication.Instance.FetchLevelFromFirestore();
        await GoogleAuthentication.Instance.FetchTTCoinFromFirestore();

        userNameTxt.text = UserData.Instance.UserName;
        totalCoinTxt.text = $"Total Score: {UserData.Instance.TotalScore}";
        highScoreTxt.text = $"High Score: {UserData.Instance.HighScore}";
        ttcoin.text = $"TTcoin: {UserData.Instance.TTCoin}";
        StartCoroutine(LoadProfileImage(UserData.Instance.UserImageUrl));
    }

    private void FixedUpdate()
    {
        string startTimeString = PlayerPrefs.GetString("StartTime", string.Empty);

        if (!string.IsNullOrEmpty(startTimeString))
        {
            DateTime startTime = DateTime.Parse(startTimeString);
            TimeSpan timeSpent = DateTime.Now - startTime;

            // Geçen süreyi saat, dakika ve saniye cinsinden hesapla
            int hours = (int)timeSpent.TotalHours;
            int minutes = timeSpent.Minutes;
            int seconds = timeSpent.Seconds;

            // TextMeshPro'ya yaz
            timeSpentText.text = string.Format("Time Spent : {0} saat {1} dakika {2} saniye", hours, minutes, seconds);
        }
    }
    IEnumerator LoadProfileImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);
            Rect rect = new Rect(0, 0, downloadedTexture.width, downloadedTexture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            profilePic.sprite = Sprite.Create(downloadedTexture, rect, pivot);
        }
    }

}
