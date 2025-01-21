using UnityEngine;


namespace MPack
{
    [CreateAssetMenu(menuName="MPack/Event/float", order=3)]
    public class FloatEventReference : AbstractEventRefernece
    {
        private event System.Action<float> triggerEvent;

        public void Invoke(float parameter)
        {
            for (int i = eventDispatchers.Count - 1; i >= 0; i--)
                eventDispatchers[i].DispatchEventWithFloat(parameter);

            triggerEvent?.Invoke(parameter);
        }

        public void RegisterEvent(System.Action<float> callback) => triggerEvent += callback;
        public void UnregisterEvent(System.Action<float> callback) => triggerEvent -= callback;
    }
}