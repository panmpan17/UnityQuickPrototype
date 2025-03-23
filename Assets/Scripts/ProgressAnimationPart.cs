using MPack;
using UnityEngine;

public class ProgressAnimationPart : MonoBehaviour
{
    [SerializeField]
    private Transform defaultTarget;
    [SerializeField]
    private AnimationCurveReference defaultCurve;

    [Header("Scale")]
    [SerializeField]
    private bool scaling;
    [SerializeField]
    private bool localScale = true;
    [SerializeField]
    private Transform scaleTarget;
    [SerializeField]
    private Vector3 startScale, endScale;
    public Transform ScaleTarget { get { return scaleTarget ? scaleTarget : defaultTarget; } }
    public Vector3 Scale {
        get { return localScale ? ScaleTarget.localScale : ScaleTarget.lossyScale; }
        set {
            if (localScale) ScaleTarget.localScale = value;
            else
            {
                Vector3 scale = ScaleTarget.localScale;
                scale.x = value.x / ScaleTarget.lossyScale.x;
                scale.y = value.y / ScaleTarget.lossyScale.y;
                scale.z = value.z / ScaleTarget.lossyScale.z;
                ScaleTarget.localScale = scale;
            }
        }
    }
    public AnimationCurveReference ScaleCurve => defaultCurve;

    [Header("Color")]
    [SerializeField]
    private bool coloring;
    [SerializeField]
    private Renderer targetRenderer;
    [SerializeField]
    private Color startColor, endColor;
    // [SerializeField]
    // private AnimationCurveReference colorCurve;
    public AnimationCurveReference ColorCurve => defaultCurve;

    void Awake()
    {
        if (!defaultTarget)
        {
            defaultTarget = transform;
        }

        if (coloring)
        {
            InitColor();
        }
    }

    void InitColor()
    {
        if (!targetRenderer)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (!targetRenderer)
        {
            coloring = false;
            return;
        }
    }

    public void UpdateProgress(float progress)
    {
        if (scaling)
        {
            float evaluateValue = ScaleCurve.Value.Evaluate(progress);
            Scale = Vector3.Lerp(startScale, endScale, evaluateValue);
        }

        if (coloring)
        {
            targetRenderer.material.color = Color.Lerp(startColor, endColor, ColorCurve.Value.Evaluate(progress));
        }
    }
}
