using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameVanilla.Game.UI;
public class Setter : MonoBehaviour
{
    public List<GameObject> ButtonParents;
    int level = 0;
    private void Awake()
    {
        for (int i = 0; i < ButtonParents.Count; i++)
        {
            for (int a = 0; a < ButtonParents[i].transform.childCount; a++)
            {
                level += 1;
                ButtonParents[i].transform.GetChild(a).GetComponent<LevelButton>().numLevel = level;
            }
        }
    }
}
