using UnityEngine;

public class Boundary : MonoBehaviour
{
    [SerializeField]
    Vector2 maxSize;
    [SerializeField]
    Vector2 spawnBorder;

    [SerializeField]
    int startingAsteroidCount = 3;
    [SerializeField]
    float randomSpeedMin = 1f;
    [SerializeField]
    float randomSpeedMax = 5f;
    [SerializeField]
    float randomDirectionMin = 10;
    [SerializeField]
    float randomDirectionMax = 90;

    int m_currentAsteroidCount = 0;
    float m_targetAsteroidCount = 0;

    [SerializeField]
    Asteroid[] asteroids;

    void Start()
    {
        m_targetAsteroidCount = startingAsteroidCount;

        for (int i = 0; i < asteroids.Length; i++)
        {
            asteroids[i].SetBoundary(this);
            asteroids[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < m_targetAsteroidCount; i++)
        {
            SpawnAsteroid();
        }
    }

    void Update()
    {
        if (m_currentAsteroidCount < m_targetAsteroidCount)
        {
            SpawnAsteroid();
        }
    }

    void SpawnAsteroid()
    {
        Vector2 position;

        if (Random.value < 0.5f)
        {
            float x = Random.Range(-spawnBorder.x / 2, spawnBorder.x / 2);
            position = new Vector2(x, Random.value < 0.5f ? spawnBorder.y / 2 : -spawnBorder.y / 2);
        }
        else
        {
            float y = Random.Range(-spawnBorder.y / 2, spawnBorder.y / 2);
            position = new Vector2(Random.value < 0.5f ? spawnBorder.x / 2 : -spawnBorder.x / 2, y);
        }
        position += (Vector2)transform.position;

        for (int i = 0; i < asteroids.Length; i++)
        {
            if (!asteroids[i].gameObject.activeInHierarchy)
            {
                Vector2 velocity = ((Vector2)transform.position - position).normalized * Random.Range(randomSpeedMin, randomSpeedMax);


                velocity = Quaternion.Euler(0, 0, Random.Range(randomDirectionMin, randomDirectionMax) * (Random.value < 0.5f ? 1 : -1)) * velocity;

                asteroids[i].gameObject.SetActive(true);
                asteroids[i].SetVelocity(position, velocity);
                m_currentAsteroidCount++;
                break;
            }
        }
    }

    public bool ShouldAsteroidHide(Asteroid asteroid)
    {
        Vector2 delta = (Vector2)transform.position - (Vector2)asteroid.transform.position;
        if (Mathf.Abs(delta.x) > maxSize.x / 2 || Mathf.Abs(delta.y) > maxSize.y / 2)
        {
            m_currentAsteroidCount--;
            asteroid.gameObject.SetActive(false);
            return true;
        }

        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(maxSize.x, maxSize.y, 0));
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnBorder.x, spawnBorder.y, 0));
    }
}
