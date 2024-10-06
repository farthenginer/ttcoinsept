using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Linq;
public class ScoreboardManager : MonoBehaviour
{
    public async Task<List<UserData>> FetchTop10Users()
    {
        
        List<UserData> top50Users = new List<UserData>();

        // Kullanıcıların verilerini al
        var usersSnapshot = await GoogleAuthentication.Instance.GetUsersSnapshot();

        // Null kontrolü
        if (usersSnapshot == null || usersSnapshot.Documents == null)
        {
            Debug.LogError("usersSnapshot veya usersSnapshot.Documents null değer döndü.");
            return GenerateRandomUsers(50); // Rastgele kullanıcılar döndürüyoruz
        }

        foreach (var document in usersSnapshot.Documents)
        {
            if (document == null)
            {
                Debug.LogWarning("Belge null, bu kullanıcı atlanacak.");
                continue;
            }

            // UserData nesnesini oluştur
            var user = new UserData();

            // Alanları kontrol et ve değerlerini ata
            user.UserName = document.TryGetValue("userName", out string userName) ? userName : "Unknown";
            user.UserImageUrl = document.TryGetValue("userImageUrl", out string userImageUrl) ? userImageUrl : "";
            user.UserLevel = document.TryGetValue("level", out int userLevel) ? userLevel : 0;
            user.TotalScore = document.TryGetValue("totalScore", out int totalScore) ? totalScore : 0;

            top50Users.Add(user);
        }

        // Skorları azalan sıraya göre sırala ve ilk 10'u al
        top50Users = top50Users.OrderByDescending(u => u.TotalScore).Take(50).ToList();

        // Eğer liste 10'dan az kullanıcı içeriyorsa rastgele kullanıcılarla tamamla
        if (top50Users.Count < 50)
        {
            int remainingUsers = 50 - top50Users.Count;
            top50Users.AddRange(GenerateRandomUsers(remainingUsers));
        }

        return top50Users;
         
    }

    private List<UserData> GenerateRandomUsers(int count)
    {
        List<UserData> randomUsers = new List<UserData>();
        string[] randomNames = { "Player1", "Player2", "Player3", "Player4", "Player5", "Player6", "Player7", "Player8", "Player9", "Player10" };

        for (int i = 0; i < count; i++)
        {
            var user = new UserData
            {
                UserName = randomNames[Random.Range(0, randomNames.Length)],
                UserImageUrl = "", // Rastgele resim URL'si ekleyebilirsiniz
                UserLevel = Random.Range(1, 101), // Rastgele bir seviyeye ayarla
                TotalScore = Random.Range(0, 1001) // Rastgele bir skor ayarla
            };
            randomUsers.Add(user);
        }

        return randomUsers;
    }


}
