using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPack
{
    public class StringVariableReactor : MonoBehaviour
    {
        [SerializeField]
        private StringVariable stringVariable;
        [SerializeField]
        private bool checkValueWhenStarted;

        public StringUnityEvent OnValueChanged;

        void Awake()
        {
            stringVariable.OnChanged += OnChanged;
        }

        void Start()
        {
            if (checkValueWhenStarted)
                OnChanged(stringVariable.Value);
        }

        void OnChanged(string newValue)
        {
            OnValueChanged.Invoke(newValue);
        }

        void OnDestroy()
        {
            stringVariable.OnChanged -= OnChanged;
        }
    }
}