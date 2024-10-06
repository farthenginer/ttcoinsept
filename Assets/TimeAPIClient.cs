using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
public static class TimeAPIClient
{
    private static readonly HttpClient client = new HttpClient();

    public static async  Task<DateTime> GetServerTimeAsync()
    {
        try
        {
            string output = await ExtractDateTimeString();
            DateTime now = new DateTime();
            now = DateTime.Parse(output);
            return now; // Burada sunucu yanıtını döndürmelisiniz (örneğin: "2024-08-09T12:34:56Z")
        }
        catch (Exception ex)
        {
            // Hata durumunda log veya hata yönetimi
            Console.WriteLine($"Sunucu zamanı alınırken hata oluştu: {ex.Message}");
            return DateTime.Now;
        }
    }
    public  static async Task<string> ExtractDateTimeString()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // API isteği gönder
                string url = "https://www.timeapi.io/api/Time/current/zone?timeZone=Europe/Istanbul";
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Yanıtı JSON string olarak al
                string jsonString = await response.Content.ReadAsStringAsync();

                // JSON verisini ayrıştır
                JObject jsonObject = JObject.Parse(jsonString);

                // 'dateTime' değerini al
                string dateTimeString = (string)jsonObject["dateTime"];

                // Tarih ve saat formatını belirle
                DateTime dateTime = DateTime.Parse(dateTimeString);

                // İstenilen formatta string'e dönüştür

                string formattedDate = dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");

                // Sonucu ekrana yazdır
                return formattedDate; // ISO 8601 formatında string döndürme

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return null;
            }
        }
    }

    
}
