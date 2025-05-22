using UnityEngine;

public class ItemsGenerator : MonoBehaviour
{
    public static ItemsGenerator Instance;

    public GameObject[] itemsPrefabs; // Prefabs de items

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

    public void SpawnRandomItem(float minY)
    {
        Vector2 spawnPos = new Vector2(
            Random.Range(-spawnRangeX, spawnRangeX),
            Random.Range(minY, minY + spawnOffsetY)
        );

        GameObject prefab = itemsPrefabs[Random.Range(0, itemsPrefabs.Length)];

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    public void SpawnItem(Vector2 spawnPos)
    {
        GameObject prefab = itemsPrefabs[Random.Range(0, itemsPrefabs.Length)];

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}
