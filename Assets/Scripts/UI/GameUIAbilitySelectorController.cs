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
    
    [Header("Background")] 
    public GameObject[] background; 

    [Header("Children")]
    public GameObject[] abilitiesIcons;

    private string[] _abilities;
    private AbilitySystemComponent _ASC;
    private uint _abilityIndex = 0;

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
        
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _abilities = new[] { "TP","Blind" };
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
                GameUIAbilityIconController ability =
                    abilitiesIcons[_abilityIndex].GetComponent<GameUIAbilityIconController>();
                ability.SetHighlight(true);
                _ASC.BindAbility(ability.GetAbilityTag(), KeyCode.E);
            }
        }
        else if (scrollAmount < 0f)
        {
            if (_abilityIndex > 0)
            {
                abilitiesIcons[_abilityIndex--].GetComponent<GameUIAbilityIconController>().SetHighlight(false);
                GameUIAbilityIconController ability =
                    abilitiesIcons[_abilityIndex].GetComponent<GameUIAbilityIconController>();
                ability.SetHighlight(true);
                _ASC.BindAbility(ability.GetAbilityTag(), KeyCode.E);
            }
        }
    }
}
