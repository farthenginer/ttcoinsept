using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Purchasing;
using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;
using Unity.Services.Core;
using System.Threading.Tasks;

[Serializable]
public class NonConsumableItem
{
    public string Name;
    public string Id;
    public string desc;
    public float price;
}
[Serializable]
public class TTCoin1500
{
    public string Name;
    public string Id;
    public string desc;
}
    public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    IStoreController m_StoreContoller;
    public TMP_Text itemBuyButtonText;
    public NonConsumableItem ncItem;

    public Image PurchaseButton;
    public Sprite buttonPink;
    public Sprite buttonGrey;
    public TTCoin1500 ttc;

        private async void Start()
    {
        
        await UnityServices.InitializeAsync();
        SetupBuilder();
        await setText();
    }
    public async Task setText()
    {
        bool status = await GoogleAuthentication.Instance.GetRemoveAdsStatus();
        if (status)
        {
            itemBuyButtonText.text = "Purchased";
            PurchaseButton.sprite = buttonGrey;
        }
        else
        {
            itemBuyButtonText.text = "14.99$";
            PurchaseButton.sprite = buttonPink;
        }
    }

    #region setup and initialize
    void SetupBuilder()
    {

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(ncItem.Id, ProductType.NonConsumable);
        builder.AddProduct(ttc.Id, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        print("Success");
        m_StoreContoller = controller;
        CheckNonConsumable(ncItem.Id);
    }
    #endregion


    #region button clicks 
    public async void NonConsumable_Btn_Pressed()
    {
        bool removeAdAlreadyPurchased = await GoogleAuthentication.Instance.GetRemoveAdsStatus();
        if (removeAdAlreadyPurchased)
        {
            return;
        }
        else
        {
            m_StoreContoller.InitiatePurchase(ncItem.Id);
        }
    }
    public void TTCButtonPressed()
    {
        m_StoreContoller.InitiatePurchase(ttc.Id);
    }
    #endregion

    #region main
    //processing purchase
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        //Retrive the purchased product
        var product = args.purchasedProduct;

        print("Purchase Complete" + product.definition.id);

        if (product.definition.id == ncItem.Id)//non consumable
        {
            RemoveAds();
        }

        else if (product.definition.id == ttc.Id)
        {
            SetTTCoin();
        }
        // Satın alım başarılı olduğunda burası tetiklenir

        // Sipariş bilgilerine erişim
        string transactionID = args.purchasedProduct.transactionID;
        string orderID = args.purchasedProduct.receipt;
        savePurchases(transactionID, orderID);

        return PurchaseProcessingResult.Complete;
    }
    #endregion
    

    #region functions
    public async void RemoveAds()
    {
        await GoogleAuthentication.Instance.SetRemoveAdsStatus(true);

    }
    public async void SetTTCoin()
    {
        await GoogleAuthentication.Instance.SaveTTCoin(1500);
    }
    #endregion


    void CheckNonConsumable(string id)
    {
        if (m_StoreContoller != null)
        {
            var product = m_StoreContoller.products.WithID(id);
            if (product != null)
            {
                if (product.hasReceipt)//purchased
                {
                    //alınmış
                    RemoveAds();
                }
                else
                {
                    //alınmamış
                }
            }
        }
    }


    #region error handeling
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        print("failed" + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        print("initialize failed" + error + message);
    }

    

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        print("purchase failed" + failureReason);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        print("purchase failed" + failureDescription);
    }
    public async void savePurchases(string id, string code)
    {
        await setText();

        await GoogleAuthentication.Instance.SavePurchase(id, code);
    }
    #endregion
}


