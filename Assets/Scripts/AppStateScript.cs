using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppStateScript : MonoBehaviour
{
    public int vie = 3;

    public static AppStateScript instance;

    private void Awake(){
        if(instance != null){
            Debug.LogWarning("Multiple AppStateScript");
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);
    }

}
