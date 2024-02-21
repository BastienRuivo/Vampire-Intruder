using Systems.Ability;
using UnityEngine;

namespace UI
{
    public class GameUIAbilityIconController : MonoBehaviour
    {
        private AbilitySystemComponent _ASC = null;
        private string _abilityTag = null;

        public void SetASC(AbilitySystemComponent abilitySystemComponent)
        {
            _ASC = abilitySystemComponent;
        }

        public void SetAbilityTag(string abilityTag)
        {
            _abilityTag = abilityTag;
        }
    
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if (_ASC == null || _abilityTag == null || _abilityTag == "")
                return;
            
            //_ASC.
        }
    }
}
