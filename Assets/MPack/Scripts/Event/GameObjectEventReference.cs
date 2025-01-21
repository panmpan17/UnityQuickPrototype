using UnityEngine;

namespace MPack
{
    [CreateAssetMenu(menuName="MPack/Event/GameObject", order=1)]
    public class GameObjectEventReference : AbstractEventRefernece
    {
        private event System.Action<GameObject> triggerEvent;

        public void Invoke(GameObject parameter)
        {
            for (int i = eventDispatchers.Count - 1; i >= 0; i--)
                eventDispatchers[i].DispatchEventWithGameObject(parameter);

            triggerEvent?.Invoke(parameter);
        }

        public void RegisterEvent(System.Action<GameObject> callback) => triggerEvent += callback;
        public void UnregisterEvent(System.Action<GameObject> callback) => triggerEvent -= callback;
    }
}
