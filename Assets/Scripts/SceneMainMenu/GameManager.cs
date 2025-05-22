using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Player player;
    private HUDManager hudManager;
    public bool cutscene = false;
    public bool gameStart = false;
    public int levelActual = 0;
    private float playedTime = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Update(){
        // Solo sumamos tiempo si el juego no está pausado
        if (Time.timeScale > 0 && !cutscene && gameStart)
        {
            playedTime += Time.unscaledDeltaTime;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void SetVisual(Player obj, float escala, float alpha)
    {
        obj.transform.localScale = Vector3.one * escala;

        var renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers)
        {
            Color c = r.color;
            c.a = alpha;
            r.color = c;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        CutsceneManager.Instance.Init();
        if (scene.name.StartsWith("Level"))
        {
            player.gameObject.SetActive(true);
            //Cargo hudManager con el HUDManger del Level01 solo en su primer inicio
            if (scene.name == "Level01" && !gameStart)
            {
                gameStart = true;
                if (hudManager == null)
                {
                    hudManager = FindFirstObjectByType<HUDManager>();

                }
            }
            levelActual = MapGenerator.Instance.levelActual;
            CutsceneManager.Instance.InitLevel();

            // Player hace su Init
            if (player != null)
            {
                player.GetComponent<Player>().Init();
                SetVisual(player, 1.5f, 1f);
            }
            // HudManager hace su Init
            if (hudManager != null)
            {
                hudManager.Init();
            }

        }
        else
        {
            StartCoroutine(CutsceneManager.Instance.EnterSceneSequence());
        }
        
    }

    public void BackToMainMenu()
    {
        // Destruyo los Objetos DontDestroyOnLoad
        if(HUDManager.Instance.gameObject != null)
            Destroy(HUDManager.Instance.gameObject);
        if(CutsceneManager.Instance.gameObject != null) 
            Destroy(CutsceneManager.Instance.gameObject);
        if(Player.Instance.gameObject != null) 
            Destroy(Player.Instance.gameObject);
        cutscene = false;
        levelActual = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public string GetPlayedTime(){
        int minutes = Mathf.FloorToInt(playedTime / 60f);
        int seconds = Mathf.FloorToInt(playedTime % 60f);
        string tiempoFormateado = $"{minutes:00}:{seconds:00}";
        return tiempoFormateado;
    }
}