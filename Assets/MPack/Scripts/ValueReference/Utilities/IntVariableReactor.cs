using UnityEngine;


namespace MPack
{
    public class IntVariableReactor : MonoBehaviour
    {
        [SerializeField]
        private IntVariable intVariable;
        [SerializeField]
        private bool checkValueWhenEnabled = true;

        public IntUnityEvent OnValueChanged;
        public StringUnityEvent OnValueChangedToString;

        void OnEnable()
        {
            if (checkValueWhenEnabled)
                OnChanged(intVariable.Value);
            intVariable.OnChanged += OnChanged;
        }
        void OnDisable()
        {
            intVariable.OnChanged -= OnChanged;
        }

        void OnChanged(int newValue)
        {
            OnValueChanged.Invoke(newValue);
            OnValueChangedToString.Invoke(newValue.ToString());
        }
    }
}
