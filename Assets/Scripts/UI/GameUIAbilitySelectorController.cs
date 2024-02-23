using System.Collections;
using System.Collections.Generic;
using Systems.Ability;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class GameUIAbilitySelectorController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject abilityImageIcon;
    public GameObject abilityName;
    public GameObject abilityBloodCost;
    public GameObject abilityCooldown;
    public GameObject abilityDescription;
    public GameObject confirmHintIcon;
    public GameObject confirmHintText;
    
    
    [Header("Background")] 
    public GameObject[] background; 

    [Header("Children")]
    public GameObject[] abilitiesIcons;

    private string[] _abilities;
    private AbilitySystemComponent _ASC;
    private uint _abilityIndex = 0;
    private static readonly int Opacity = Shader.PropertyToID("_Opacity");

    public void SetAbilitySystemComponent(AbilitySystemComponent asc)
    {
        _ASC = asc;
        foreach (GameObject icon in abilitiesIcons)
        {
            icon.GetComponent<GameUIAbilityIconController>().SetAbilitySystemComponent(_ASC);
        }
    }
    
    public void UpdateAbilityUIBase()
    {
        string boundAbility = _ASC.GetAbilityByBinding(KeyCode.E);
        
        Texture2D tex = Resources.Load<Texture2D>(_ASC.QueryAbilityUIIcon(boundAbility));
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        abilityImageIcon.GetComponent<Image>().sprite =sprite;
        
        float blood = _ASC.QueryAbilityCosts(boundAbility, "Blood");

        abilityName.GetComponent<TextMeshProUGUI>().SetText(_ASC.QueryAbilityUIName(boundAbility));
        abilityBloodCost.GetComponent<TextMeshProUGUI>().SetText($"Sang : {-blood} %");
        abilityDescription.GetComponent<TextMeshProUGUI>().SetText(_ASC.QueryAbilityUIDescription(boundAbility));
        abilityCooldown.GetComponent<TextMeshProUGUI>().SetText($"Temps de recharge : {_ASC.QueryAbilityCooldownBase(boundAbility)}s");

        for (uint i = 0; i < _abilities.Length; i++)
        {
            abilitiesIcons[i].GetComponent<GameUIAbilityIconController>().SetAbilityTag(_abilities[i]);
            if (boundAbility == _abilities[i])
            {
                _abilityIndex = i;
                abilitiesIcons[i].GetComponent<GameUIAbilityIconController>().SetHighlight(true);
            }
        }
        for (int i = _abilities.Length; i < 6; i++)
        {
            abilitiesIcons[i].SetActive(false);
        }
    }

    public void SetOpacity(float alpha)
    {
        for (uint i = 0; i < _abilities.Length; i++)
        {
            abilitiesIcons[i].GetComponent<GameUIAbilityIconController>().SetOpacity(alpha);
        }
        
        Image img;
        TextMeshProUGUI txt;
        Color col;
        
        img = background[0].GetComponent<Image>();
        col = img.color;
        col.a = alpha * 0.75f;
        img.color = col;

        for (uint i = 1; i < background.Length; i++)
        {
            background[i].GetComponent<Image>().material.SetFloat(Opacity, alpha * 0.75f);
        }

        img = abilityImageIcon.GetComponent<Image>();
        col = img.color;
        col.a = alpha;
        img.color = col;

        txt = abilityName.GetComponent<TextMeshProUGUI>();
        col = txt.color;
        col.a = alpha;
        txt.color = col;
        
        txt = abilityBloodCost.GetComponent<TextMeshProUGUI>();
        col = txt.color;
        col.a = alpha;
        txt.color = col;
        
        txt = abilityCooldown.GetComponent<TextMeshProUGUI>();
        col = txt.color;
        col.a = alpha;
        txt.color = col;
        
        txt = abilityDescription.GetComponent<TextMeshProUGUI>();
        col = txt.color;
        col.a = alpha;
        txt.color = col;
        
        img = confirmHintIcon.GetComponent<Image>();
        col = img.color;
        col.a = alpha;
        img.color = col;
        
        txt = confirmHintText.GetComponent<TextMeshProUGUI>();
        col = txt.color;
        col.a = alpha;
        txt.color = col;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _abilities = new[] { "TP","Blind", "Invisibility", "Lure", "Sedate", "BloodPack" };
    }

    // Update is called once per frame
    void Update()
    {
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");

        if (scrollAmount > 0f)
        {
            if (_abilityIndex < (_abilities.Length - 1))
            {
                abilitiesIcons[_abilityIndex++].GetComponent<GameUIAbilityIconController>().SetHighlight(false);
                _ASC.BindAbility( abilitiesIcons[_abilityIndex].GetComponent<GameUIAbilityIconController>().GetAbilityTag(), KeyCode.E);
                UpdateAbilityUIBase();
            }
        }
        else if (scrollAmount < 0f)
        {
            if (_abilityIndex > 0)
            {
                abilitiesIcons[_abilityIndex--].GetComponent<GameUIAbilityIconController>().SetHighlight(false);
                _ASC.BindAbility( abilitiesIcons[_abilityIndex].GetComponent<GameUIAbilityIconController>().GetAbilityTag(), KeyCode.E);
                UpdateAbilityUIBase();
            }
        }
    }
}
