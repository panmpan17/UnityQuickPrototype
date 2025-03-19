using MPack;
using UnityEngine;

public class ShootProjectile : AbstractWeaponAddon
{
    [SerializeField]
    private GenericProjectile projectilePrefab;
    private PrefabPool<GenericProjectile> m_projectilePool;
    [SerializeField]
    private Timer shootInterval;
    [SerializeField]
    private Vector2 shootOffset;
    
    [Header("Rotate")]
    [SerializeField]
    private bool rotateToTarget;
    [SerializeField]
    private float rotateOffset;
    [SerializeField]
    private ValueWithEnable<float> slowRotate;

    [SerializeField]
    private Transform m_target;

    private AbstractPowerUpAddon[] m_powerUps;

    void Awake()
    {
        m_projectilePool = new PrefabPool<GenericProjectile>(projectilePrefab);
        m_projectilePool.OnInstantiate = (IPoolableObj obj) =>
        {
            ((GenericProjectile)obj).SetPool(m_projectilePool);
        };
    }

    void Update()
    {
        if (!m_target)
            return;
        
        if (shootInterval.UpdateEnd)
        {
            if (rotateToTarget && !UpdateRotation())
            {
                return;
            }

            shootInterval.Reset();
            Shoot();
        }
    }

    bool UpdateRotation()
    {
        Vector2 direction = m_target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += rotateOffset;

        if (slowRotate.Enable)
        {
            float currentAngle = transform.rotation.eulerAngles.z;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, angle, slowRotate.Value * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, newAngle);

            return Mathf.Abs(newAngle - angle) < 0.1f;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, angle);
            return true;
        }
    }

    void Shoot()
    {
        Vector2 direction = m_target.position - transform.position;
        direction.Normalize();

        GenericProjectile projectile = m_projectilePool.Get();
        projectile.transform.position = transform.TransformPoint(shootOffset);
        projectile.Shoot(direction);
    }

    public override void ApplyPowerUp(AbstractPowerUpAddon[] powerUps)
    {
        
    }
}
