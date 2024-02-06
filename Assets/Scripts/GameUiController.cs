using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GameUiController : MonoBehaviour, IEventObserver<GameProgressionState>
{
    public float dialogLineDuration = 4.0f;

    [FormerlySerializedAs("JessikaBeginQuote")] [Header("Jessika's Quotes")] public string jessikaBeginQuote;
    [FormerlySerializedAs("JessikaMiddleQuote")] [Header("Jessika's Quotes")] public string jessikaMiddleQuote;
    [FormerlySerializedAs("JessikaFinalQuote")] [Header("Jessika's Quotes")] public string jessikaFinalQuote;
    
    // Start is called before the first frame update
    void Start()
    {
        GameController.GetGameMode().SubscribeToGameProgressionEvent(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEvent(GameProgressionState context)
    {
        switch (context)
        {
            case GameProgressionState.Begin:
                StartCoroutine(DialogRoutine($"Jessika : \"{jessikaBeginQuote}\""));
                break;
            case GameProgressionState.Middle:
                StartCoroutine(DialogRoutine($"Jessika : \"{jessikaMiddleQuote}\""));
                break;
            case GameProgressionState.Last:
                StartCoroutine(DialogRoutine($"Jessika : \"{jessikaFinalQuote}\""));
                break;
            case GameProgressionState.End:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(context), context, null);
        }
    }

    private IEnumerator DialogRoutine(string text)
    {
        GetComponentInChildren<TextMeshProUGUI>().SetText(text);
        GetComponentInChildren<Animator>().SetBool("ShowDialog", true);
        yield return new WaitForSeconds(dialogLineDuration);
        GetComponentInChildren<Animator>().SetBool("ShowDialog", false);

    }
}
