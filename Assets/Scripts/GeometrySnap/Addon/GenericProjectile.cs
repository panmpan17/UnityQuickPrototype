using MPack;
using UnityEngine;

public class GenericProjectile : MonoBehaviour, IPoolableObj
{
    [SerializeField]
    private int damgePoint;
    [SerializeField]
    private float speed;
    [SerializeField]
    private Timer lifeTime;
    [SerializeField]
    private float rotationOffset = -90;
    [SerializeField]
    private EffectReference hitEffect;
    [SerializeField]
    private AudioClipSet hitSound;

    [Header("Special Effects")]
    [SerializeField]
    private SpriteRenderer mainSprite;
    [SerializeField]
    private Color burningColor;
    private Color m_defaultColor;
    [SerializeField]
    private ParticleSystem burningEffect;
    [SerializeField]
    private int[] burningDamges;
    [SerializeField]
    private float[] burningDurations;
    private int m_burningLevel; 

    private Rigidbody2D m_rigidbody;

    private PrefabPool<GenericProjectile> m_pool;

    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_defaultColor = mainSprite.color;
    }

    public void DeactivateObj(Transform collectionTransform)
    {
        gameObject.SetActive(false);
        m_rigidbody.linearVelocity = Vector2.zero;
        m_rigidbody.angularVelocity = 0;
    }

    public void Instantiate()
    {
    }

    public void SetPool(PrefabPool<GenericProjectile> pool)
    {
        m_pool = pool;
    }

    public void Reinstantiate()
    {
        gameObject.SetActive(true);
        lifeTime.Reset();
    }

    public void SetupPowerUps(int burnCount)
    {
        m_burningLevel = burnCount;
        if (burnCount > 0)
        {
            mainSprite.color = burningColor;
            burningEffect.Play();
        }
        else
        {
            mainSprite.color = m_defaultColor;
            burningEffect.Stop();
        }
    }

    public void Shoot(Vector2 direction, float speedMultiplier=1)
    {
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset);
        m_rigidbody.linearVelocity = (speed * speedMultiplier) * direction;
    }

    void Update()
    {
        if (lifeTime.UpdateEnd)
        {
            if (m_pool != null)
                m_pool.Put(this);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_pool != null)
            m_pool.Put(this);
        
        if (hitEffect)
        {
            ContactPoint2D contact = collision.GetContact(0);
            Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(contact.normal.y, contact.normal.x) * Mathf.Rad2Deg);
            hitEffect.AddWaitingList(contact.point, rotation);
        }

        if (hitSound)
            hitSound.PlayClipAtPoint(transform.position);
        
        var damgeable = collision.collider.GetComponent<IDamgeable>();
        if (damgeable != null)
        {
            damgeable.Hurt(damgePoint);
            if (m_burningLevel > 0)
            {
                float duration = burningDurations[Mathf.Min(m_burningLevel - 1, burningDurations.Length - 1)];
                damgeable.TriggerInFlame(duration);
            }
        }
    }
}
