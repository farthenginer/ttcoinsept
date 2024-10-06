using UnityEngine;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
public static class TimeFetcher
{
    // WorldClockAPI URL
    private static string timeApiUrl = "https://www.timeapi.io/api/Time/current/zone?timeZone=Europe/Istanbul";

    // Task ile internetten DateTime dönen fonksiyon
    public static async Task<DateTime?> GetInternetDateTimeAsync()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string jsonResult = await client.GetStringAsync(timeApiUrl);
                DateTime? dateTime = ParseDateTime(jsonResult);
                return dateTime;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error fetching time: " + ex.Message);
                return null;
            }
        }
    }

    private static DateTime? ParseDateTime(string json)
    {
        try
        {
            // JSON verisini C# modeline dönüştür
            DateTimeInfo dateTimeInfo = JsonConvert.DeserializeObject<DateTimeInfo>(json);

            if (dateTimeInfo != null)
            {
                // DateTime nesnesini oluştur
                DateTime dateTime = new DateTime(
                    dateTimeInfo.year,
                    dateTimeInfo.month,
                    dateTimeInfo.day,
                    dateTimeInfo.hour,
                    dateTimeInfo.minute,
                    dateTimeInfo.seconds,
                    dateTimeInfo.milliSeconds
                );
                return dateTime;
            }
            else
            {
                Debug.LogError("Failed to parse DateTimeInfo.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing datetime: " + ex.Message);
            return null;
        }
    }
}
public class DateTimeInfo
{
    public int year { get; set; }
    public int month { get; set; }
    public int day { get; set; }
    public int hour { get; set; }
    public int minute { get; set; }
    public int seconds { get; set; }
    public int milliSeconds { get; set; }
    public string dateTime { get; set; }
    public string date { get; set; }
    public string time { get; set; }
    public string timeZone { get; set; }
    public string dayOfWeek { get; set; }
    public bool dstActive { get; set; }
}

