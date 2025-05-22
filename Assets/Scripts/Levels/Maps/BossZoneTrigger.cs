using UnityEngine;
using System.Collections;

public class BossZoneTrigger : MonoBehaviour
{
    public static BossZoneTrigger Instance;

    private Player player;

    [SerializeField] private BossIA boss;

    public Camera mainCamera;
    public Transform cameraPoint;
    public Transform targetPlayerPos;
    [SerializeField] private GameObject bottomCollider;
    [SerializeField] private GameObject topCollider;

    [SerializeField] private Transform chestPos;
    [SerializeField] private Transform nextLevelPos;

    public bool playerInside = false;
    public bool bossIsAlive = true;


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
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (boss == null)
            boss = GetComponent<BossIA>();

        PowerUpGenerator.Instance.SpawnPowerUp(chestPos.position);
        HUDManager.Instance.AnimateBossHealthBar(1f);

    }

    void Update()
    {
        if (playerInside && cameraPoint != null && bossIsAlive)
        {
            CameraFollowVertical.Instance.cameraPositionBoss(cameraPoint);
            //mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, cameraPoint.position, smoothSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && bossIsAlive)
        {
            
            StartCoroutine(BossFight());
            // Opcional: bloquear el seguimiento, desactivar scripts de cámara, etc.
        }
    }

    IEnumerator BossFight(){
        playerInside = true;
        EnemyGenerator.Instance.EliminarTodosLosEnemigos();
        yield return StartCoroutine(CutsceneManager.Instance.BossEnterCutscene());
        activateLimits(true);
        boss.Init();
    }

    public void activateLimits(bool state)
    {
        bottomCollider.SetActive(state);
        topCollider.SetActive(state);
    }
}
