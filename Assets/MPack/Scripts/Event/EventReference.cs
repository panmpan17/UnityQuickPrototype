using System.Collections.Generic;
using UnityEngine;


namespace MPack
{
    public class AbstractEventRefernece : ScriptableObject
    {
        [System.NonSerialized]
        protected List<EventDispatcher> eventDispatchers = new List<EventDispatcher>();


        public void RegisterEvent(EventDispatcher dispatcher) => eventDispatchers.Add(dispatcher);
        public void UnregisterEvent(EventDispatcher dispatcher) => eventDispatchers.Remove(dispatcher);
    }

    [CreateAssetMenu(menuName="MPack/Event/No Parameter", order=0)]
    public class EventReference : AbstractEventRefernece
    {
        private event System.Action triggerEvent;

        public void Invoke()
        {
            for (int i = eventDispatchers.Count - 1; i >= 0; i--)
                eventDispatchers[i].DispatchEvent();

            triggerEvent?.Invoke();
        }

        public void RegisterEvent(System.Action callback) => triggerEvent += callback;
        public void UnregisterEvent(System.Action callback) => triggerEvent -= callback;
    }
}