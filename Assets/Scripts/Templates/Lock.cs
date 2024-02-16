using UnityEngine;

namespace DefaultNamespace.Templates
{
    public class Lock
    {
        private uint _count = 0;

        public bool IsClosed()
        {
            return _count > 0;
        }

        public bool IsOpened()
        {
            return _count == 0;
        }

        public void Take()
        {
            _count++;
        }
        
        public void Release()
        {
            if (_count == 0) //todo remove when game tested
            {
                Debug.LogError("Release not possible");
                return;
            }
            _count--;
        }
    }
}