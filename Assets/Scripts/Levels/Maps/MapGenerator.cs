using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{

    public static MapGenerator Instance;

    public int levelActual = 0;
    public string levelName = "LevelName";
    [SerializeField] private GameObject[] mapSections;  // Prefabs de las secciones del mapa
    [SerializeField] private GameObject bossMap; // Prefab del mapa del boss
    public Transform player;          // El jugador

    public float sectionHeight = 19.04628f; // Altura de cada sección del mapa
    public int preloadSections = 5;   // Número total de secciones precargadas

    public int maxNormalSections = 10;  // Número máximo de secciones normales antes del boss
    private int normalSectionsCount = 0; // Contador de secciones normales generadas
    private bool bossSectionSpawned = false; // Para evitar instanciar boss más de una vez

    private float nextSpawnY = 0f;    // Posición Y para la próxima sección
    private Queue<GameObject> activeSections = new Queue<GameObject>(); // Cola para almacenar las secciones activas

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
        Init();
        player = GameObject.FindWithTag("Player").GetComponent<Player>().transform;

        // Generar las secciones adelante
        for (int i = 0; i < preloadSections - 1; i++) // Deja espacio para la sección actual
        {
            SpawnSection(Camera.main.transform, true); // Genera las secciones hacia adelante
        }
    }

    void Update()
    {
        // Verifica si el jugador ha alcanzado la próxima posición donde se debe generar una nueva sección
        if (player.position.y + sectionHeight > nextSpawnY)
        {
            // Genera una nueva sección adelante
            SpawnSection(player);

            // Elimina las secciones antiguas para evitar sobrecargar el juego
            if (activeSections.Count > preloadSections + 1) // Mantén siempre una sección atrás, la actual y 3 adelante
            {
                Destroy(activeSections.Dequeue());
            }
        }
    }

    public void Init()
    {
        sectionHeight = mapSections[0].GetComponent<SpriteRenderer>().bounds.size.y;
    }

    void SpawnSection(Transform objObjetivo, bool behind = false)
    {
        if (bossSectionSpawned) return;
        GameObject sectionPrefab;

        // Si es una sección detrás, la colocamos un poco antes que el jugador
        if (behind)
        {
            nextSpawnY = objObjetivo.position.y - sectionHeight;  // Genera una sección justo detrás del jugador
            // Escoge una sección aleatoria del array de secciones
            sectionPrefab = mapSections[Random.Range(0, mapSections.Length)];
        }
        else
        {

            nextSpawnY += sectionHeight; // De lo contrario, genera la nueva sección adelante
            int cantItems = Random.Range(1, 4);
            for (int i = 0; i < cantItems; i++)
            {
                ItemsGenerator.Instance.SpawnRandomItem(nextSpawnY);
            }
            int cantEnemies = Random.Range(5, 10);
            for (int i = 0; i < cantEnemies; i++)
            {
                EnemyGenerator.Instance.createRandomEnemy(nextSpawnY);
            }

            if (!bossSectionSpawned && normalSectionsCount >= maxNormalSections)
            {
                // Instancia la sección del boss
                sectionPrefab = bossMap;
                bossSectionSpawned = true;
                CameraFollowVertical.Instance.maxY = nextSpawnY + sectionHeight / 2;
                CameraFollowVertical.Instance.useMaxY = true;

            }
            else
            {
                sectionPrefab = mapSections[Random.Range(0, mapSections.Length)];
                normalSectionsCount++;

            }
        }
        
        Spawn(sectionPrefab);
    }

    void Spawn(GameObject sectionPrefab)
    {
        // Calcula la posición en Y para la nueva sección
        Vector2 spawnPos = new Vector2(0, nextSpawnY);

        // Instancia la nueva sección en la posición calculada
        GameObject section = Instantiate(sectionPrefab, spawnPos, Quaternion.identity);
        // Añade la nueva sección a la cola de secciones activas
        activeSections.Enqueue(section);
    }
}
