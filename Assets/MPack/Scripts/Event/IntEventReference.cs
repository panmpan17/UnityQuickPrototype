using UnityEngine;


namespace MPack
{
    [CreateAssetMenu(menuName="MPack/Event/int", order=2)]
    public class IntEventReference : AbstractEventRefernece
    {
        private event System.Action<int> triggerEvent;

        public void Invoke(int parameter)
        {
            for (int i = eventDispatchers.Count - 1; i >= 0; i--)
                eventDispatchers[i].DispatchEventWithInt(parameter);

            triggerEvent?.Invoke(parameter);
        }

        public void RegisterEvent(System.Action<int> callback) => triggerEvent += callback;
        public void UnregisterEvent(System.Action<int> callback) => triggerEvent -= callback;
    }
}