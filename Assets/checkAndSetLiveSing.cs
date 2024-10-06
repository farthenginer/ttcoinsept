using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkAndSetLiveSing : MonoBehaviour
{
    private void Start()
    {
        check();
    }
    async void check()
    {
        bool video = await GoogleAuthentication.Instance.CheckVideoBoolJust();
        UserData.Instance.videoBool = video;
    }
}
