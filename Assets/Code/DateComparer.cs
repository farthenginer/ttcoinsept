public class DateComparer
{
    // İki tarih string'ini karşılaştıran metod
    public int CompareDates(string date1, string date2)
    {
        // Tarihleri ayır (gün ve ay olarak)
        string[] parts1 = date1.Split('.');
        string[] parts2 = date2.Split('.');

        // Gün ve ayları integer olarak dönüştür
        int day1 = int.Parse(parts1[0]);
        int month1 = int.Parse(parts1[1]);

        int day2 = int.Parse(parts2[0]);
        int month2 = int.Parse(parts2[1]);

        // Önce ayları karşılaştır
        if (month1 > month2)
        {
            return 1;
        }
        else if (month1 < month2)
        {
            return -1;
        }
        else
        {
            // Eğer aylar eşitse günleri karşılaştır
            if (day1 > day2)
            {
                return 1;
            }
            else if (day1 < day2)
            {
                return -1;
            }
            else
            {
                return 0; // Aylar ve günler eşit ise
            }
        }
    }
}
