using System.Text;
using MPack;
using UnityEngine;

public interface IDamgeable
{
    void Hurt(float damge);
    void TriggerInFlame(float duration);
}

public class DummyEnemy : MonoBehaviour, IDamgeable
{
    [SerializeField]
    private GameObjectPoolReference pool;
    [SerializeField]
    private ParticleSystem flameParticle;
    [SerializeField]
    private Timer flameDamageTimer;

    private float m_flameDuration;

    void Update()
    {
        if (m_flameDuration > 0)
        {
            m_flameDuration -= Time.deltaTime;
            if (m_flameDuration <= 0)
            {
                flameParticle.Stop();
            }

            if (flameDamageTimer.UpdateEnd)
            {
                flameDamageTimer.Reset();
                Hurt(1);
            }
        }
    }

    public void Hurt(float damge)
    {
        GameObject dropText =  pool.Get();
        float xVelocity = Random.Range(5f, 8f);
        Vector2 velocty = new Vector2(Random.value > 0.5f ? xVelocity : -xVelocity, Random.Range(15f, 20f));
        StringBuilder sb = new StringBuilder();
        sb.Append("-");
        sb.Append(damge.ToString());
        dropText.GetComponent<DropTextEffect>().Drop(transform.position, sb.ToString(), velocty);
    }

    public void TriggerInFlame(float duration)
    {
        m_flameDuration = Mathf.Max(duration, m_flameDuration);
        flameParticle.Play();
    }
}
