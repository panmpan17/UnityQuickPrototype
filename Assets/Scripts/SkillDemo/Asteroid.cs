using UnityEngine;

public class Asteroid : MonoBehaviour
{
    Boundary m_boundary;
    Vector2 m_velocity;

    public void SetBoundary(Boundary boundary)
    {
        m_boundary = boundary;
    }

    public void SetVelocity(Vector2 position, Vector2 velocity)
    {
        transform.position = position;
        m_velocity = velocity;
    }

    void Update()
    {
        transform.position += (Vector3)m_velocity * Time.deltaTime;

        m_boundary.ShouldAsteroidHide(this);
    }
}
