using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace  MPack
{
    [CreateAssetMenu(menuName="MPack/Variable/String", order=0)]
    public class StringVariable : ScriptableObject
    {
        public string Value;

#if UNITY_EDITOR
        [TextArea]
        public string Note;

        public StringWithEnable EditorOverrideReset;
#endif

        public System.Action<string> OnChanged;

        public void SetValue(string value)
        {
            Value = value;
            OnChanged?.Invoke(value);
        }

        public void ResetValue(bool notify=true)
        {
            Value = "";
#if UNITY_EDITOR
            if (EditorOverrideReset.Enable)
                Value = EditorOverrideReset.Value;
#endif
            if (notify)
                OnChanged?.Invoke(Value);
        }
    }
}
