using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{

    public static EnemyGenerator Instance;


    public GameObject[] enemyPrefabs; // Array de prefabs de enemigos
    public float spawnRangeX = 3f;
    public float spawnOffsetY = 19.04628f;

    public Transform playerTransform; // Asignalo desde el Inspector

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
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindWithTag("Player")?.transform;
        }
    }

    public void createRandomEnemy(float minY)
    {
        const int maxAttempts = 20;
        const float minDistance = 2f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 spawnPos = new Vector2(
                Random.Range(-spawnRangeX, spawnRangeX),
                Random.Range(minY, minY + spawnOffsetY)
            );

            // Verificar si hay otro enemigo cerca
            if (!IsPositionOccupied(spawnPos, minDistance))
            {
                GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

                EnemyIA ai = enemy.GetComponent<EnemyIA>();
                if (ai != null)
                {
                    ai.playerTransform = playerTransform;
                }
                return;
            }
        }

        Debug.LogWarning("No se pudo encontrar una posición válida para spawnear un enemigo.");
    }

    private bool IsPositionOccupied(Vector2 position, float radius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                return true;
            }
        }
        // Revisar cercanía con el jugador
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(position, playerTransform.position);
            if (distanceToPlayer < radius)
            {
                return true;
            }
        }

        return false;
    }

    /* public void createRandomEnemy()
    {
        Vector2 spawnPos = new Vector2(
            Random.Range(-3f, 3f),
            transform.position.y + Random.Range(0f, 5f)
        );

        // Elegir un prefab aleatorio
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // Instanciar el enemigo
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Asignar el jugador como objetivo si tiene EnemyAI
        EnemyIA ai = enemy.GetComponent<EnemyIA>();
        if (ai != null)
        {
            ai.playerTransform = playerTransform;
        }
    } */

    public void createBossEnemy(int cant, Vector2 bossPosition)
    {
        float radioMaximo = 5f;

        for (int i = 0; i < cant; i++)
        {
            // Obtener una posición aleatoria alrededor del bossPosition
            Vector2 offset = Random.insideUnitCircle * radioMaximo;
            Vector2 spawnPos = bossPosition + offset;

            // Elegir un prefab aleatorio
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            // Instanciar el enemigo
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Asignar el jugador como objetivo si tiene EnemyIA
            EnemyIA ai = enemy.GetComponent<EnemyIA>();
            if (ai != null)
            {
                ai.playerTransform = playerTransform;
            }
        }
    }

    public void EliminarTodosLosEnemigos()
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemigo in enemigos)
        {
            StartCoroutine(enemigo.GetComponent<EnemyIA>().Die(true));
        }
    }


}
