using UnityEngine;


namespace MPack
{
    [CreateAssetMenu(menuName="MPack/Variable/Int Range", order=0)]
    public class IntRangeVariable : ScriptableObject
    {
        public int Min;
        public int Max;

#if UNITY_EDITOR
        [TextArea]
        public string Note;

        public ValueWithEnable<int> EditorOverrideResetMin;
        public ValueWithEnable<int> EditorOverrideResetMax;
#endif

        public event System.Action<int, int> OnChanged;

        public void SetValue(int min, int max)
        {
            Min = min;
            Max = max;
            OnChanged?.Invoke(Min, Max);
        }

        public void AddValue(int min, int max)
        {
            Min += min;
            Max += max;
            OnChanged?.Invoke(Min, Max);
        }

        public void ResetValue(bool notify=true)
        {
            Min = 0;
            Max = 0;
#if UNITY_EDITOR
            if (EditorOverrideResetMin.Enable)
                Min = EditorOverrideResetMin.Value;
            if (EditorOverrideResetMax.Enable)
                Max = EditorOverrideResetMax.Value;
#endif
            if (notify)
                OnChanged?.Invoke(Min, Max);
        }
    }
}