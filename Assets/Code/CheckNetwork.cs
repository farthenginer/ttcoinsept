using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckNetwork : MonoBehaviour
{
    public GameObject failpanel, alerconnect;
    public bool islogger;
    private void Start()
    {
        InvokeRepeating("check", 1, 3);
    }
    void check()
    {
        failpanel.SetActive(false);
        bool status = CheckInternetConnection();

        if (status)
        {
            
            return;
        }
        else
        {
            CancelInvoke("check");
            failpanel.SetActive(true);
            alerconnect.GetComponent<Animator>().Play("CheckIn");
        }
    }
    public void tryButton()
    {
        InvokeRepeating("check", 0.01f, 3);


    }
    public bool CheckInternetConnection()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            //ShowWarning("İnternet bağlantınız zayıf veya kesildi.");
            return false;
        }
        else
        {
            //HideWarning();
            return true;
        }
    }
}
