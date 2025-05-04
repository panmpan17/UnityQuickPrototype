using MPack;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorReceiver : MonoBehaviour
{
    public IntUnityEvent onAnimationEventTriggered;

    public void TriggerAnimationEvent(int eventType)
    {
        Debug.Log($"Animation event triggered with type: {eventType}");
        onAnimationEventTriggered?.Invoke(eventType);
    }
}
