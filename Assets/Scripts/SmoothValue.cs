using UnityEngine;

namespace DefaultNamespace
{
    public class SmoothScalarValue
    {
        public SmoothScalarValue(float defaultValue, float interpolationTime = 1)
        {
            _value = defaultValue;
            _target = _value;
            _interpolationTime = interpolationTime;
        }
        
        public float GetValue() {return _value;}
        public float UpdateGetValue() {UpdateTick(); return GetValue();}

        public void Update(float deltaTime)
        {
            if(_locked) return;
            
            _time += deltaTime;

            if (_time > _interpolationTime)
            {
                _value = _target;
                _locked = true;
            }
            else
            {
                _value = FloatLerp(_value, _target, (_time / _interpolationTime));
            }
        }

        public void RetargetValue(float V)
        {
            _time = 0;
            _locked = false;
            _target = V;
        }
        
        public void UpdateTick() {Update(Time.deltaTime);}

        private float FloatLerp(float a, float b, float alpha)
        {
            return alpha * b + (1 - alpha) * a;
        }
        
        private float _value;
        private float _target;
        private readonly float _interpolationTime;
        private float _time = 0.0f;
        private bool _locked = true;
    }
}