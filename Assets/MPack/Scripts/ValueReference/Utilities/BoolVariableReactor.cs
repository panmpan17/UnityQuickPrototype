using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace MPack
{
    public class BoolVariableReactor : MonoBehaviour
    {
        [SerializeField]
        private BoolVariable boolVariable;
        [SerializeField]
        private bool checkValueWhenStarted;

        public BoolUnityEvent OnValueChanged;
        public BoolUnityEvent OnValueChanged_Reverse;

        public UnityEvent OnTrue;
        public UnityEvent OnFalse;

        void OnEnable()
        {
            if (boolVariable)
                boolVariable.OnChanged += OnChanged;
        }

        void OnDisable()
        {
            if (boolVariable)
                boolVariable.OnChanged -= OnChanged;
        }

        void Start()
        {
            if (checkValueWhenStarted)
                OnChanged(boolVariable.Value);
        }

        void OnChanged(bool newValue)
        {
            OnValueChanged.Invoke(newValue);
            OnValueChanged_Reverse.Invoke(!newValue);

            if (newValue) OnTrue.Invoke();
            else OnFalse.Invoke();
        }

        public void Event_OnOutSideEvent(bool value)
        {
            if (value) OnTrue.Invoke();
            else OnFalse.Invoke();
        }
    }
}