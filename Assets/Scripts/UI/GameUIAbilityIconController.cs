using Systems.Ability;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameUIAbilityIconController : MonoBehaviour
    {
        public GameObject abilityIconIndicator;
        public GameObject cooldownIndicator;
        public GameObject cooldownTextIndicator;
        public GameObject keyBindingIndicator;

        public bool showKeyBinding = true;
        
        private AbilitySystemComponent _ASC = null;
        private string _abilityTag = null;

        private Image _cooldownIndicatorImage;
        private TextMeshProUGUI _cooldownText;

        public string GetAbilityTag()
        {
            return _abilityTag;
        }

        public void SetAbilitySystemComponent(AbilitySystemComponent abilitySystemComponent)
        {
            _ASC = abilitySystemComponent;
            if(!string.IsNullOrEmpty(_abilityTag))
                UpdateAbilityUIBase();
        }

        public void SetAbilityTag(string abilityTag)
        {
            _abilityTag = abilityTag;
            if (_ASC != null)
                UpdateAbilityUIBase();
        }

        private void UpdateAbilityUIBase()
        {
            Texture2D tex = Resources.Load<Texture2D>(_ASC.QueryAbilityUIIcon(_abilityTag));
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            abilityIconIndicator.GetComponent<Image>().sprite =sprite;
        }
        
        public void SetOpacity(float alpha)
        {
            Image abilityBg = GetComponent<Image>();
            Color bgCol = abilityBg.color;
            bgCol.a = alpha;
            abilityBg.color = bgCol;

            Image abilityIcon = abilityIconIndicator.GetComponent<Image>();
            Color iconCol = abilityIcon.color;
            iconCol.a = alpha;
            abilityIcon.color = iconCol;

            Image cooldownIcon = cooldownIndicator.GetComponent<Image>();
            Color cdCol = cooldownIcon.color;
            cdCol.a = alpha;
            abilityIcon.color = cdCol;

            TextMeshProUGUI cooldownText = cooldownTextIndicator.GetComponent<TextMeshProUGUI>();
            Color cdTxtCol = cooldownText.color;
            cdTxtCol.a = alpha;
            cooldownText.color = cdTxtCol;
            
            Image keyBindingIcon = keyBindingIndicator.GetComponent<Image>();
            Color kbCol = keyBindingIcon.color;
            kbCol.a = alpha;
            keyBindingIcon.color = kbCol;
        }

        public void SetHighlight(bool highlight)
        {
            if (highlight)
            {
                GetComponent<Image>().color =Color.yellow;
            }
            else
            {
                GetComponent<Image>().color =Color.black;
            }
        }
    
        // Start is called before the first frame update
        void Start()
        {
            keyBindingIndicator.GetComponent<Image>().enabled = showKeyBinding;
            _cooldownIndicatorImage = cooldownIndicator.GetComponent<Image>();
            _cooldownText = cooldownTextIndicator.GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_ASC == null || string.IsNullOrEmpty(_abilityTag))
                return;

            float cooldown = _ASC.QueryAbilityCooldown(_abilityTag);
            if (cooldown > 0.0f)
            {
                cooldownIndicator.SetActive(true);
                _cooldownText.SetText($"{cooldown:F1}");
            }
            else
            {
                cooldownIndicator.SetActive(false);
            }
        }
    }
}
