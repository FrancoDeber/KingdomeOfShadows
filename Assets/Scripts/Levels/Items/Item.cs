using UnityEngine;

public class Item : MonoBehaviour, ILoreProvider
{
    public string loreName;
    public string GetLoreName() => loreName;
    public string lore;
    public string GetLore() => lore;
    [SerializeField] private ItemType itemType;
    [SerializeField] private int value;

    void Start()
    {
        if (itemType == ItemType.EXP)
            value = Random.Range(20, 101);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().ApplyItem(itemType, value);
            Destroy(gameObject); // Destruí el powerup
        }
    }
}
