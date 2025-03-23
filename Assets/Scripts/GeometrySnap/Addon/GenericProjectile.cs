using MPack;
using UnityEngine;

public class GenericProjectile : MonoBehaviour, IPoolableObj
{
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

    private Rigidbody2D m_rigidbody;

    private PrefabPool<GenericProjectile> m_pool;

    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
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
    }
}
