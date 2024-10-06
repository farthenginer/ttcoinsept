using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternetLevelController : MonoBehaviour
{
    public GameObject canvasFailed, internetPanel;

    private void Start()
    {
        InvokeRepeating("checkReachable", 0, 3);
    }
    void checkReachable()
    {
        bool internetReached = FindObjectOfType<CheckNetwork>().GetComponent<CheckNetwork>().CheckInternetConnection();
        if (!internetReached)
        {
            CancelInvoke("checkReachable");
            canvasFailed.SetActive(true);
            internetPanel.GetComponent<Animator>().Play("CheckIn");
        }
        else
        {
            canvasFailed.SetActive(false);
            InvokeRepeating("checkReachable", 0, 3);
        }
    }
    public void TryAgainButton()
    {
        internetPanel.GetComponent<Animator>().Play("CheckIn");
        checkReachable();
    }
}
