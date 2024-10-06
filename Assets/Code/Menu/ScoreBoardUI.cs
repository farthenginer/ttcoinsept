using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections.Generic;

public class ScoreboardUI : MonoBehaviour
{
    public List<TMP_Text> usernames, levels, totalscores;
    public List<Image> userPics;
    public GameObject back;
    public Transform container;
    public List<string> urls;
    public GameObject load, main;

    List<UserData> top10Users;
    private void Start()
    {
        load.SetActive(true);
        main.SetActive(false);
        StartGM();
    }
    private async void StartGM()
    {
        var scrb = FindObjectOfType<ScoreboardManager>();
        top10Users = await scrb.GetComponent<ScoreboardManager>().FetchTop10Users();
        for (int i = 0; i < 47; i++)
        {
            var obj = Instantiate(back, container);
            var madal = obj.transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>();
            usernames.Add(obj.transform.GetChild(2).GetComponent<TMP_Text>());
            madal.text = $"{i + 4}";
            levels.Add(obj.transform.GetChild(3).GetComponent<TMP_Text>());
            totalscores.Add(obj.transform.GetChild(4).GetComponent<TMP_Text>());
            userPics.Add(obj.transform.GetChild(1).GetComponent<Image>());
        }
        load.SetActive(false);
        main.SetActive(true);
        InvokeRepeating("updateuI", 1, 5); 
    }

    void updateuI()
    {
         UpdateScoreboardUI();
    }

    private async void UpdateScoreboardUI()
    {
        // Verileri al
       

        if (top10Users == null)
        {
            Debug.LogError("top10Users listesi null.");
            Application.Quit();
            return;
        }

        for (int i = 0; i < top10Users.Count; i++)
        {
            if (usernames != null && usernames[i] != null)
            {
                usernames[i].text = top10Users[i].UserName;
            }

            if (levels != null && levels[i] != null)
            {
                levels[i].text = top10Users[i].UserLevel.ToString();
            }

            if (totalscores != null && totalscores[i] != null)
            {
                totalscores[i].text = top10Users[i].TotalScore.ToString();
            }

            if (userPics != null && userPics.Count > i && userPics[i] != null)
            {
                urls.Add(top10Users[i].UserImageUrl);
            }
        }

        // Paralel görsel yükleme
        await LoadAllProfileImagesAsync();
    }

    private async Task LoadAllProfileImagesAsync()
    {
        List<Task> imageLoadTasks = new List<Task>();

        for (int i = 0; i < urls.Count; i++)
        {
            int index = i;
            imageLoadTasks.Add(Task.Run(() =>
            {
                StartCoroutine(LoadProfileImage(urls[index], userPics[index]));
            }));
        }

        await Task.WhenAll(imageLoadTasks);
    }

    private IEnumerator LoadProfileImage(string url, Image image)
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
            image.sprite = Sprite.Create(downloadedTexture, rect, pivot);
        }
    }
}
