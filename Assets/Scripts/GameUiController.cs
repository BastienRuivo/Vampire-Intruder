using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Interfaces;
using Systems.Ability;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameUiController : MonoBehaviour, IEventObserver<TimeProgression>, IEventObserver<GameObject>
{
    public float dialogLineDuration = 4.0f;

    [FormerlySerializedAs("JessikaBeginQuote")] [Header("Jessika's Quotes")] public string jessikaBeginQuote;
    [FormerlySerializedAs("JessikaMiddleQuote")] [Header("Jessika's Quotes")] public string jessikaMiddleQuote;
    [FormerlySerializedAs("JessikaFinalQuote")] [Header("Jessika's Quotes")] public string jessikaFinalQuote;

    private readonly SmoothScalarValue _playerBloodStatSmooth = new SmoothScalarValue(1);
    private readonly SmoothScalarValue _playerBloodStatEyeEffectSmooth =  new SmoothScalarValue(0, 5f);

    private GameObject _healthBarPanel;
    private GameObject _eyeballBarPanel;

    // Start is called before the first frame update
    void Start()
    {
        GameController.GetGameMode().SubscribeToGameProgressionEvent(this);
        PlayerController.GetPlayer().GetComponent<AbilitySystemComponent>().SubscribeToStatChanges(this);
        
        foreach (Transform child in transform)
        {
            GameObject childGameObject = child.gameObject;
            if (childGameObject.CompareTag("GameUIHealthBar"))
            {
                _healthBarPanel = childGameObject;
            }
            
            if (childGameObject.CompareTag("GameUIEyeBall"))
            {
                _eyeballBarPanel = childGameObject;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        _healthBarPanel.GetComponent<Image>().material.SetFloat("_Progression",_playerBloodStatSmooth.UpdateGetValue());
        _eyeballBarPanel.GetComponent<Image>().material.SetFloat("_Damage",_playerBloodStatEyeEffectSmooth.UpdateGetValue());
    }

    public void OnEvent(TimeProgression context)
    {
        switch (context)
        {
            case TimeProgression.Begin:
                StartCoroutine(DialogRoutine($"Jessika : \"{jessikaBeginQuote}\""));
                break;
            case TimeProgression.Middle:
                StartCoroutine(DialogRoutine($"Jessika : \"{jessikaMiddleQuote}\""));
                break;
            case TimeProgression.Last:
                StartCoroutine(DialogRoutine($"Jessika : \"{jessikaFinalQuote}\""));
                break;
            case TimeProgression.End:
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

    public void OnEvent(GameObject context)
    {
        AbilitySystemComponent ascRef = context.GetComponent<AbilitySystemComponent>();
        
        float blood = ascRef.QueryStat("Blood");
        float maxBlood = ascRef.QueryStat("BloodMax");
        float bloodRatio = blood / maxBlood;
        bloodRatio = bloodRatio > 0? bloodRatio : 0.0f;
        
        _playerBloodStatSmooth.RetargetValue(bloodRatio);
        _playerBloodStatEyeEffectSmooth.RetargetValue(bloodRatio * 4 > 1 ? 0.0f : 1 - (bloodRatio * 4.0f));
    }
}
