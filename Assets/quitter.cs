using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class quitter : MonoBehaviour
{
    public void SignOut()
    {
        var menumanager = FindObjectOfType<MenuManager>();
        menumanager.GetComponent<MenuManager>().outOturum();
        Invoke("quitfunc", 2);  
    }
    public void DeleteUser()
    {
        var menumanager = FindObjectOfType<MenuManager>();
        menumanager.GetComponent<MenuManager>().delete();
        Invoke("quitfunc", 4);

    }
    public void Resume()
    {
        var menumanager = FindObjectOfType<MenuManager>().GetComponent<MenuManager>();
        menumanager.closeAllPanel();
        menumanager.openPanel(menumanager.gamePanel);
    }
    void quitfunc()
    {
        Application.Quit();
    }
}
