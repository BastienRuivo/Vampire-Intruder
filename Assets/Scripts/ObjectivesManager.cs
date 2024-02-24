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
    private bool _isUp = false;

    //public Color uknownPos;
    //public Color unknownPosMain;
    //public Color discovered;
    //public Color discoveredMain;
    //public Color playerAt;
    //public Color playerAtMain;
    //public Color done;
    //public Color doneMain;
    void Start()
    {
        _textMeshPro= GetComponent<TextMeshProUGUI>();
        _textCoroutine = UpdateText();
        StartCoroutine(_textCoroutine);
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
        return $"<#{UnityEngine.ColorUtility.ToHtmlStringRGB(col)}>{text}</color>\n";
    }

    private IEnumerator UpdateText()
    {
        while(true)
        {
            string text = "";
            if (_textCoroutine != null && _textMeshPro != null)
            {
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
                    text += obj.state == GameController.ObjectiveState.DONE ? $"<s>{obj.phrase}</s>" : obj.phrase;
                });

                _textMeshPro.text = text;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
