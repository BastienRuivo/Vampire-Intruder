using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DefaultNamespace.Templates;
using Interfaces;
using JetBrains.Annotations;
using UnityEngine;

namespace Systems.Ability
{
    public class AbilitySystemComponent : MonoBehaviour
    {
        /// <summary>
        /// Instance an object and returns it. This is to allow ability system to instance object
        /// </summary>
        /// <param name="ability">The ability spawning the actor.</param>
        /// <param name="path">The Asset path of the object to load</param>
        /// <param name="asAbilityResource">If the object should be considered as an ability's resource I.E. have the life time of the ability.</param>
        /// <returns>a game object instanced from asset path.</returns>
        public GameObject InstanceGameObject(Ability ability, string path, bool asAbilityResource = true)
        {
            if(ability == null)
                return null;
            GrantedAbility grantedAbility = _abilities.First(pair => pair.Value.GetAbility() == ability).Value;
            if (grantedAbility == null)
                return null;
            GameObject instance = Instantiate(Resources.Load<GameObject>(path));
            if(asAbilityResource)
                grantedAbility.Resources.AddFirst(instance);
            return instance;
        }

        /// <summary>
        /// Destroy a game object form an ability
        /// </summary>
        /// <param name="ability">The ability trying to destroy the actor.</param>
        /// <param name="gameObject">the game object to destroy</param>
        /// <param name="asResource">if the object was instanced from this ability as a resource</param>
        public void DestroyGameObject(Ability ability, GameObject gameObject, bool asResource = true)
        {
            if(gameObject == null || ability == null)
                return;
            if (asResource)
            {
                GrantedAbility grantedAbility = _abilities.First(pair => pair.Value.GetAbility() == ability).Value;
                grantedAbility.Resources.Remove(gameObject);
            }
            Destroy(gameObject);
        }

        public void RequestInputLock(Ability ability, bool status)
        {
            if(ability == null)
                return;
            
            GrantedAbility grantedAbility = _abilities.First(pair => pair.Value.GetAbility() == ability).Value;
            if (grantedAbility == null)
                return;

            if (status)
            {
                if(!grantedAbility.HasInputLock())
                    _inputLock.Take();
            }
            else
            {
                if(grantedAbility.HasInputLock())
                    _inputLock.Release();
            }
            grantedAbility.SetInputLock(status);
        }

        public Lock GetInputLock()
        {
            return _inputLock;
        }

        public void CommitAbility(Ability ability)
        {
            if(ability == null)
                return;
            
            GrantedAbility grantedAbility = _abilities.First(pair => pair.Value.GetAbility() == ability).Value;
            if (grantedAbility == null)
                return;
            ApplyAbilityCosts(grantedAbility);
            
            //handle cooldown
            if(grantedAbility.HasCooldown())
                grantedAbility.Cooldown();
        }
        
        public void CancelAbility(Ability ability)
        {
            if(ability == null)
                return;
            
            var grantedAbility = _abilities.First(pair => pair.Value.GetAbility() == ability);
            if (grantedAbility.Value == null)
                return;
            CancelAbility(grantedAbility.Key);
        }

        public void ApplyEffect(Ability ability, params KeyValuePair<string, float>[] effectStats)
        {
            if(effectStats.Length == 0)
                return;
            
            if(ability == null)
                return;
            
            GrantedAbility grantedAbility = _abilities.First(pair => pair.Value.GetAbility() == ability).Value;
            if (grantedAbility == null)
                return;
            
            foreach (var (attribute, value) in effectStats)
            {
                if (_stats.TryGetValue(attribute, out var stat))
                {
                    stat += value;
                }
            }
            
            _statChangeEventDispatcher.BroadcastEvent(gameObject);
        }
        
        /// <summary>
        /// Define a name/tag and float value pair that will be used and modified by the ability system.
        /// </summary>
        /// <param name="name">a name/tag</param>
        /// <param name="baseValue">a default value</param>
        /// <param name="lowerRange">minimum value</param>
        /// <param name="upperRange">maximum value, disabled if equals 0</param>
        public void DefineStat(string name, float baseValue = 0.0f, float lowerRange = 0.0f, float upperRange = 0.0f)
        {
            if(!_stats.TryAdd(name, new Attribute(baseValue, lowerRange, upperRange))) return;
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
            return _stats[name].GetValue();
        }

        /// <summary>
        /// Subscribe to events caused by any change in stats. Context is the game object owner of this ability system.
        /// </summary>
        /// <param name="observer">subscriber object</param>
        public void SubscribeToStatChanges(IEventObserver<GameObject> observer)
        {
            _statChangeEventDispatcher.Subscribe(observer);
        }

        /// <summary>
        /// Unsubscribe from events caused by any change in stats.
        /// </summary>
        /// <param name="observer">subscriber object</param>
        public void UnsubscribeToStatChanges(IEventObserver<GameObject> observer)
        {
            _statChangeEventDispatcher.Unsubscribe(observer);
        }
        
        /// <param name="name">name/tag associated with this stat.</param>
        /// <returns>The remaining cooldown of an ability.</returns>
        public float QueryAbilityCooldown(string name)
        {
            if (!_abilities.ContainsKey(name))
            {
                Debug.LogError($"Attempted to query state data from unknown ability with name \"{name}\"");
                return -1; //TODO throw exception here
            }
            return _abilities[name].GetCurrentCooldown();
        }
        
        /// <param name="name">name/tag associated with this stat.</param>
        /// <returns>The base cooldown of an ability.</returns>
        public float QueryAbilityCooldownBase(string name)
        {
            if (!_abilities.ContainsKey(name))
            {
                Debug.LogError($"Attempted to query state data from unknown ability with name \"{name}\"");
                return -1; //TODO throw exception here
            }
            return _abilities[name].GetAbility().GetCooldown();
        }
        
        /// <param name="name">name/tag associated with this stat.</param>
        /// <returns>the remaining charges if the ability is a consumable, else returns -1.</returns>
        public int QueryAbilityCharges(string name)
        {
            if (!_abilities.ContainsKey(name))
            {
                Debug.LogError($"Attempted to query state data from unknown ability with name \"{name}\"");
                return -1; //TODO throw exception here
            }
            if(_abilities[name].HasCharge())
                return _abilities[name].GetRemainingCharges();
            return -1;
        }

        /// <param name="name">name/tag associated with this stat.</param>
        /// <param name="statName"></param>
        /// <returns>the remaining charges if the ability is a consumable, else returns -1.</returns>
        public float QueryAbilityCosts(string name, string statName)
        {
            if (!_abilities.ContainsKey(name))
            {
                Debug.LogError($"Attempted to query state data from unknown ability with name \"{name}\"");
                return -1; //TODO throw exception here
            }

            if (_abilities[name].GetAbility().GetAbilityCosts().ContainsKey(statName))
                return _abilities[name].GetAbility().GetAbilityCosts()[statName];
            
            return 0.0f;
        }
        
        /// <param name="name">name/tag associated with this stat.</param>
        /// <returns>The icon of an ability.</returns>
        public string QueryAbilityUIIcon(string name)
        {
            if (!_abilities.ContainsKey(name))
            {
                Debug.LogError($"Attempted to query UI data from unknown ability with name \"{name}\"");
                return ""; //TODO throw exception here
            }
            return _abilities[name].GetAbility().GetIcon();
        }
        
        /// <param name="name">name/tag associated with this stat.</param>
        /// <returns>The UI name of an ability.</returns>
        public string QueryAbilityUIName(string name)
        {
            if (!_abilities.ContainsKey(name))
            {
                Debug.LogError($"Attempted to query state UI from unknown ability with name \"{name}\"");
                return ""; //TODO throw exception here
            }
            return _abilities[name].GetAbility().GetUIName();
        }
        
        /// <param name="name">name/tag associated with this stat.</param>
        /// <returns>The UI description of an ability.</returns>
        public string QueryAbilityUIDescription(string name)
        {
            if (!_abilities.ContainsKey(name))
            {
                Debug.LogError($"Attempted to query state UI from unknown ability with name \"{name}\"");
                return ""; //TODO throw exception here
            }
            return _abilities[name].GetAbility().GetDescription();
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
            if (code == KeyCode.None)
            {
                Debug.LogWarning($"Unable to bind ability \"{name}\" to key None. Consider using UnbindAbility instead.");
                return;
            }
            
            if (!_abilities.ContainsKey(name))
            {
                Debug.LogError($"Unable to bind unknown ability \"{name}\".");
                return; //todo throw error
            }

            if (_keyBindings.TryAdd(code, name))
            {
                _abilities[name].Bind(code);
                return;
            }
            
            //handle unbinding of previous ability
            _abilities[_keyBindings[code]].Bind(KeyCode.None);
            
            //handle removing previous binding of new ability
            if (_abilities[name].GetBinding() != KeyCode.None)
            {
                _keyBindings.Remove(_abilities[name].GetBinding());
                _abilities[name].Bind(KeyCode.None);
            }

            //Commit binding
            _keyBindings[code] = name;
            _abilities[name].Bind(code);
        }

        /// <summary>
        /// Unbind any ability from a given key code.
        /// </summary>
        /// <param name="code">a key code</param>
        public void UnbindAbility(KeyCode code)
        {
            if(!_keyBindings.ContainsKey(code)) return;

            _abilities[_keyBindings[code]].Bind(KeyCode.None);
            
            _keyBindings.Remove(code);
        }

        public string GetAbilityByBinding(KeyCode code)
        {
            if (_keyBindings.TryGetValue(code, out var binding))
            {
                return binding;
            }

            return null;
        }
        
        /// <returns>returns a set of name with all the abilities granted to the game object.</returns>
        public List<string> GetAbilities()
        {
            return _abilities.Keys.ToList();
        }

        /// <summary>
        /// Trigger an ability using its name. Handle its costs and charges.
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
        /// Handle the execution, charges, and costs of the ability inside a Coroutine.
        /// </summary>
        /// <param name="name">an ability name/tag</param>
        /// <returns></returns>
        private IEnumerator HandleAbility(string name)
        {
            GrantedAbility thisAbility = _abilities[name];
            
            //block excecution of abilities that are not ready to use (no charges or in cooldown)
            if (thisAbility.IsConsumable() && !(thisAbility.HasCharge())) yield break;
            if (thisAbility.HasCooldown() && thisAbility.IsInCooldown()) yield break;
            
            //check ability costs
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
                
                if(_stats[cost.Key].CanAffordSubtraction(cost.Value))
                {
                    isAbilityValid = false;
                    break;
                };
            }

            if (!isAbilityValid) yield break;
            
            thisAbility.SetHasAppliedCosts(false);
            if(thisAbility.GetAbility().DoApplyCostsOnTrigger())
                ApplyAbilityCosts(thisAbility);
            
            //start ability
            _runningAbilities[name] = thisAbility.GetAbility().OnAbilityTriggered(gameObject);
            yield return StartCoroutine(_runningAbilities[name]); //wait for ability to complete
            if(_runningAbilities[name] == null)
                yield break;
            
            //mark ability as complete
            _runningAbilities[name] = null;
            
            //cleanup resources left behind
            foreach (GameObject resource in _abilities[name].Resources)
                Destroy(resource);
            _abilities[name].Resources.Clear();
            
            //release potential input lock
            if (_abilities[name].HasInputLock())
            {
                _abilities[name].SetInputLock(false);
                _inputLock.Release();
            }
            
            //handle cooldown
            if(thisAbility.HasCooldown() && !thisAbility.HasAppliedCosts())
                thisAbility.Cooldown();
            
            //apply costs in case ability did not applied them
            ApplyAbilityCosts(thisAbility);
            
            //handle consumables
            if (!thisAbility.IsConsumable()) 
                yield break;
            thisAbility.ConsumeCharge();
            
            //todo maybe remove ability if consumable runs out of charges, for now ability is just unusable
        }

        /// <summary>
        /// Cancel an ability running, using its name.
        /// </summary>
        /// <param name="name">an ability name/tag</param>
        public void CancelAbility(string name)
        {
            if (!_runningAbilities.ContainsKey(name) || _runningAbilities[name] == null)
                return;
            
            StopCoroutine(_runningAbilities[name]);
            _runningAbilities[name] = null;
            
            //release potential input lock
            if (_abilities[name].HasInputLock())
            {
                _abilities[name].SetInputLock(false);
                _inputLock.Release();
            }
            
            //Cleanup in use resources
            foreach (GameObject resource in _abilities[name].Resources)
                Destroy(resource);
            _abilities[name].Resources.Clear();

            if (!_abilities[name].GetAbility().DoRefundOnCancel())
                return;
            
            //Manage ability refunds;
            if (_abilities[name].HasAppliedCosts())
            {
                var costs = _abilities[name].GetAbility().GetAbilityCosts();
                foreach (KeyValuePair<string,float> cost in costs)
                {
                    _stats[cost.Key] += cost.Value;
                }
                if(costs.Count > 0) _statChangeEventDispatcher.BroadcastEvent(gameObject);
            }
        }

        /// <summary>
        /// Cancel all ability running.
        /// </summary>
        public void CancelAbilities()
        {
            List<string> abilities = _runningAbilities.Keys.ToList();
            foreach (string ability in abilities)
            {
                CancelAbility(ability);
            }
        }

        private void ApplyAbilityCosts(GrantedAbility ability)
        {
            if(ability.HasAppliedCosts()) return;
            
            var costs = ability.GetAbility().GetAbilityCosts();
            
            //solve ability costs
            foreach (KeyValuePair<string,float> cost in costs)
            {
                _stats[cost.Key] -= cost.Value;
            }
            
            ability.SetHasAppliedCosts(true);
            
            if(costs.Count > 0) _statChangeEventDispatcher.BroadcastEvent(gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            //Solve abilities cooldown
            foreach (KeyValuePair<string, GrantedAbility> ability in _abilities)
            {
                GrantedAbility abilityToCheck = ability.Value;
                if (abilityToCheck.HasCooldown() && abilityToCheck.IsInCooldown())
                {
                    abilityToCheck.UpdateCooldown();
                }
            }
            
            //Handle passive/self-triggering abilities
            foreach (KeyValuePair<string, GrantedAbility> ability in _abilities)
            {
                Ability abilityToCheck = ability.Value.GetAbility();
                if (abilityToCheck.IsSelfTriggeringAbility())
                {
                    if(abilityToCheck.ShouldAbilityTrigger(gameObject))
                        TriggerAbility(ability.Key);
                }
            }
            
            //Auto trigger abilities bound to input keys
            if(_inputLock.IsOpened())
                foreach (KeyValuePair<KeyCode,string> binding in _keyBindings)
                {
                    if (Input.GetKeyDown(binding.Key))
                    {
                        TriggerAbility(binding.Value);
                    }
                }
        }
        
        private class GrantedAbility
        {
            private readonly Ability _ability;
            private int _charges;
            private float _currentCooldown;
            private bool _inCooldown = false;
            public readonly LinkedList<GameObject> Resources = new LinkedList<GameObject>();
            private bool _hasInputLock = false;
            private bool _hasAppliedCosts = false;

            private KeyCode _binding = KeyCode.None;
            
            public GrantedAbility(Ability ability)
            {
                _ability = ability;
                _charges = ability.IsConsumableAbility() ? 1 : -1;
                _currentCooldown = ability.GetCooldown() > 0 ? 0 : -1;
            }

            public bool IsConsumable()
            {
                return _ability.IsConsumableAbility();
            }

            public bool HasCharge()
            {
                return _charges > 0;
            }

            public int GetRemainingCharges()
            {
                return _charges;
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

            public bool HasCooldown()
            {
                return _ability.GetCooldown() > 0;
            }

            public bool IsInCooldown()
            {
                return _inCooldown;
            }

            public float GetCurrentCooldown()
            {
                return _currentCooldown;
            }

            public void UpdateCooldown()
            {
                if (!HasCooldown())
                    return;
                _currentCooldown -= Time.deltaTime;
                if (_currentCooldown <= 0)
                    _inCooldown = false;
            }

            public void Cooldown()
            {
                if (!HasCooldown())
                    return;
                if(_inCooldown) 
                    Debug.LogError("Tried to put in cooldown an ability that was already in this state.");
                _inCooldown = true;
                _currentCooldown = _ability.GetCooldown();
            }

            public void Bind(KeyCode code)
            {
                _binding = code;
            }

            public KeyCode GetBinding()
            {
                return _binding;
            }

            public void SetInputLock(bool status)
            {
                _hasInputLock = status;
            }

            public bool HasInputLock()
            {
                return _hasInputLock;
            }

            public void SetHasAppliedCosts(bool status)
            {
                _hasAppliedCosts = status;
            }

            public bool HasAppliedCosts()
            {
                return _hasAppliedCosts;
            }
        }

        private class Attribute
        {
            private float _value;
            private readonly float _rangeMin;
            private readonly float _rangeMax;

            public Attribute(float value, float rangeMin = 0, float rangeMax = 0)
            {
                if (value < rangeMax || (HasUpperBounds() && value > rangeMax))
                {
                    Debug.LogWarning("Attribute default value out of range.");
                    Clamp();
                }
                
                _value = value;
                _rangeMin = rangeMin;
                _rangeMax = rangeMax;
            }

            public bool CanAffordSubtraction(float v)
            {
                return v > (_value - _rangeMin);
            }

            public float GetValue()
            {
                return _value;
            }
            
            private void Clamp()
            {
                _value = (HasUpperBounds() && _value > _rangeMax) ? _rangeMax : _value < _rangeMin ? _rangeMin : _value;
            }


            private static Attribute Clamp(Attribute a)
            {
                a.Clamp();
                return a;
            }

            public static Attribute operator + (Attribute a, float v)
            {
                a._value += v;
                return Clamp(a);
            }
            
            public static Attribute operator - (Attribute a, float v)
            {
                return a + -1 * v;
            }

            private bool HasUpperBounds()
            {
                return _rangeMax > 0;
            }
        }
        
        private readonly Dictionary<string, GrantedAbility> _abilities = new ();
        private readonly Dictionary<KeyCode, string> _keyBindings = new ();
        private readonly Dictionary<string, Attribute> _stats = new ();
        [ItemCanBeNull] private readonly Dictionary<string, IEnumerator> _runningAbilities = new ();

        private readonly EventDispatcher<GameObject> _statChangeEventDispatcher = new ();

        private readonly Lock _inputLock = new Lock();

        //todo add a way to affect stats of ASC from an ability.
    }
}
