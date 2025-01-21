using UnityEngine;


namespace MPack
{
    [CreateAssetMenu(menuName="MPack/Event/string", order=4)]
    public class StringEventReference : AbstractEventRefernece
    {
        private event System.Action<string> triggerEvent;

        public void Invoke(string parameter)
        {
            for (int i = eventDispatchers.Count - 1; i >= 0; i--)
                eventDispatchers[i].DispatchEventWithString(parameter);

            triggerEvent?.Invoke(parameter);
        }

        public void RegisterEvent(System.Action<string> callback) => triggerEvent += callback;
        public void UnregisterEvent(System.Action<string> callback) => triggerEvent -= callback;
    }
}