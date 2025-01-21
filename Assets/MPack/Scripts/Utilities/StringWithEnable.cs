using UnityEngine;

namespace MPack
{
    [System.Serializable]
    public struct StringWithEnable
    {
        public bool Enable;
        public string Value;

        public StringWithEnable(string value, bool enable=false)
        {
            Enable = enable;
            Value = value;
        }
    }
}