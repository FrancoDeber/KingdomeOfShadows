using UnityEngine;
using System.Collections;

public class BossIA : MonoBehaviour, ILoreProvider
{
    public string GetLoreName() => bossName;
    public string lore;
    public string GetLore() => lore;
    public Transform playerTransform; // Este lo usas solo para movimiento
    public Player player;            // Este lo usas para modificar stats

    //Info
    [SerializeField] private string bossName = "BossNameDefault";
    [SerializeField] private string bossTitle = "BossTitleDefault";
    [SerializeField] private float speed = 2f;
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float currentHealth;
    [SerializeField] private int experience;
    public bool isDead = false;

    // Shot
    public bool isPowerUpMenuOpen = false;
    [SerializeField] private GameObject bulletPrefab;
    public float shotSpeed = 10f;
    public float shotSize = 0.2f; // Tamaño escalable desde el Inspector
    public float shotRate = 0.8f; // Tiempo de disparo
    private float lastShotTime = 0f;
    private Vector2 shotDirection = Vector2.zero;
    private Vector2 moveDirection;
    public int damage = 1;
    public float shotRange = 5f;
    public float shotKnockback = 1f;
    public BulletPassiveType bulletPassive; // Pasiva de la bala

    private SpriteRenderer spriteRenderer;

    private bool isPassiveActive = false;
    private bool isInHitFeedback = false;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerTransform = GameObject.FindWithTag("Player").GetComponent<Player>().transform;
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        HUDManager.Instance.SetBossName(bossName+ "\n" + bossTitle);
    }

    void Update()
    {
        if (!BossZoneTrigger.Instance.playerInside) return;// Si no entro el personaje en la zona no hace nada
        if (GameManager.Instance.cutscene) return; // Si esta en cutscene el personaje no hace nada
        
        if (playerTransform != null && !isDead)
        {
            shotDirection = (playerTransform.position - transform.position).normalized;
            if (shotDirection != Vector2.zero && Time.time - lastShotTime >= shotRate)
            {
                // Compruebo si hay algun menu que este abierto para no disparar
                if (HUDManager.Instance.IsAnyMenuOpen()) return;
                Shoot(shotDirection);
                lastShotTime = Time.time;
            }
        }
    }

    public void Init()
    {

        StartCoroutine(SpawnEnemies());
    }

    public void TakeDamage(float damage, Color color)
    {
        if (isDead) return;
        if (player != null)
        {
            if (color == Color.red)
            {
                player.IncrementarStat(StatType.ShotsHit, 1f);
            }
            player.IncrementarStat(StatType.DamageDone, damage);
        }

        currentHealth -= damage;
        animator.SetTrigger("Hurt");
        StartCoroutine(HitFeedback(color));
        float lifeRest = Mathf.Clamp01((float)currentHealth / maxHealth);
        HUDManager.Instance.AnimateBossHealthBar(lifeRest);

        if (currentHealth <= 0f)
        {
            StartCoroutine(Die());
        }
    }

    void Shoot(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // Asigna los datos a la bala
        Bala bulletScript = bullet.GetComponent<Bala>();
        if (bulletScript != null)
        {
            bulletScript.damage = damage;
            bulletScript.shotRange = shotRange;
            bulletScript.passive = bulletPassive;
            bulletScript.fromEnemy = true;
            bulletScript.knockbackForce = 4f;
        }

        // Cambiar tamaño
        bullet.transform.localScale = Vector3.one * shotSize;

        // Rotar la bala para que mire hacia la dirección de disparo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Aplicar dirección y velocidad
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            Vector2 finalDirection = direction;

            // Si hay movimiento y no es opuesto al disparo
            if (moveDirection != Vector2.zero && Vector2.Dot(moveDirection, direction) >= 0)
            {
                finalDirection += moveDirection * 0.5f;
            }

            bulletRb.linearVelocity = finalDirection.normalized * shotSpeed;
        }
    }

    public void ApplyPassive(BulletPassiveType passive)
    {
        if (isPassiveActive) return;
        if (passive == BulletPassiveType.None) return;
        isPassiveActive = true;
        StartCoroutine(PassiveDamage(passive));
    }

    private IEnumerator PassiveDamage(BulletPassiveType passiveType)
    {

        float damagePerTick = BulletPassiveInfo.Data[passiveType].dotDamage;
        float duration = BulletPassiveInfo.Data[passiveType].dotDuration;
        float tickRate = BulletPassiveInfo.Data[passiveType].dotTickRate;
        Color color = BulletPassiveInfo.Data[passiveType].color;
        
        float elapsed = 0f;

        while (elapsed < duration)
        {
            TakeDamage(damagePerTick, color);
            yield return new WaitForSeconds(tickRate);
            elapsed += tickRate;
        }
        isPassiveActive = false;
    }

    private IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(2f);
        while (!isDead)
        {
            animator.SetTrigger("Attack");
            yield return new WaitForSeconds(0.5f);
            int enemyNumber = Random.Range(1, 3);
            EnemyGenerator.Instance.createBossEnemy(enemyNumber, transform.position);
            int cooldown = Random.Range(5, 11);
            yield return new WaitForSeconds(cooldown);
        }
    }

    private IEnumerator Die()
    {
        isDead = true;
        player.StopPlayer();
        EnemyGenerator.Instance.EliminarTodosLosEnemigos();
        animator.SetTrigger("Death");
        yield return StartCoroutine(CutsceneManager.Instance.BossDeathCutscene());
        player.AddExperience(experience);
        player.IncrementarStat(StatType.BossKills, 1f);
        GetComponent<BoxCollider2D>().enabled = false;
        yield return null;
    }

    


    IEnumerator HitFeedback(Color color)
    {

        if (isInHitFeedback) yield break; // Evita solaparse

        isInHitFeedback = true;

        float originalSpeed = speed;
        speed = 0f;

        if (spriteRenderer != null)
            spriteRenderer.color = color;

        yield return new WaitForSeconds(0.2f);

        speed = originalSpeed;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
        
        isInHitFeedback = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.collider.CompareTag("Player"))
        {
            Player collidedPlayer = collision.collider.GetComponent<Player>();
            if (collidedPlayer != null)
            {
                Vector2 hitDirection = collision.transform.position - transform.position;
                collidedPlayer.TakeDamage(damage, hitDirection, 4f);
            }
        }
    }
}
