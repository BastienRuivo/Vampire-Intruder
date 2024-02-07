using UnityEngine;

namespace Systems.Ability.tests
{
    public class ASCTestAsync : MonoBehaviour
    {
        private AbilitySystemComponent _ascRef;
    
        // Start is called before the first frame update
        void Start()
        {
            _ascRef = GetComponent<AbilitySystemComponent>();
        
            //Defines statistics the ASC will work with.
            _ascRef.DefineStat("Blood", baseValue:100.0f, lowerRange:-15.0f);
        
            //Grant ability. For consumable ones this will just increment the available charge count if the ability has already been granted.
            _ascRef.GrantAbility<TestAbility>("TestAbility");
            _ascRef.GrantAbility<TestPassiveAbility>("TestPassiveAbility");
        
            //bind ability to a keyboard input. The ability will then be executed when this key is pressed.
            _ascRef.BindAbility("TestAbility", KeyCode.A);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                //get data from ASC
                Debug.Log($"Blood stat is : {_ascRef.QueryStat("Blood")}.");
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                Debug.Log("Cancel every ability.");
            
                //Cancel a given ability if it is currently running. (revert costs)
                //_ascRef.CancelAbility("TestAbility");
            
                //Cancel all abilities currently running. (revert costs)
                _ascRef.CancelAbilities();
            }
        }
    }
}
