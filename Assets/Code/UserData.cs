public class UserData
{
    private static UserData instance;

    public static UserData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new UserData();
            }
            return instance;
        }
    }

    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string UserImageUrl { get; set; }
    public int TTCoin { get; set; }
    public string WalletAddress { get; set; }
    public string SignUpDate { get; set; }
    public int TotalScore { get; set; }  // TotalScore özelliği eklendi
    public int HighScore { get; set; }
    public int UserLevel { get; set; }
    public string userID { get; set; }
    public string NextTime { get; set; }
    public int LevelNum { get; set; }
    public int LiveCount { get; set; }
    public bool videoBool { get; set; }
    public string Date { get; set; }
    public System.DateTime? dateTime { get; set; }
}
