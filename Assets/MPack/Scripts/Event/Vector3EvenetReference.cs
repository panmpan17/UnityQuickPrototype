using UnityEngine;

namespace MPack
{
    [CreateAssetMenu(menuName="MPack/Event/Vector3", order=1)]
    public class Vector3EventReference : AbstractEventRefernece
    {
        private event System.Action<Vector3> triggerEvent;

        public void Invoke(Vector3 parameter)
        {
            for (int i = eventDispatchers.Count - 1; i >= 0; i--)
                eventDispatchers[i].DispatchEventWithVector3(parameter);

            triggerEvent?.Invoke(parameter);
        }

        public void RegisterEvent(System.Action<Vector3> callback) => triggerEvent += callback;
        public void UnregisterEvent(System.Action<Vector3> callback) => triggerEvent -= callback;
    }
}
