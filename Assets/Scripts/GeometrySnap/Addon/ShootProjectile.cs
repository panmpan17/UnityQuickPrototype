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
    [SerializeField]
    private ProgressAnimationPart chargeAnimation;
    [SerializeField]
    private AudioClipSet shootSound;
    
    [Header("Rotate")]
    [SerializeField]
    private bool rotateToTarget;
    [SerializeField]
    private bool rotateThenWait;
    [SerializeField]
    private float rotateOffset;
    [SerializeField]
    private ValueWithEnable<float> slowRotate;

    [SerializeField]
    private TransformPointer target;

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
        if (!target || !target.HasTarget)
            return;
        
        if (rotateThenWait)
        {
            shootInterval.Update();
            chargeAnimation.UpdateProgress(shootInterval.Progress);
            if (rotateToTarget && !UpdateRotation())
            {
                return;
            }

            if (shootInterval.Ended)
            {
                shootInterval.Reset();
                Shoot();
            }
        }
        else
        {
            if (shootInterval.UpdateEnd)
            {
                if (rotateToTarget && !UpdateRotation())
                {
                    chargeAnimation.UpdateProgress(shootInterval.Progress);
                    return;
                }

                shootInterval.Reset();
                Shoot();
            }
            chargeAnimation.UpdateProgress(shootInterval.Progress);
        }

    }

    bool UpdateRotation()
    {
        Vector2 direction = target.Position - transform.position;
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
        Vector2 direction = target.Position - transform.position;
        direction.Normalize();

        GenericProjectile projectile = m_projectilePool.Get();
        projectile.transform.position = transform.TransformPoint(shootOffset);
        projectile.Shoot(direction);

        shootSound.PlayClipAtPoint(transform.position);
    }

    public override void ApplyPowerUp(AbstractPowerUpAddon[] powerUps)
    {
        
    }
}
