using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Firestore;
using GameVanilla.Core;
using GameVanilla.Game.Common;
using Google;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
public class GoogleAuthentication : MonoBehaviour
{
    public static GoogleAuthentication Instance { get; private set; }

    public string imageURL;
    public TMP_Text userNameTxt, userEmailTxt;
    public Image profilePic;
    public GameObject loginPanel, profilePanel;
    private GoogleSignInConfiguration configuration;

    public string webClientId = "521989196285-cd29il43qdt71jgild6toj91rrinu075.apps.googleusercontent.com";

    public const string removeAdsKey = "RemoveAds";

    public FirebaseAuth auth;
    public FirebaseFirestore db;
    public DatabaseReference databaseReference;

    public GameObject video_Panel;
    public GameObject signIn_Panel;
    public RawImage BLACK_image;

    public VideoPlayer player;
    public RawImage image_vid;

    public GameObject versionError, loading;
    public TMP_Text versionText, alertLabel;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true,
            UseGameSignIn = false,
            RequestEmail = true
        };


        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Exception);
                return;
            }

            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            signIn_Panel.SetActive(false);
            GetVersionFromFirestoreFunc();

        });
        var date = DateTime.Now;
        var hour = date.Hour;
        var min = date.Minute;
        var sec = date.Second;
    }
    public GameObject signButton;


    void Start()
    {
        Time.timeScale = 1;
        // Oyunun başladığı zamanı kaydet
        PlayerPrefs.SetString("StartTime", DateTime.Now.ToString());

    }
    async void GetVersionFromFirestoreFunc()
    {
        int versionFromFirestore = await GetVersionFromFirestore();
        var myVersion = GetApplicationVersionAsInt();
        bool gameStatus = await GetGameStatusFromFirestore();
        versionText.text = $"v.{myVersion} \n © 2024 - TTcoin Games";
        if (myVersion < versionFromFirestore)
        {
            alertLabel.text = "You are currently using an old version. Please update the game to the latest version to continue.";
            signIn_Panel.SetActive(false);
            versionError.SetActive(true);
            return;

        }
        else if (!gameStatus && myVersion < versionFromFirestore)
        {
            alertLabel.text = "Currently the server is under maintenance! please try again later.";
            signIn_Panel.SetActive(false);
            versionError.SetActive(true);
            return;
        }
        else if (myVersion >= versionFromFirestore && gameStatus)
        {
            checkLog();
        }
        
    }

    int GetApplicationVersionAsInt()
    {
        int version = 0;
        if (int.TryParse(Application.version, out version))
        {
            return version;
        }
        else
        {
            Debug.LogError("Application.version is not a valid integer.");
            return -1; // Geçersiz durumda -1 dönebilir
        }
    }
    async Task<int> GetVersionFromFirestore()
    {
        try
        {
            // Firestore'da version/version dökümanına eriş
            DocumentReference docRef = db.Collection("version").Document("version");

            // Dökümanı asenkron olarak al
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // Dökümandaki 'version' alanını güvenli şekilde al
                if (snapshot.TryGetValue<int>("version", out int version))
                {
                    // Elde edilen versiyonu konsola yaz
                    Debug.Log("Current version: " + version);
                    return version;
                }
                else
                {
                    Debug.LogWarning("Version field not found in the document!");
                    return 0; // Versiyon numarası bulunamadığında null dönebilir
                }
            }
            else
            {
                Debug.LogWarning("Version document does not exist!");
                return 0; // Doküman bulunamadığında null dönebilir
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error getting version: " + e.Message);
            return 0; // Hata durumunda null dönebilir
        }
    }
    async Task<bool> GetGameStatusFromFirestore()
    {
        try
        {
            // Firestore'da version/version dökümanına eriş
            DocumentReference docRef = db.Collection("gameConfig").Document("config");

            // Dökümanı asenkron olarak al
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // Dökümandaki 'version' alanını güvenli şekilde al
                if (snapshot.TryGetValue("gameEnabled", out bool version))
                {
                    // Elde edilen versiyonu konsola yaz
                    Debug.Log("Current version: " + version);
                    return version;
                }
                else
                {
                    Debug.LogWarning("Version field not found in the document!");
                    return true; // Versiyon numarası bulunamadığında null dönebilir
                }
            }
            else
            {
                Debug.LogWarning("Version document does not exist!");
                return true; // Doküman bulunamadığında null dönebilir
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error getting version: " + e.Message);
            return true; // Hata durumunda null dönebilir
        }
    }

    public void checkLog()
    {
        if (PlayerPrefs.HasKey("logined"))
        {
            Invoke("OnSignIn", 2);
            signIn_Panel.SetActive(false);
            loading.SetActive(true);
            signButton.SetActive(false);
        }
        else
        {
            signIn_Panel.SetActive(true);
            signButton.SetActive(true);
        }
    }
    public GameObject internetPanel;
    
    public void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
            OnAuthenticationFinished, TaskScheduler.FromCurrentSynchronizationContext());
    }



    string idToken;
    GoogleSignInUser userr;
    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            Debug.LogError("Sign-in failed: " + task.Exception);
            return;
        }
        if (task.IsCanceled)
        {
            Debug.LogError("Sign-in canceled");
            return;
        }

        GoogleSignInUser user = task.Result;
        userr = task.Result;
        Credential credential = GoogleAuthProvider.GetCredential(user.IdToken, null);
        UserData.Instance.userID = task.Result.UserId;
        auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
        {
            if (authTask.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (authTask.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + authTask.Exception);
                return;
            }

            FirebaseUser newUser = authTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
            idToken = task.Result.IdToken;
        });
    }
    private void Update()
    {
        if (!String.IsNullOrEmpty(idToken))
        {
            loading.SetActive(true);
            signIn_Panel.SetActive(false);
            SaveUserToSingleton(userr);
            StartCoroutine(UpdateUI(userr));
            PlayerPrefs.SetInt("logined", 1);
            setSpin();
            // Fetch the total score after signing in
            FetchTotalScoreFromFirestore();
            InitLevelNum();
            idToken = string.Empty;
            return;
        }
    }
    async void setSpin()
    {
        await CheckAndSetSpinVerildi();

    }
    IEnumerator UpdateUI(GoogleSignInUser user)
    {
        Debug.Log("Welcome: " + user.DisplayName + "!");

        userNameTxt.text = user.DisplayName;
        userEmailTxt.text = user.Email;
        imageURL = user.ImageUrl.ToString();

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageURL))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Texture download failed: " + request.error);
                yield break;
            }

            Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);
            Rect rect = new Rect(0, 0, downloadedTexture.width, downloadedTexture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            profilePic.sprite = Sprite.Create(downloadedTexture, rect, pivot);
            CancelInvoke("OnSignIn");
            if (PlayerPrefs.HasKey("logined"))
            {
                Invoke("loadmenu", 1);
            }
            else
            {
                loginPanel.SetActive(false);
                profilePanel.SetActive(true);
            }
        }
    }
    async void loadmenu()
    {
        DateTime? timeFetch = await TimeFetcher.GetInternetDateTimeAsync();
        if (timeFetch != null)
        {
            UserData.Instance.dateTime = timeFetch;
            UserData.Instance.Date = timeFetch?.ToString("dd/MM");
            int live = await GoogleAuthentication.Instance.GetLiveCountFromDB();
            UserData.Instance.LiveCount = live;
            //bool video = await GoogleAuthentication.Instance.CheckVideoBoolJust();
            //UserData.Instance.videoBool = video;
            SceneManager.LoadScene(1);
            return;
        }
    }
    public static event Action OnUserDataLoaded;

    public void SaveUserToSingleton(GoogleSignInUser user)
    {
        // Güncel kullanıcı bilgilerini singleton üzerinden ayarla
        UserData.Instance.UserName = user.DisplayName;
        UserData.Instance.UserEmail = user.Email;
        UserData.Instance.UserImageUrl = user.ImageUrl.ToString();

        // Kullanıcı verilerini Firestore'a kaydet
        Dictionary<string, object> userData = new Dictionary<string, object>
        {
            { "userName", user.DisplayName },
            { "email", user.Email },
            { "userImageUrl", user.ImageUrl.ToString() },
            { "highscore", 0 }, // Varsayılan yüksek puan sıfır
            
        };
        DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);
        userDocRef.SetAsync(userData, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("User data successfully written to Firestore");
                setSpin();

                OnUserDataLoaded?.Invoke();  // Kullanıcı bilgileri yüklendiğinde tetiklenir
            }
            else
            {
                Debug.LogError("Failed to write user data to Firestore: " + task.Exception);
            }
        });
        fullFirstLive();
    }
    async void fullFirstLive()
    {
        DateTime? currentTimee = UserData.Instance.dateTime;
        var currentTimeSTR = currentTimee?.ToString("dd/MM");

        var userRef = databaseReference.Child("users").Child(auth.CurrentUser.UserId);
        var snapshot = await userRef.GetValueAsync();

        if (snapshot.Child("LifeTime").Value == null)
        {
            fulleLiveCount();
            await userRef.Child("LifeTime").SetValueAsync(currentTimeSTR);
        }

    }
    public void SignOut()
    {
        if (auth != null)
        {
            auth.SignOut();
            PlayerPrefs.DeleteAll();
        }
    }

    public void SaveOrUpdateWalletAddress(string walletAddress)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        Dictionary<string, object> walletData = new Dictionary<string, object>
        {
            { "walletAddress", walletAddress }
        };

        db.Collection("users").Document(auth.CurrentUser.UserId).UpdateAsync(walletData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Wallet address successfully updated in Firestore");
                UserData.Instance.WalletAddress = walletAddress;
            }
            else
            {
                Debug.LogError("Failed to update wallet address in Firestore: " + task.Exception);
            }
        });
    }

    public void FetchWalletAddressFromFirestore()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        db.Collection("users").Document(auth.CurrentUser.UserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Dictionary<string, object> userData = snapshot.ToDictionary();
                    if (userData.ContainsKey("walletAddress"))
                    {
                        string walletAddress = userData["walletAddress"].ToString();
                        Debug.Log("Fetched wallet address: " + walletAddress);
                        UserData.Instance.WalletAddress = walletAddress;
                    }
                    else
                    {
                        Debug.LogError("Wallet address field not found in Firestore");
                    }
                }
                else
                {
                    Debug.LogError("User data not found in Firestore");
                }
            }
            else
            {
                Debug.LogError("Failed to fetch user data from Firestore: " + task.Exception);
            }
        });
    }
    public bool fetchWalletStatus()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return false;
        }

        db.Collection("users").Document(auth.CurrentUser.UserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Dictionary<string, object> userData = snapshot.ToDictionary();
                    if (userData.ContainsKey("walletAddress"))
                    {
                        string walletAddress = userData["walletAddress"].ToString();
                        Debug.Log("Fetched wallet address: " + walletAddress);
                        UserData.Instance.WalletAddress = walletAddress;
                        return true;
                    }
                    else
                    {
                        Debug.LogError("Wallet address field not found in Firestore");
                        return false;
                    }

                }
                else
                {
                    Debug.LogError("User data not found in Firestore");
                    return false;

                }

            }
            else
            {
                Debug.LogError("Failed to fetch user data from Firestore: " + task.Exception);
                return false;
            }
        });
        return false;
    }
    public async Task SaveOrUpdateTotalScore(int scoreToAdd)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        DocumentReference userDoc = db.Collection("users").Document(auth.CurrentUser.UserId);

        try
        {
            DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();
            int currentTotalScore = 0;

            if (snapshot.Exists && snapshot.TryGetValue("totalScore", out object totalScoreValue))
            {
                if (totalScoreValue is long)
                {
                    currentTotalScore = (int)(long)totalScoreValue;
                }
                else if (totalScoreValue is int)
                {
                    currentTotalScore = (int)totalScoreValue;
                }
                else
                {
                    Debug.LogError("Unexpected data type for totalScore");
                }
            }

            int newTotalScore = currentTotalScore + scoreToAdd;
            Dictionary<string, object> scoreData = new Dictionary<string, object>
        {
            { "totalScore", newTotalScore }
        };

            await userDoc.UpdateAsync(scoreData);
            Debug.Log("TotalScore successfully updated in Firestore");
            UserData.Instance.TotalScore = newTotalScore;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to update TotalScore in Firestore: " + e);
        }
    }

    public async Task FetchTotalScoreFromFirestore()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            UserData.Instance.TotalScore = 0;
            return;
        }

        DocumentReference userDoc = db.Collection("users").Document(auth.CurrentUser.UserId);

        try
        {
            DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();
            int totalScore = 0;

            if (snapshot.Exists && snapshot.TryGetValue("totalScore", out object totalScoreValue))
            {
                if (totalScoreValue is long)
                {
                    totalScore = (int)(long)totalScoreValue;
                }
                else if (totalScoreValue is int)
                {
                    totalScore = (int)totalScoreValue;
                }
                else
                {
                    Debug.LogError("Unexpected data type for totalScore");
                }
            }

            UserData.Instance.TotalScore = totalScore;
            Debug.Log("Fetched TotalScore: " + totalScore);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to fetch user data from Firestore: " + e);
            UserData.Instance.TotalScore = 0;
        }
    }








    public async Task FetchTTCoinFromFirestore()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        try
        {
            DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();
            int currentTTCoin = 0; // Varsayılan değer 0

            if (snapshot.Exists && snapshot.ContainsField("TTCoin"))
            {
                currentTTCoin = snapshot.GetValue<int>("TTCoin");
            }

            // Kullanıcı TTCoin bilgisini güncelle
            UserData.Instance.TTCoin = currentTTCoin;
            Debug.Log($"Fetched TTCoin: {currentTTCoin}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to fetch TTCoin from Firestore: {e.Message}");
        }
    }


    public async Task SpendTTCoin(int amount)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);
        DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            if (snapshot.TryGetValue<int>("TTCoin", out int currentTTCoin))
            {
                if (currentTTCoin >= amount)
                {
                    currentTTCoin -= amount;

                    Dictionary<string, object> userData = new Dictionary<string, object>
                {
                    { "TTCoin", currentTTCoin }
                };

                    try
                    {
                        await userDocRef.UpdateAsync(userData);
                        UserData.Instance.TTCoin = currentTTCoin;
                        Debug.Log("TTCoin successfully spent and updated in Firestore");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to update TTCoin in Firestore: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning("Not enough TTCoin to spend");
                }
            }
            else
            {
                Debug.LogError("TTCoin does not exist in Firestore.");
            }
        }
        else
        {
            Debug.LogError("Failed to fetch user data from Firestore.");
        }
    }


    public async Task FetchHighScoreFromFirestoreAsync()
    {
        var currentUser = auth.CurrentUser;
        if (currentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        DocumentSnapshot snapshot = await db.Collection("users").Document(currentUser.UserId).GetSnapshotAsync();
        if (snapshot.Exists)
        {
            // Highscore alanını güvenli bir şekilde çekmek için TryGetValue kullanımı
            if (snapshot.TryGetValue("highScore", out long highScoreValue))
            {
                UserData.Instance.HighScore = (int)highScoreValue;
                Debug.Log("Fetched highscore: " + UserData.Instance.HighScore);
            }
            else
            {
                UserData.Instance.HighScore = 0;
                Debug.LogError("Highscore field not found in Firestore");
            }
        }
        else
        {
            UserData.Instance.HighScore = 0;
            Debug.LogError("User data not found in Firestore");
        }
    }



    public async Task UpdateHighScoreInFirestoreAsync(int newScore)
    {
        var currentUser = auth.CurrentUser;
        if (currentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        // Mevcut highscore'u Firestore'dan çek
        DocumentSnapshot snapshot = await db.Collection("users").Document(currentUser.UserId).GetSnapshotAsync();
        int currentHighScore = 0;

        if (snapshot.Exists)
        {
            // Highscore alanını güvenli bir şekilde çekmek için TryGetValue kullanımı
            if (snapshot.TryGetValue("highScore", out long highScoreValue))
            {
                currentHighScore = (int)highScoreValue;
            }
        }

        if (newScore > currentHighScore)
        {
            Dictionary<string, object> scoreData = new Dictionary<string, object>
        {
            { "highScore", newScore }
        };

            await db.Collection("users").Document(currentUser.UserId).UpdateAsync(scoreData);
            UserData.Instance.HighScore = newScore;
            Debug.Log("Highscore successfully updated in Firestore");
        }
    }
    public void NextLevel(bool setLevel, int newLevel)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                int currentLevel = 1; // Varsayılan seviye 1

                if (snapshot.Exists)
                {
                    // level verisini almaya çalış
                    if (snapshot.TryGetValue<int>("level", out int levelValue))
                    {
                        currentLevel = levelValue;
                        if (currentLevel == 0)
                        {
                            currentLevel = 1; // Eğer seviye 0 ise, 1 olarak ayarla
                        }
                    }
                }

                if (setLevel)
                {
                    currentLevel = newLevel;
                }
                else
                {
                    currentLevel++;
                }

                Dictionary<string, object> userData = new Dictionary<string, object>
            {
                { "level", currentLevel }
            };

                userDocRef.UpdateAsync(userData).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompleted)
                    {
                        Debug.Log("Level successfully updated in Firestore");
                        UserData.Instance.UserLevel = currentLevel; // UserData sınıfını güncelle
                    }
                    else
                    {
                        Debug.LogError("Failed to update level in Firestore: " + updateTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to fetch user data from Firestore: " + task.Exception);
            }
        });
    }

    public async Task FetchLevelFromFirestore()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        try
        {
            DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);
            DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();

            int currentLevel = 1; // Varsayılan seviye 1

            if (snapshot.Exists)
            {
                if (snapshot.TryGetValue<int>("level", out int levelValue))
                {
                    currentLevel = levelValue;
                    if (currentLevel == 0)
                    {
                        currentLevel = 1; // Eğer seviye 0 ise, 1 olarak ayarla
                    }
                }
            }

            Debug.Log($"Fetched level: {currentLevel}");

            // UserData sınıfını güncelle
            UserData.Instance.UserLevel = currentLevel;
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to fetch user data from Firestore: " + ex.Message);
        }
    }

    public async Task<List<UserData>> FetchTop100Users()
    {
        Firebase.Firestore.Query usersQuery = db.Collection("users").OrderByDescending("totalScore").Limit(100);
        QuerySnapshot snapshot = await usersQuery.GetSnapshotAsync();

        List<UserData> userList = new List<UserData>();
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (document.Exists)
            {
                UserData userData = new UserData();

                if (document.TryGetValue("userName", out string userName))
                {
                    userData.UserName = userName;
                }
                else
                {
                    userData.UserName = "Unknown";
                }

                if (document.TryGetValue("userImageUrl", out string userImageUrl))
                {
                    userData.UserImageUrl = userImageUrl;
                }
                else
                {
                    userData.UserImageUrl = "";
                }

                if (document.TryGetValue("level", out long levelValue))
                {
                    userData.UserLevel = Convert.ToInt32(levelValue);
                }
                else
                {
                    userData.UserLevel = 0;
                }

                if (document.TryGetValue("totalScore", out long totalScoreValue))
                {
                    userData.TotalScore = Convert.ToInt32(totalScoreValue);
                }
                else
                {
                    userData.TotalScore = 0;
                }

                userList.Add(userData);
            }
        }

        return userList.OrderByDescending(user => user.TotalScore).ToList();
    }
    public async Task SetRemoveAdsStatus(bool status)
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            
            var userDocRef = FirebaseFirestore.DefaultInstance.Collection("users").Document(user.UserId);
            
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { removeAdsKey, status }
            };
            await userDocRef.UpdateAsync(updates);
            await GetRemoveAdsStatus();
        }
    }

    // RemoveAds değerini Firestore'dan alma fonksiyonu
    public async Task<bool> GetRemoveAdsStatus()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            var userDocRef = FirebaseFirestore.DefaultInstance.Collection("users").Document(user.UserId);
            var snapshot = await userDocRef.GetSnapshotAsync();
            if (snapshot.Exists && snapshot.TryGetValue(removeAdsKey, out bool removeAdsStatus))
            {
                if (!removeAdsStatus)
                {
                    await SaveTTCoin(100);
                }
                return removeAdsStatus;
            }
        }
        return false; // Varsayılan değer false
    }
    public async Task SaveTTCoin(int amount)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        try
        {
            // Firestore'daki mevcut TTCoin değerine amount kadar ekle
            await userDocRef.UpdateAsync(new Dictionary<string, object>
        {
            { "TTCoin", FieldValue.Increment(amount) }
        });

            // UserData'daki TTCoin bilgisini güncelle
            UserData.Instance.TTCoin += amount;

            Debug.Log($"Added TTCoin: {amount}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to add TTCoin to Firestore: {e.Message}");
        }
    }
    public async Task<bool> DeleteUserAccount()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            try
            {
                // Kullanıcının Firestore'daki verilerini sil
                var userDocRef = FirebaseFirestore.DefaultInstance.Collection("users").Document(user.UserId);
                await userDocRef.DeleteAsync();

                // Firebase Authentication'dan kullanıcıyı sil
                await user.DeleteAsync();

                Debug.Log("Kullanıcı hesap verileri başarıyla silindi.");
                return true;
            }
            catch (FirebaseException e)
            {
                Debug.LogError("Kullanıcı hesap verilerini silerken hata: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("Geçerli kullanıcı bulunamadı.");
        }

        return false;
    }

    public async Task CreateWithdrawalRequestAsync(float amount, string walletAddress, string userLevel)
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            DocumentReference docRef = db.Collection("requests").Document();
            Dictionary<string, object> request = new Dictionary<string, object>
        {
            { "userId", user.UserId },
            { "userName", user.DisplayName },
            { "amount", amount },
            { "status", "pending" },
            { "walletAddress", walletAddress },
            { "sentDate", DateTime.Now.ToString("dd/MM/yyyy") },
            { "userLevel", userLevel },
            { "email", user.Email}
        };
            try
            {
                await SpendTTCoin((int)amount);
                await docRef.SetAsync(request);
                Debug.Log("Request successfully added to Firestore!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to add request: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("User is not signed in.");
        }
    }

    public async Task<QuerySnapshot> GetUsersSnapshot()
    {
        CollectionReference usersRef = db.Collection("users");
        Firebase.Firestore.Query query = usersRef.OrderByDescending("totalScore").Limit(50);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return snapshot;
    }
    public void OnSignOut()
    {
        userNameTxt.text = "";
        userEmailTxt.text = "";
        imageURL = "";
        loginPanel.SetActive(true);
        profilePanel.SetActive(false);
        Debug.Log("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();
        auth.SignOut();
    }
    List<DocumentSnapshot> snaps;

    public async Task<List<Dictionary<string, object>>> FetchRequestsForUser()
    {
        var userId = auth.CurrentUser.UserId;
        List<Dictionary<string, object>> requests = new List<Dictionary<string, object>>();

        try
        {
            QuerySnapshot snapshot = await db.Collection("requests")
                                              .WhereEqualTo("userId", userId)
                                              .GetSnapshotAsync();

            Debug.Log("Number of documents found: " + snapshot.Documents);

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Dictionary<string, object> requestData = document.ToDictionary();
                    requests.Add(requestData);
                    Debug.Log("Document data: " + string.Join(", ", requestData.Select(kv => kv.Key + "=" + kv.Value.ToString())));
                }
                else
                {
                    Debug.Log("Document does not exist: " + document.Id);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error fetching requests: " + e.Message);
        }

        return requests;
    }
    public async Task<List<Dictionary<string, object>>> FetchResultsForUser()
    {
        var userId = auth.CurrentUser.UserId;
        List<Dictionary<string, object>> requests = new List<Dictionary<string, object>>();

        try
        {
            QuerySnapshot snapshot = await db.Collection("results")
                                              .WhereEqualTo("userId", userId)
                                              .GetSnapshotAsync();

            Debug.Log("Number of documents found: " + snapshot.Documents);

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Dictionary<string, object> requestData = document.ToDictionary();
                    requests.Add(requestData);
                    Debug.Log("Document data: " + string.Join(", ", requestData.Select(kv => kv.Key + "=" + kv.Value.ToString())));
                }
                else
                {
                    Debug.Log("Document does not exist: " + document.Id);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error fetching requests: " + e.Message);
        }

        return requests;
    }
    public async Task<bool> CheckWalletStatus()
    {
        try
        {
            // gameConfig koleksiyonundaki config dokümanını al
            DocumentSnapshot doc = await db.Collection("gameConfig").Document("config").GetSnapshotAsync();

            if (doc.Exists)
            {
                bool walletStatus = doc.ContainsField("wallet") && doc.GetValue<bool>("wallet");
                Debug.Log("Wallet Status: " + walletStatus);
                return walletStatus;
            }
            else
            {
                Debug.Log("No such document!");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error getting document: " + e.Message);
            return false;
        }
    }
    #region spin hakkı
    public async Task CheckAndSetSpinVerildi()
    {
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        var userDocRef = db.Collection("users").Document(userId);
        var snapshot = await userDocRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            var userData = snapshot.ToDictionary();

            if (!userData.ContainsKey("spinVerildi"))
            {
                // spinVerildi alanı yoksa ekle ve false olarak ayarla
                await userDocRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "spinVerildi", false }
                });
            }

            else if (userData.ContainsKey("spinVerildi") && snapshot.GetValue<bool>("spinVerildi") == false)
            {
                // İlgili işlemleri burada gerçekleştir
                Debug.Log("Spin işlemleri gerçekleştiriliyor...");
                await InitializeUserSpinData(userDocRef);
                // spinVerildi değerini true olarak güncelle
                await userDocRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "spinVerildi", true }
                });
            }
            else
            {
                Debug.Log("Spin işlemleri daha önce gerçekleştirilmiş.");
            }
        }
        else
        {
            // Kullanıcı verisi yoksa oluştur ve spinVerildi değerini false olarak ayarla
            var newUserData = new Dictionary<string, object>
            {
                { "spinVerildi", false }
            };
            await userDocRef.SetAsync(newUserData, SetOptions.MergeAll);
            Debug.Log("Kullanıcı verisi oluşturuldu ve spinVerildi false olarak ayarlandı.");
        }
    }

    public async Task<bool> CanUserSpin()
    {
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        var userDocRef = FirebaseFirestore.DefaultInstance.Collection("users").Document(userId);
        var snapshot = await userDocRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            var userSpinData = snapshot.ToDictionary();
            if (userSpinData != null)
            {
                // Spin haklarını kontrol et
                int weeklySpins = userSpinData.ContainsKey("WeeklySpins") ? (int)(long)userSpinData["WeeklySpins"] : 0;
                if (weeklySpins < 2)
                {
                    DateTime lastSpinDate = userSpinData.ContainsKey("LastSpinDate") ? DateTime.Parse((string)userSpinData["LastSpinDate"]) : DateTime.MinValue;
                    DateTime currentDate = DateTime.UtcNow;

                    // Eğer spin tarihlerinin haftası aynı ise
                    if (GetIso8601WeekOfYear(lastSpinDate) == GetIso8601WeekOfYear(currentDate))
                    {
                        // Kullanıcının spin hakkı var mı kontrol et
                        if (weeklySpins < 2)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // Yeni bir hafta başladı, spin hakkını sıfırla
                        await ResetWeeklySpins(userDocRef);
                        return true;
                    }
                }
            }
        }
        else
        {
            // Kullanıcı verisi yoksa, yeni veri oluştur
            await InitializeUserSpinData(userDocRef);
            return true;
        }

        return false;
    }


    public async Task Spin()
    {
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        var userDocRef = FirebaseFirestore.DefaultInstance.Collection("users").Document(userId);
        var snapshot = await userDocRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            var userSpinData = snapshot.ToDictionary();
            int weeklySpins = userSpinData.ContainsKey("WeeklySpins") ? (int)(long)userSpinData["WeeklySpins"] : 0;
            weeklySpins++;

            var updates = new Dictionary<string, object>
        {
            { "WeeklySpins", weeklySpins },
            { "LastSpinDate", DateTime.UtcNow.ToString("o") }
        };

            await userDocRef.UpdateAsync(updates);
        }
    }

    public async Task<DateTime> getSpinData()
    {
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        var userDocRef = FirebaseFirestore.DefaultInstance.Collection("users").Document(userId);
        var snapshot = await userDocRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            var userSpinData = snapshot.ToDictionary();
            string lastDateSTR;
            if (snapshot.TryGetValue("LastSpinDate", out lastDateSTR))
            {
                var lastSTR = DateTime.Parse(lastDateSTR);
                return lastSTR;
            }
            else
            {
                string format = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
                DateTime lastSTR;
                    DateTime.TryParseExact(lastDateSTR, format, null, System.Globalization.DateTimeStyles.RoundtripKind, out lastSTR);
                return lastSTR;
            }
        }
        else
        {
            return DateTime.Now;
        }
    }
    private async Task InitializeUserSpinData(DocumentReference userDocRef)
    {
        // Burada sadece spin ile ilgili verileri güncelliyoruz, diğer veriler korunuyor
        await userDocRef.UpdateAsync(new Dictionary<string, object>
        {
            { "WeeklySpins", 0 },
            { "LastSpinDate", DateTime.UtcNow.ToString("o") }
        });
    }
    private async Task ResetWeeklySpins(DocumentReference userDocRef)
    {
        
        await userDocRef.UpdateAsync(new Dictionary<string, object>
        {
            { "WeeklySpins", 0 },
            { "LastSpinDate", DateTime.UtcNow.ToString("o") }
        });
    }

    // ISO 8601 haftası hesaplama
    public static int GetIso8601WeekOfYear(DateTime time)
    {
        DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        {
            time = time.AddDays(3);
        }

        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
    public async Task<int> GetRemainingSpins()
    {
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        var userDocRef = db.Collection("users").Document(userId);
        var snapshot = await userDocRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            if (snapshot.ToDictionary() != null)
            {
                string ld;
                    snapshot.TryGetValue("LastSpinDate", out ld);
                DateTime lastSpinDate = DateTime.Parse(ld);
                DateTime currentDate = DateTime.UtcNow;

                if (GetIso8601WeekOfYear(lastSpinDate) == GetIso8601WeekOfYear(currentDate))
                {
                    int val;
                    snapshot.TryGetValue("WeeklySpins", out val);
                    return 2 - val;
                }
                else
                {
                    // Yeni bir hafta başladı, spin hakkını sıfırla
                    await  ResetWeeklySpins(userDocRef);
                    return 2;
                }
            }
        }
        else
        {
            // Kullanıcı verisi yoksa, yeni veri oluştur
            await InitializeUserSpinData(userDocRef);
            return 2; // Yeni kullanıcıya iki hak ver
        }

        return 0;
    }
    #endregion
    public async Task<bool> CheckAndUpdateWatchVideo()
    {
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        var userDocRef = FirebaseFirestore.DefaultInstance.Collection("users").Document(userId);
        var snapshot = await userDocRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            var userData = snapshot.ToDictionary();

            // Eğer watchVideo veya LastWatchVideoDate alanı mevcut değilse, oluştur
            if (!userData.ContainsKey("watchVideo") || !userData.ContainsKey("LastWatchVideoDate"))
            {
                // Yeni kullanıcı için başlangıç verisi oluştur
                userData["watchVideo"] = false; // İlk başta video izleme izni verilmez
                userData["LastWatchVideoDate"] = DateTime.UtcNow.ToString("o");
                await userDocRef.SetAsync(userData); // Belgeyi baştan sona yazar
                return true; // İlk kez oluşturulduğunda video izlenemez
            }

            DateTime lastWatchVideoDate = DateTime.Parse((string)userData["LastWatchVideoDate"]);
            DateTime currentDate = DateTime.UtcNow;
            DateTime nextEligibleDate = lastWatchVideoDate.Date.AddDays(1); // Ertesi günün tarihi

            if (currentDate.Date >= nextEligibleDate)
            {
                // Ertesi gün gelmişse, watchVideo değerini true olarak güncelle
                var updateData = new Dictionary<string, object>
            {
                { "watchVideo", false },
                { "LastWatchVideoDate", currentDate.ToString("o") }
            };
                await userDocRef.UpdateAsync(updateData); // Sadece belirtilen alanları günceller
                return true; // Video izleme hakkı verildi
            }
            else
            {
                // Aynı gün içinde video izlenemez
                return false;

            }
            
        }

        else
        {
            // Kullanıcı verisi yoksa oluştur ve video izleme iznini false olarak ayarla
            var newUserData = new Dictionary<string, object>
        {
            { "watchVideo", false },
            { "LastWatchVideoDate", DateTime.UtcNow.ToString("o") }
        };
            await userDocRef.SetAsync(newUserData); // Belgeyi baştan sona yazar
            return true; // Video izleme izni verilmedi
        }
    }
    public async Task<string> checkVideoRemain()
    {
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        var userDocRef = FirebaseFirestore.DefaultInstance.Collection("users").Document(userId);
        var snapshot = await userDocRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            var userData = snapshot.ToDictionary();

            // Eğer watchVideo veya LastWatchVideoDate alanı mevcut değilse, oluştur
            if (!userData.ContainsKey("watchVideo") || !userData.ContainsKey("LastWatchVideoDate"))
            {
                // Yeni kullanıcı için başlangıç verisi oluştur
                // Belgeyi baştan sona yazar
                return "1"; // İlk kez oluşturulduğunda video izlenemez
            }

            DateTime lastWatchVideoDate = DateTime.Parse((string)userData["LastWatchVideoDate"]);
            DateTime? currentDate = UserData.Instance.dateTime;
            DateTime nextEligibleDate = lastWatchVideoDate.Date.AddDays(1); // Ertesi günün tarihi

            if (currentDate?.Date >= nextEligibleDate)
            {
                // Ertesi gün gelmişse, watchVideo değerini true olarak güncelle
                
                return "1"; // Video izleme hakkı verildi
            }
            else
            {
                // Aynı gün içinde video izlenemez
                return "0";

            }

        }

        else
        {
            // Kullanıcı verisi yoksa oluştur ve video izleme iznini false olarak ayarla
            
            return "1"; // Video izleme izni verilmedi
        }
    }

    public void ContinueButton()
    {
        SceneManager.LoadScene(1);
    }
    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("video");
    }
    public void goToPrivacyPolicy()
    {
        Application.OpenURL("https://sites.google.com/view/ttcoin-tc/en");
    }
    private string googlePlayUrl = "https://play.google.com/store/apps/details?id=com.ttcoin.sweet";

    public void OpenGooglePlayPage()
    {
        // URL'yi varsayılan tarayıcıda aç
        Application.OpenURL(googlePlayUrl);
    }
    #region checkLivenumberAndLiveNextTime
    public async Task saveLevelNum(int amount)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        try
        {
            // Firestore'daki mevcut TTCoin değerine amount kadar ekle
            await userDocRef.UpdateAsync(new Dictionary<string, object>
        {
            { "LevelNum", amount}
        });

            // UserData'daki TTCoin bilgisini güncelle
            UserData.Instance.LevelNum = amount;

            Debug.Log($"Added LevelNum: {amount}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to add TTCoin to Firestore: {e.Message}");
        }
    }

    private async Task InitLevelNum()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DocumentReference docRef = db.Collection("users").Document(userId);

        // Firestore'daki belgeyi alıyoruz
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            // Belirli bir değeri kontrol etmek için TryGetValue kullanıyoruz
            if (snapshot.TryGetValue("LevelNum", out int levelNum))
            {
                Debug.Log("levelNum already exists: " + levelNum);
            }
            else
            {
                // Eğer levelNum yoksa, varsayılan değerle ekliyoruz
                await docRef.UpdateAsync("LevelNum", 5);
                Debug.Log("levelNum was missing and has been set to 1.");
            }
        }
    }




    public async Task FetchLevelNumFromFirestore()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        try
        {
            DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();
            int LevelNum = 5;
            if (snapshot.Exists && snapshot.ContainsField("LevelNum"))
            {
                snapshot.TryGetValue("LevelNum", out LevelNum);
            }

            // Kullanıcı TTCoin bilgisini güncelle
            UserData.Instance.LevelNum = LevelNum;
            Debug.Log($"Fetched LevelNum: {LevelNum}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to fetch LevelNum from Firestore: {e.Message}");
        }
    }
    public async Task<bool> CheckIfLevelNumExistsInFirestore()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return false;
        }

        DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        try
        {
            DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();

            if (snapshot.Exists && snapshot.ContainsField("LevelNum"))
            {
                int LevelNum = snapshot.GetValue<int>("LevelNum");
                UserData.Instance.LevelNum = LevelNum;
                Debug.Log($"Fetched LevelNum: {LevelNum}");
                return true;
            }
            else
            {
                Debug.Log("LevelNum field does not exist.");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to fetch LevelNum from Firestore: {e.Message}");
            return false;
        }
    }


    public async Task SpendLevelNum(int amount)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);
        DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            if (snapshot.TryGetValue<int>("LevelNum", out int LevelNum))
            {
                if (LevelNum >= amount)
                {
                    LevelNum -= amount;

                    Dictionary<string, object> userData = new Dictionary<string, object>
                {
                    { "LevelNum", LevelNum }
                };

                    try
                    {
                        await userDocRef.UpdateAsync(userData);
                        UserData.Instance.LevelNum = LevelNum;
                        Debug.Log("TTCoin successfully spent and updated in Firestore");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to update TTCoin in Firestore: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning("Not enough TTCoin to spend");
                }
            }
            else
            {
                Debug.LogError("TTCoin does not exist in Firestore.");
            }
        }
        else
        {
            Debug.LogError("Failed to fetch user data from Firestore.");
        }
    }
    ////////////
    public async void SaveNextTimeToFirestore(DateTime nextTime)
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DocumentReference docRef = FirebaseFirestore.DefaultInstance.Collection("users").Document(userId);

        Dictionary<string, object> userData = new Dictionary<string, object>
    {
        { "NextTime", nextTime.ToString("O") }
    };

        await docRef.SetAsync(userData, SetOptions.MergeAll);
    }


    
    public async Task<bool> IsNextTimeExistsInFirestore()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DocumentReference docRef = FirebaseFirestore.DefaultInstance.Collection("users").Document(userId);

        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        // Eğer doküman varsa ve "NextTime" alanı mevcutsa, true döndür
        if (snapshot.Exists && snapshot.ContainsField("NextTime"))
        {
            return true;
        }

        // Aksi takdirde, false döndür
        return false;
    }



public async Task SpendNextTime(string amount)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("Şu anda oturum açmış bir kullanıcı yok.");
            return;
        }

        DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);
        DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            if (snapshot.TryGetValue<string>("NextTime", out string nextTime))
            {
                // Burada `nextTime` ile `amount` arasında bir işlem yapılabilir.
                // Eğer `NextTime` bir tarih veya zaman ise bu işlemi uygun şekilde gerçekleştir.

                // Örneğin, amount değeri eklenecek veya çıkarılacak bir zaman dilimini temsil ediyorsa:
                // DateTime updatedTime = DateTime.Parse(nextTime).AddMinutes(double.Parse(amount));
                // nextTime = updatedTime.ToString("o");

                Dictionary<string, object> userData = new Dictionary<string, object>
            {
                { "NextTime", nextTime }
            };

                try
                {
                    await userDocRef.UpdateAsync(userData);
                    UserData.Instance.NextTime = nextTime;
                    Debug.Log("NextTime successfully spent and updated in Firestore");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to update NextTime in Firestore: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("NextTime does not exist in Firestore.");
            }
        }
        else
        {
            Debug.LogError("Failed to fetch user data from Firestore.");
        }
    }

    #endregion
    #region billing
    public async Task SavePurchase(string ID, string CODE)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("Şu anda oturum açmış bir kullanıcı yok.");
            return;
        }

        // E-posta adresi özel karakterler içerebilir; bu yüzden güvenli bir şekilde oluşturulmalı
        string userId = auth.CurrentUser.UserId;  // Daha güvenli bir kullanıcı kimliği kullanın
        string newDocName = $"{userId}/{UnityEngine.Random.Range(0, 100000)}";

        // Belge referansını oluşturun
        DocumentReference userDocRef = db.Collection("purchases").Document(newDocName);

        try
        {
            // Belgeyi oluştur veya güncelle
            await userDocRef.SetAsync(new Dictionary<string, object>
            {
                { "itemID", ID },
                { "orderCode", CODE },
                { "userName", auth.CurrentUser.DisplayName ?? "Unknown" },
                { "purchaseDate", System.DateTime.UtcNow.ToString("o") } // UTC formatında tarih
            });

            Debug.Log("Satın alma bilgileri başarıyla kaydedildi.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Satın alma bilgilerini Firestore'a kaydetme başarısız: {e.Message}");
        }
    }
    public async Task<int> GetLiveCountFromDB()
    {
        if (auth.CurrentUser == null)
        {
            Debug.Log("Kullanıcı yok");
            return 0;
        }

        var userRef = databaseReference.Child("users").Child(auth.CurrentUser.UserId);
        var snapshot = await userRef.GetValueAsync();

        if (snapshot.Exists)
        {
            if (snapshot.Child("LiveCount").Exists && snapshot.Child("LiveCount").Value != null)
            {
                int levelNum = int.Parse(snapshot.Child("LiveCount").Value.ToString());
                Debug.Log("LiveCount already exists: " + levelNum);
                return levelNum;
            }
            else
            {
                await userRef.Child("LiveCount").SetValueAsync(5);
                Debug.Log("LiveCount was missing and has been set to 5.");
                return 5;
            }
        }
        else
        {
            return 0;
        }
    }
    ////////////
    public async void SaveLiveCount(bool remove)
    {
        int current = await GetLiveCountFromDB();
        int newLive = remove ? current - 1 : current + 1;

        var userRef = databaseReference.Child("users").Child(auth.CurrentUser.UserId);
        await userRef.Child("LiveCount").SetValueAsync(newLive);
    }
    public async void fulleLiveCount()
    {
        var userRef = databaseReference.Child("users").Child(auth.CurrentUser.UserId);
        await userRef.Child("LiveCount").SetValueAsync(5);
    }
    public async Task CheckAndGetOrSaveLifeTime()
    {
        DateComparer comparer = new DateComparer();
        DateTime? currentTimee = UserData.Instance.dateTime;
        var currentTimeSTR = currentTimee?.ToString("dd/MM");

        var userRef = databaseReference.Child("users").Child(auth.CurrentUser.UserId);
        var snapshot = await userRef.GetValueAsync();

        if (snapshot.Exists)
        {
            if (snapshot.Child("LifeTime").Value != null)
            {
                string lifeTime = snapshot.Child("LifeTime").Value.ToString();
                int result = comparer.CompareDates(currentTimeSTR, lifeTime);
                if (result > 0)
                {
                    fulleLiveCount();
                    PlayerPrefs.DeleteKey("notif");
                    RemoveAllNotifications();
                    await userRef.Child("LifeTime").SetValueAsync(currentTimeSTR);
                }
            }
            else
            {
                await userRef.Child("LifeTime").SetValueAsync(currentTimeSTR);
            }
        }
    }
    public void RemoveAllNotifications()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // AndroidJavaClass ve AndroidJavaObject kullanarak tüm bildirimleri kaldırma
        AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass notificationManagerClass = new AndroidJavaClass("android.app.NotificationManager");
        AndroidJavaObject notificationManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "notification");

        if (notificationManager != null)
        {
            // Tüm bildirimleri kaldır
            notificationManager.Call("cancelAll");
        }
        else
        {
            Debug.Log("NotificationManager null.");
        }
#endif
    }
        public async Task<bool> CheckVideoBool()
    {
        DateComparer comparer = new DateComparer();
        DateTime? currentTimee = UserData.Instance.dateTime;
        var currentTimeSTR = currentTimee?.ToString("dd/MM");
        DocumentReference docRef = db.Collection("users").Document(auth.CurrentUser.UserId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            if (snapshot.TryGetValue("videoTime", out string lifeTime))
            {
                int result = comparer.CompareDates(currentTimeSTR, lifeTime);
                if (result > 0)
                {
                    await docRef.UpdateAsync("videoTime", currentTimeSTR);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {

                await docRef.UpdateAsync("videoTime", currentTimeSTR);
                return true;
            }
        }
        else
        {
            return false;
        }
    }
    public async Task<bool> CheckVideoBoolJust()
    {
        DateComparer comparer = new DateComparer();
        DateTime? currentTimee = UserData.Instance.dateTime;
        var currentTimeSTR = currentTimee?.ToString("dd/MM");
        DocumentReference docRef = db.Collection("users").Document(auth.CurrentUser.UserId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            if (snapshot.TryGetValue("videoTime", out object videoTimeValue))
            {
                string lifeTime;

                lifeTime = videoTimeValue.ToString();

                int result = comparer.CompareDates(currentTimeSTR, lifeTime);
                if (result > 0)
                {
                    //await docRef.UpdateAsync("videoTime", currentTimeSTR);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // Eğer "videoTime" alanı yoksa veya değeri alınamıyorsa
                //await docRef.UpdateAsync("videoTime", currentTimeSTR);
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    public async Task<DateTime> GetTomorrowDate()
    {
        // Bugünkü tarihi al
        DateTime? today = UserData.Instance.dateTime;
        

        // Yarının tarihini al
        DateTime? tomorrow = today?.AddDays(1);

       
        return (DateTime)tomorrow;
        
        
    }

    public async Task<string> GetTomorrowDateAsString()
    {
        DateTime tomorrow = await GetTomorrowDate();

        // Tarihi "dd/MM" formatında döndür
        return tomorrow.ToString("dd/MM");
    }
    
    public async Task<string> GetNextLiveDateForUI()
    {
        var userRef = databaseReference.Child("users").Child(auth.CurrentUser.UserId);
        var snapshot = await userRef.GetValueAsync();
        if (snapshot.Exists)
        {
            if (snapshot.Child("LifeTime").Exists && snapshot.Child("LifeTime").Value != null)
            {
                string lifeTime = snapshot.Child("LifeTime").Value.ToString();
                string result = ConvertDateToDayMonth(lifeTime);
                return result;
            }
            else
            {
                string time = await GetTomorrowDateAsString();
                return ConvertDateToDayMonth(time);
            }
        }
        else
        {
            return null;
        }
    }
    public static string ConvertDateToDayMonth(string isoDateString)
    {
        // ISO 8601 formatındaki string'i DateTime türüne çeviriyoruz
        DateTime dateTime = DateTime.Parse(isoDateString);
        var tom = dateTime.AddDays(1);
        // Gün ve Ay formatında bir string oluşturuyoruz
        string formattedDate = $"{tom.Day:00}/{tom.Month:00}";

        return formattedDate;
    }

    #endregion
    #region banProcessor
    public async Task Ban()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return;
        }

        DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);
        DocumentReference blockedDocRef = db.Collection("Blocked").Document(auth.CurrentUser.UserId);

        try
        {
            // Kullanıcının mevcut verilerini al
            DocumentSnapshot userSnapshot = await userDocRef.GetSnapshotAsync();
            if (userSnapshot.Exists)
            {
                Dictionary<string, object> userData = userSnapshot.ToDictionary();

                // Firestore'daki mevcut TTCoin değerine amount kadar ekle
                await userDocRef.UpdateAsync(new Dictionary<string, object>
            {
                { "Banned", 1}
            });

                // Kullanıcı verilerini "Blocked" koleksiyonuna kopyala
                await blockedDocRef.SetAsync(userData);

                Debug.Log("User data successfully copied to Blocked collection.");
            }
            else
            {
                Debug.LogError("User document does not exist.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to ban user or copy data: {e.Message}");
        }
    }
    public async Task<int?> GetBanStatus()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user currently signed in.");
            return null;
        }

        DocumentReference userDocRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        try
        {
            DocumentSnapshot userSnapshot = await userDocRef.GetSnapshotAsync();
            if (userSnapshot.Exists)
            {
                // "Banned" alanını int olarak çek
                if (userSnapshot.TryGetValue("Banned", out int bannedStatus))
                {
                    Debug.Log($"Banned status: {bannedStatus}");
                    return bannedStatus;
                }
                else
                {
                    Debug.LogWarning("Banned field does not exist.");
                    return null;
                }
            }
            else
            {
                Debug.LogError("User document does not exist.");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get Banned status: {e.Message}");
            return null;
        }
    }

    #endregion
}

[FirestoreData]
public class SpinData
{
    [FirestoreProperty]
    public int WeeklySpins { get; set; }

    [FirestoreProperty]
    public string LastSpinDate { get; set; }
}
