using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Systems.Ability
{
    public class AbilitySystemComponent : MonoBehaviour
    {
        private class GrantedAbility
        {
            private readonly Ability _ability;
            private int _charges;
            
            public GrantedAbility(Ability ability)
            {
                _ability = ability;
                _charges = ability.IsConsumableAbility() ? 1 : -1;
            }

            public bool IsConsumable()
            {
                return _ability.IsConsumableAbility();
            }

            public bool HasCharge()
            {
                return _charges > 0;
            }

            public Ability GetAbility()
            {
                return _ability;
            }

            public void ConsumeCharge()
            {
                _charges--;
            }
            
            public void AddCharge()
            {
                _charges++;
            }
        }
        
        private Dictionary<string, GrantedAbility> _abilities = new ();
        private Dictionary<KeyCode, string> _keyBindings = new ();
        private Dictionary<string, float> _stats = new ();
        [ItemCanBeNull] private Dictionary<string, IEnumerator> _runningAbilities = new ();

        /// <summary>
        /// Define a name/tag and float value pair that will be used and modified by the ability system.
        /// </summary>
        /// <param name="name">a name/tag</param>
        /// <param name="baseValue">a default value</param>
        public void DefineStat(string name, float baseValue = 0.0f)
        {
            if(!_stats.TryAdd(name, baseValue)) return;
        }
        
        /// <param name="name">name/tag associated with this stat.</param>
        /// <returns>the value associated with a given name/tag.</returns>
        public float QueryStat(string name)
        {
            if (!_stats.ContainsKey(name))
            {
                Debug.LogError($"Attempted to query unknown stat with name \"{name}\"");
                return -1; //TODO throw exception here
            }
            return _stats[name];
        }

        /// <summary>
        /// Grand an ability, given by its class, to the game object.
        /// If the ability has already been granted, add a charge if it is a consumable ability, or throw a warning if
        /// not.
        /// </summary>
        /// <param name="name">a name/tag</param>
        /// <typeparam name="TAbilityType">Ability class.</typeparam>
        public void GrantAbility<TAbilityType>(string name) where TAbilityType : Ability, new()
        {
            if (_abilities.TryAdd(name, new GrantedAbility(new TAbilityType())))
                return; //All good;

            if (!(_abilities[name].GetType() == typeof(TAbilityType)))
            {
                Debug.LogError($"Unable to grant ability named \"{name}\" as it has already been bound to " +
                               $"another kind of ability.");
                return; //todo throw error 
            }

            if (!(_abilities[name].IsConsumable()))
            {
                Debug.LogWarning($"Tried to bind an ability, \"{name}\", that has already been bound.");
                return; //todo throw warning
            }
            
            _abilities[name].AddCharge();
        }

        /// <summary>
        /// Bind an ability to a key input, so the ability will be self triggered when the given key is pressed.
        /// Only one ability can be bound a single key binding. Binding an ability to a key that was already bound to
        /// another will remove the previous binding.
        /// </summary>
        /// <param name="name">an ability name/tag</param>
        /// <param name="code">a key code</param>
        public void BindAbility(string name, KeyCode code)
        {
            if (!_abilities.ContainsKey(name))
            {
                Debug.LogError($"Unable to bind unknown ability \"{name}\".");
                return; //todo throw error
            }
            
            if(_keyBindings.TryAdd(code, name))
                return;

            _keyBindings[code] = name;
        }

        /// <summary>
        /// Unbind any ability from a given key code.
        /// </summary>
        /// <param name="code">a key code</param>
        public void UnbindAbility(KeyCode code)
        {
            if(!_keyBindings.ContainsKey(code)) return;

            _keyBindings.Remove(code);
        }
        
        /// <returns>returns a set of name with all the abilities granted to the game object.</returns>
        public List<string> GetAbilities()
        {
            return _abilities.Keys.ToList();
        }

        /// <summary>
        /// Trigger an ability using its name.
        /// </summary>
        /// <param name="name">an ability name/tag</param>
        public void TriggerAbility(string name)
        {
            if(_runningAbilities.ContainsKey(name) && _runningAbilities[name] != null)
                return;
            
            _runningAbilities.TryAdd(name, null);

            StartCoroutine(HandleAbility(name));
        }

        /// <summary>
        /// Handle the execution of the ability inside a Coroutine.
        /// </summary>
        /// <param name="name">an ability name/tag</param>
        /// <returns></returns>
        private IEnumerator HandleAbility(string name)
        {
            GrantedAbility thisAbility = _abilities[name];
            if (thisAbility.IsConsumable() && !(thisAbility.HasCharge())) yield break;
            
            //check costs
            var costs = thisAbility.GetAbility().GetAbilityCosts();
            bool isAbilityValid = true;
            foreach (KeyValuePair<string,float> cost in costs)
            {
                if (!_stats.ContainsKey(cost.Key))
                {
                    Debug.LogError($"Undefined stat {cost.Key} required by ability {name}'s cost.");
                    isAbilityValid = false;
                    break;
                } //todo throw error here
                
                if(_stats[cost.Key] < cost.Value)
                {
                    isAbilityValid = false;
                    break;
                };
            }

            if (!isAbilityValid) yield break;
            
            //solve ability costs
            foreach (KeyValuePair<string,float> cost in costs)
            {
                _stats[cost.Key] -= cost.Value;
            }
            
            _runningAbilities[name] = thisAbility.GetAbility().OnAbilityTriggered(gameObject);
            yield return StartCoroutine(_runningAbilities[name]); //wait for ability to complete
            
            //handle ability cancel
            if (_runningAbilities[name] == null)
            {
                //revert ability costs
                foreach (KeyValuePair<string,float> cost in costs)
                {
                    _stats[cost.Key] += cost.Value;
                }
                yield break;
            } 
            
            _runningAbilities[name] = null;
            
            //handle consumables
            if (!thisAbility.IsConsumable()) yield break;
            thisAbility.ConsumeCharge();
            
            //todo maybe remove ability if consumable runs out of charges, for now ability is just unusable
        }

        /// <summary>
        /// Cancel an ability running, using its name.
        /// </summary>
        /// <param name="name">an ability name/tag</param>
        public void CancelAbility(string name)
        {
            if (_runningAbilities.ContainsKey(name) && _runningAbilities[name] != null)
            {
                _runningAbilities[name] = null;
            }
        }

        /// <summary>
        /// Cancel all ability running.
        /// </summary>
        public void CancelAbilities()
        {
            foreach (string ability in _runningAbilities.Keys)
            {
                CancelAbility(ability);
            }
        }
        
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            //Auto trigger abilities bound to input keys
            foreach (KeyValuePair<KeyCode,string> binding in _keyBindings)
            {
                if (Input.GetKeyDown(binding.Key))
                {
                    TriggerAbility(binding.Value);
                }
            }
        }
    }
}
