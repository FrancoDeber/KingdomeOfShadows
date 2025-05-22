using UnityEngine;

public class PowerUp : MonoBehaviour, ILoreProvider
{
    public string loreName;
    public string GetLoreName() => loreName;
    public string lore;
    public string GetLore() => lore;
    public PowerUpType[] options = new PowerUpType[3];

    void Start()
    {
        // Rellená con 3 tipos aleatorios
        for (int i = 0; i < options.Length; i++)
        {
            options[i] = (PowerUpType)Random.Range(0, System.Enum.GetValues(typeof(PowerUpType)).Length);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HUDManager.Instance.ShowPowerUpMenu(options, collision.GetComponent<Player>());
            Destroy(gameObject); // Destruí el powerup, el menú sigue
        }
    }
}
