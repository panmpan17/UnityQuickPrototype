using UnityEngine;

namespace MPack
{
    [CreateAssetMenu(menuName="MPack/Event/Vector2", order=1)]
    public class Vector2EventReference : AbstractEventRefernece
    {
        private event System.Action<Vector2> triggerEvent;

        public void Invoke(Vector2 parameter)
        {
            for (int i = eventDispatchers.Count - 1; i >= 0; i--)
                eventDispatchers[i].DispatchEventWithVector2(parameter);

            triggerEvent?.Invoke(parameter);
        }

        public void RegisterEvent(System.Action<Vector2> callback) => triggerEvent += callback;
        public void UnregisterEvent(System.Action<Vector2> callback) => triggerEvent -= callback;
    }
}
