using UnityEngine;

public class PowerUpGenerator : MonoBehaviour
{
    public static PowerUpGenerator Instance;

    public GameObject[] powerUpPrefabs; // Prefabs de power-ups

    public Transform player; // Referencia al jugador
    public float spawnRangeX = 3f;
    public float spawnOffsetY = 19.04628f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
        }

    }
    

    void Update()
    {

    }

    void SpawnRandomPowerUp()
    {
        Vector2 spawnPos = new Vector2(
            Random.Range(-spawnRangeX, spawnRangeX),
            transform.position.y + Random.Range(0f, spawnOffsetY)
        );

        GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    public void SpawnPowerUp(Vector2 spawnPos)
    {
        GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}
