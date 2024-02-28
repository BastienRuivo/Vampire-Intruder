using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectivesManager : MonoBehaviour
{
    private TextMeshProUGUI _textMeshPro;
    private IEnumerator _textCoroutine;
    public Animator _animObjectives;
    private bool _isUp = true;

    public Color mainColor;
    public Color mainTitleColor;
    public Color secondaryTitle;
    public Color secondaryColor;
    void Start()
    {
        _textMeshPro= GetComponent<TextMeshProUGUI>();
        _textCoroutine = UpdateText();
        StartCoroutine(_textCoroutine);

        _animObjectives.SetTrigger("Up");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            if(_isUp)
            {
                _animObjectives.SetTrigger("Down");
                _isUp = false;
            }
            else
            {
                _animObjectives.SetTrigger("Up");
                _isUp = true;
            }        
        }

    }

    private void OnDestroy()
    {
        if(_textCoroutine!= null)
            StopCoroutine(_textCoroutine);
    }

    private string TextWithColor(string text, Color col)
    {
        return $"<#{UnityEngine.ColorUtility.ToHtmlStringRGB(col)}>{text}</color>";
    }

    private IEnumerator UpdateText()
    {
        while(true)
        {
            string text = "";
            if (_textCoroutine != null && _textMeshPro != null)
            {
                int nbObjectives = GameController.GetInstance().objectivesToComplete.Count;
                GameController.GetInstance().objectivesToComplete.ForEach(obj =>
                {
                    Color color = Color.white;
                    //if (obj.isMain)
                    //{
                    //    switch (obj.state)
                    //    {
                    //        case GameController.ObjectiveState.UKNOWN_POS: color = unknownPosMain; break;
                    //        case GameController.ObjectiveState.DISCOVERED: color = discoveredMain; break;
                    //        case GameController.ObjectiveState.PLAYER_AT: color = playerAtMain; break;
                    //        case GameController.ObjectiveState.DONE: color = doneMain; break;
                    //    }
                    //}
                    //else
                    //{
                    //    switch (obj.state)
                    //    {
                    //        case GameController.ObjectiveState.UKNOWN_POS: color = uknownPos; break;
                    //        case GameController.ObjectiveState.DISCOVERED: color = discovered; break;
                    //        case GameController.ObjectiveState.PLAYER_AT: color = playerAt; break;
                    //        case GameController.ObjectiveState.DONE: color = done; break;
                    //    }
                    //}
                    //text += TextWithColor(obj.phrase, color);
                    string txt = "";
                    txt += obj.state == GameController.ObjectiveState.DONE ? $"<s>{obj.phrase}</s>" : obj.phrase;
                    if(obj.isMain)
                    {
                        txt = TextWithColor("Objectif principal :\n", mainTitleColor) + TextWithColor(txt, mainColor);
                    }
                    else
                    {
                        txt = TextWithColor(txt, secondaryColor);
                    }
                    text += txt + "\n\n";
                    if(obj.isMain && nbObjectives > 1)
                    {
                        text += TextWithColor("Objectifs secondaires : \n", secondaryTitle);
                    }

                });

                _textMeshPro.text = text;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
