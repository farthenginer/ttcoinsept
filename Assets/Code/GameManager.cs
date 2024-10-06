using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void SaveWalletAddress(string walletAddress)
    {
        GoogleAuthentication.Instance.SaveOrUpdateWalletAddress(walletAddress);
    }

    public void FetchWalletAddress()
    {
        GoogleAuthentication.Instance.FetchWalletAddressFromFirestore();
    }
    public bool checkWalletAdress()
    {
        return GoogleAuthentication.Instance.fetchWalletStatus();
    }


    public async void FetchTTCoin()
    {
        await GoogleAuthentication.Instance.FetchTTCoinFromFirestore();
    }

}
