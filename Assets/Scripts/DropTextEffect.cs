using MPack;
using TMPro;
using UnityEngine;

public class DropTextEffect : MonoBehaviour
{
    [SerializeField]
    private GameObjectPoolReference pool;
    [SerializeField]
    private TextMeshPro text;
    [SerializeField]
    private Vector2 gravity;
    [SerializeField]
    private float drag;
    [SerializeField]
    private RangeStruct lifetime;
    private Timer m_lifetimeTimer;
    [SerializeField]
    private RangeStruct scaleRange;
    [SerializeField]
    private AnimationCurve scaleCurve;
    [SerializeField]
    private RangeStruct alphaRange;
    [SerializeField]
    private AnimationCurve alphaCurve;

    private Vector2 velocity;

    public void Drop(Vector3 position, string text, Vector2 velocity)
    {
        transform.position = position;
        this.text.text = text;
        this.velocity = velocity;
        m_lifetimeTimer = new Timer(lifetime.PickRandomNumber());
    }

    void Update()
    {
        velocity += gravity * Time.deltaTime;
        transform.position += (Vector3)velocity * Time.deltaTime;
        velocity *= 1 - drag * Time.deltaTime;

        UpdateDisplay();

        if (m_lifetimeTimer.UpdateEnd)
        {
            pool.Put(gameObject);
        }
    }

    void UpdateDisplay()
    {
        float t = m_lifetimeTimer.Progress;
        Color color = text.color;
        color.a = alphaRange.Lerp(alphaCurve.Evaluate(t));
        text.color = color;

        transform.localScale = Vector3.one * scaleRange.Lerp(scaleCurve.Evaluate(t));
    }
}
