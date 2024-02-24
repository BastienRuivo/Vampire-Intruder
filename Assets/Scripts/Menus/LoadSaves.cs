using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LoadSaves : MonoBehaviour
{
    public GameObject[] captures;
    public Button[] loadButtons;
    public Text[] descriptions;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void ReloadInformations()
    {
        AssetDatabase.Refresh();

        for (int i = 0; i < captures.Length; i++)
        {
            if (PlayerPrefs.HasKey("dialogName" + ((i % 3) + 1)))
            {
                // Description
                int runNumber = PlayerPrefs.GetInt("runNumber" + ((i % 3 + 1))) + 1;
                int levelNumber = PlayerPrefs.GetInt("levelNumber" + ((i % 3) + 1)) + 1;

                descriptions[i].text = "Run n°" + runNumber
                    + " - Niveau n°" + levelNumber
                    + " - " + PlayerPrefs.GetInt("day" + ((i % 3) + 1))
                    + "/" + PlayerPrefs.GetInt("month" + ((i % 3) + 1))
                    + "/" + PlayerPrefs.GetInt("year" + ((i % 3) + 1))
                    + " à " + PlayerPrefs.GetInt("hour" + ((i % 3) + 1))
                    + "h" + PlayerPrefs.GetInt("minute" + ((i % 3) + 1))
                    + "m" + PlayerPrefs.GetInt("second" + ((i % 3) + 1)) + "s";

                // Activate load button
                if (loadButtons.Length == captures.Length)
                {
                    loadButtons[i].interactable = true;
                }

                // Screenshot
                Sprite captureSprite = (Sprite) AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Resources/Graphics/Menus/Screenshot" + ((i % 3) + 1) + ".png");
                captures[i].GetComponent<Image>().sprite = captureSprite;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
