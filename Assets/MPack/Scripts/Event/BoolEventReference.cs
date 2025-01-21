using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MPack
{
    [CreateAssetMenu(menuName="MPack/Event/bool", order=1)]
    public class BoolEventReference : AbstractEventRefernece
    {
        private event System.Action<bool> triggerEvent;

#if UNITY_EDITOR
        [SerializeField, InspectorButton("Set True", "SetToTrue")]
        private bool _setTrue;
        [SerializeField, InspectorButton("Set False", "SetToFalse")]
        private bool _setFalse;


        void SetToTrue() => Invoke(true);
        void SetToFalse() => Invoke(false);
#endif

        public void Invoke(bool parameter)
        {
            for (int i = eventDispatchers.Count - 1; i >= 0; i--)
                eventDispatchers[i].DispatchEventWithBool(parameter);

            triggerEvent?.Invoke(parameter);
        }

        public void RegisterEvent(System.Action<bool> callback) => triggerEvent += callback;
        public void UnregisterEvent(System.Action<bool> callback) => triggerEvent -= callback;
    }
}