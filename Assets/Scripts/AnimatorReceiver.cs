using MPack;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorReceiver : MonoBehaviour
{
    public IntUnityEvent onAnimationEventTriggered;

    public void TriggerAnimationEvent(int eventType)
    {
        onAnimationEventTriggered?.Invoke(eventType);
    }
}
