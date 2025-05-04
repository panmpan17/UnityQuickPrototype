using UnityEngine;

public class WindSign : MonoBehaviour
{
    [SerializeField]
    private WindManager windManager;
    [SerializeField]
    private Transform rotateTarget;
    [SerializeField]
    private float rotationAngleOffset = -90;

    void Awake()
    {
        if (rotateTarget == null)
        {
            rotateTarget = transform;
        }

        if (windManager == null)
        {
            windManager = WindManager.Instance;
        }
    }

    void Update()
    {
        if (windManager == null)
        {
            return;
        }

        Vector2 windSpeed = windManager.CurrentWindSpeed;

        float angle = Mathf.Atan2(-windSpeed.y, windSpeed.x) * Mathf.Rad2Deg;
        rotateTarget.localRotation = Quaternion.Euler(0, 0, angle + rotationAngleOffset);

        Debug.DrawRay(rotateTarget.position, new Vector3(windSpeed.x, 0, windSpeed.y), Color.red);
    }

}
