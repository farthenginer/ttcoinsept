using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Firebase.Extensions;
using System.Threading.Tasks;

public class WalletSender : MonoBehaviour
{
    public RectTransform requestContainer; // Taleplerin gösterileceği UI container
    public GameObject requestPrefab; // Talepler için prefab

    private FirebaseFirestore db;
    private float padding = 10f; // Ögeler arasındaki boşluk (opsiyonel)

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance; // Firestore instance'ını doğru alalım
        InvokeRepeating("GetRequests", 1, 3);
    }

    async void GetRequests()
    {
        List<Dictionary<string, object>> requests = await GoogleAuthentication.Instance.FetchRequestsForUser();
        List<Dictionary<string, object>> results = await GoogleAuthentication.Instance.FetchResultsForUser();
        DisplayRequests(requests, results);
    }

    void DisplayRequests(List<Dictionary<string, object>> documents, List<Dictionary<string, object>> results)
    {
        // İlk önce eski içerikleri temizle
        foreach (Transform child in requestContainer)
        {
            Destroy(child.gameObject);
        }

        // İçerik için yükseklik hesapla
        

        // Container yüksekliğini güncelle

        // Talepleri ekle
        foreach (var request in documents)
        {
            // Prefab oluşturma ve doldurma işlemleri burada yapılacak
            GameObject newRequest = Instantiate(requestPrefab, requestContainer.transform);

            if (request.TryGetValue("sentDate", out object sentDate))
                newRequest.transform.Find("WithdrawalDateText").GetComponent<TMPro.TMP_Text>().text = "Request Date: " + sentDate.ToString();

            if (request.TryGetValue("amount", out object amount))
                newRequest.transform.Find("AmountText").GetComponent<TMPro.TMP_Text>().text = "TTcoin: " + amount.ToString();

            if (request.TryGetValue("status", out object status))
                newRequest.transform.Find("StatusText").GetComponent<TMPro.TMP_Text>().text = "Status: " + status.ToString();
            newRequest.transform.Find("StatusText").GetComponent<TMPro.TMP_Text>().color = Color.yellow;
            newRequest.transform.Find("Level").GetComponent<TMPro.TMP_Text>().text = "Level: " + UserData.Instance.UserLevel.ToString();
            // Her yeni ögeyi doğru pozisyona yerleştir
            
            // Yeni öğeyi içeriğe ekledikten sonra doğru pozisyona yerleştirin
        }
        foreach (var request in results)
        {
            // Prefab oluşturma ve doldurma işlemleri burada yapılacak
            GameObject newRequest = Instantiate(requestPrefab, requestContainer.transform);

            if (request.TryGetValue("sentDate", out object sentDate))
                newRequest.transform.Find("WithdrawalDateText").GetComponent<TMPro.TMP_Text>().text = "Request Date: " + sentDate.ToString();

            if (request.TryGetValue("amount", out object amount))
                newRequest.transform.Find("AmountText").GetComponent<TMPro.TMP_Text>().text = "TTcoin: " + amount.ToString();

            if (request.TryGetValue("status", out object status))
                newRequest.transform.Find("StatusText").GetComponent<TMPro.TMP_Text>().text = "Status: " + status.ToString();
            newRequest.transform.Find("StatusText").GetComponent<TMPro.TMP_Text>().color = Color.white;

            newRequest.transform.Find("Level").GetComponent<TMPro.TMP_Text>().text = "Level: " + UserData.Instance.UserLevel.ToString();
            // Her yeni ögeyi doğru pozisyona yerleştir
            
            // Yeni öğeyi içeriğe ekledikten sonra doğru pozisyona yerleştirin
        }


    }
}

[FirestoreData]
public class WithdrawalRequestData
{
    [FirestoreProperty]
    public string sentDate { get; set; }

    [FirestoreProperty]
    public int amount { get; set; } 

    [FirestoreProperty]
    public string status { get; set; }
    [FirestoreProperty]
    public string userId { get; set; }
    [FirestoreProperty]
    public string userLevel { get; set; }
    [FirestoreProperty]
    public string userName { get; set; }
    [FirestoreProperty]
    public string walletAddress { get; set; }


}
