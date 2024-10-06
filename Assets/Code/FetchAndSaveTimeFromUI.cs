using UnityEngine.SceneManagement;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
public class FetchAndSaveTimeFromUI : MonoBehaviour
{
    public GameObject load, error;
    private async void Start()
    {
        load.SetActive(true);
        error.SetActive(false);

    }
    
}
