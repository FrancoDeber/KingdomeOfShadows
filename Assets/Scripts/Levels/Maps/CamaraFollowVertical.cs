using UnityEngine;

public class CameraFollowVertical : MonoBehaviour
{
    public static CameraFollowVertical Instance;
    public Transform player; // Asigna el jugador en el inspector
    public float smoothSpeed = 0.125f; // Controla la suavidad del movimiento
    [SerializeField] float offset = 1.25f; // Desplazamiento desde la cámara al jugador
    public bool enableCameraFollow = false;
    [SerializeField] float minY = -6.52f; // Limite inferior
    public float maxY; // Limite superior
    public bool useMaxY = false;

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
        player = GameObject.FindWithTag("Player").GetComponent<Player>().transform;
    }

    void LateUpdate()
    {
        // Solo seguir el movimiento en el eje Y, sin mover el eje X ni Z
        if (player == null) return;
        if (enableCameraFollow == false) return;

        // Posición deseada con offset
        float targetY = player.position.y + offset;

        // Aplicar el límite mínimo
        targetY = Mathf.Max(targetY, minY);

        // Solo aplicar maxY si está habilitado
        if (useMaxY)
            targetY = Mathf.Min(targetY, maxY-3f);

        Vector3 desiredPosition = new Vector3(transform.position.x, targetY, transform.position.z);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    public void cameraPositionBoss(Transform cameraPoint)
    {
        transform.position = Vector3.Lerp(transform.position, cameraPoint.position, 2f * Time.deltaTime);
    }

}
