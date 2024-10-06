using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameVanilla.Game.Popups;
public class ApplovinAds : MonoBehaviour
{
    public static ApplovinAds Instance;

    public string InterstitialID = "";
    public string RewardVideoID = "";

    void Awake()
    {

        Instance = this;

    }

    void Start()
    {

        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => {


        };

        MaxSdk.InitializeSdk();
        InitializeInterstitialAds();
        InitializeRewardedAds();

    }

    #region Interstitial

    void InitializeInterstitialAds()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        LoadInterstitial();

    }


    private void LoadInterstitial()
    {
        MaxSdk.LoadInterstitial(InterstitialID);
    }


    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        Debug.Log("Ads failed to load");
        LoadInterstitial();
    }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        LoadInterstitial();
    }

    private void OnInterstitialDisplayedEvent(string arg1, MaxSdkBase.AdInfo arg2)
    {

        Debug.Log("Ads Showing successfull");
        LoadInterstitial();

    }


    public void ShowInterstitial()
    {
        if (MaxSdk.IsInterstitialReady(InterstitialID))
        {
            MaxSdk.ShowInterstitial(InterstitialID);
        }
    }

    #endregion



    #region Rewarding Video
    void InitializeRewardedAds()
    {
        // Attach callback
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

        // Load the first rewarded ad
        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(RewardVideoID);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {

    }

    private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {

    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        LoadRewardedAd();
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        LoadRewardedAd();
    }

    private async void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        //ödğllendir
        var Refiller = FindObjectOfType<BuyLivesPopup>().GetComponent<BuyLivesPopup>();
        Refiller.refillWithThridyParty();
        UserData.Instance.videoBool = false;
        await GoogleAuthentication.Instance.CheckVideoBool();
        Debug.Log("Oyuncu Ödüllendirildi");
    }

    public void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }


    public void ShowVideo()
    {
        if (MaxSdk.IsRewardedAdReady(RewardVideoID))
        {
            MaxSdk.ShowRewardedAd(RewardVideoID);
        }
    }

    #endregion

}