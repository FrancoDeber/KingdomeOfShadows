using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using Unity.VisualScripting;

public class EnemyIA : MonoBehaviour, ILoreProvider
{
    public string loreName;
    public string GetLoreName() => loreName;
    public string lore;
    public string GetLore() => lore;
    private Animator animator;
    public Transform playerTransform; // Este lo usas solo para movimiento
    public Player player;            // Este lo usas para modificar stats
    private SpriteRenderer spriteRenderer;
    private bool spawnCooldown = true;



    [SerializeField] private float speed = 100f;
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float currentHealth;
    [SerializeField] private int experience;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float knockbackForce = 2f;
    private float currentSpeed = 0;  

    private bool isPassiveActive = false;
    private bool isInHitFeedback = false;

    private bool attacking = false;

    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(WaitSpawn());
        
        // Buscamos el componente Player en el mismo objeto que el transform
        if (playerTransform != null)
        {
            playerTransform = GameObject.FindWithTag("Player").GetComponent<Player>().transform;
            player = GameObject.FindWithTag("Player").GetComponent<Player>();
        }
    }

    void Update()
    {
        if (GameManager.Instance.cutscene) return; // Si esta en cutscene el personaje no hace nada
        if (isStunned) return;
        if (attacking) return;
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= detectionRange)
            {
                currentSpeed = speed;
                Vector2 direction = (playerTransform.position - transform.position).normalized;
                // Flip del sprite y arranca animacion de correr
                spriteRenderer.flipX = direction.x < 0f;
                animator.SetFloat("Speed", currentSpeed);

                Vector2 newPos = (Vector2)transform.position + direction * currentSpeed * Time.deltaTime;
                GetComponent<Rigidbody2D>().MovePosition(newPos);
            }
            else
            {
                animator.SetFloat("Speed", 0f); // Para detener animación si está fuera de rango
            }
        }
    }

    IEnumerator WaitSpawn()
    {
        yield return new WaitForSeconds(0.5f);
        spawnCooldown = false;
    }

    public void StopEntity()
    {
        animator.SetFloat("Speed", 0f);
        currentSpeed = 0f;
    }

    public void TakeDamage(float damage, Color color, Vector2 knockbackDirection, float knockbackForceReceived)
    {
        animator.SetTrigger("Hurt");
        StartCoroutine(Stun(0.3f)); // Pausa el movimiento por 0.2 segundos
        if (knockbackForceReceived > 0f)
            ApplyKnockback(knockbackDirection, knockbackForceReceived);
        
        if (player != null)
        {
            if (color == Color.red)
            {
                player.IncrementarStat(StatType.ShotsHit, 1f);
            }
            player.IncrementarStat(StatType.DamageDone, damage);
        }

        currentHealth -= damage;
        StartCoroutine(HitFeedback(color));

        if (currentHealth <= 0f)
        {
            StartCoroutine(Die());
        }
    }

    public void ApplyKnockback(Vector2 direction, float knockbackForce)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);
    }


    private bool isStunned = false;
    private IEnumerator Stun(float time)
    {
        isStunned = true;
        yield return new WaitForSeconds(time);
        rb.linearVelocity = Vector2.zero; // Detiene el movimiento residual del knockback
        isStunned = false;
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
            TakeDamage(damagePerTick, color, Vector2.zero , 0f);
            yield return new WaitForSeconds(tickRate);
            elapsed += tickRate;
        }
        isPassiveActive = false;
    }


    public IEnumerator Die(bool systemKill = false)
    {
        GetComponent<Collider2D>().enabled = false;
        if (player != null && !systemKill)
        {
            player.IncrementarStat(StatType.Kills, 1f);
            player.AddExperience(experience);
        }
        StopEntity();
        animator.SetTrigger("Death");
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }

    IEnumerator HitFeedback(Color color)
    {

        if (isInHitFeedback) yield break; // Evita solaparse

        isInHitFeedback = true;


        //Se detiene 0.2secs y vuelve a moverse
        currentSpeed = 0f;
        if (spriteRenderer != null)
            spriteRenderer.color = color;
        yield return new WaitForSeconds(0.2f);
        currentSpeed = speed;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
        
        isInHitFeedback = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Player collidedPlayer = collision.collider.GetComponent<Player>();
            if (collidedPlayer != null && !spawnCooldown)
            {
                Vector2 hitDirection = (collision.transform.position - transform.position).normalized;
                collidedPlayer.TakeDamage(damage, hitDirection, knockbackForce);
                // Flipeo la imagen para que quede mirando al jugador y hace animacion de ataque
                spriteRenderer.flipX = hitDirection.x < 0f;
                StartCoroutine(AttackPlayer());
            }
        }
    }

    IEnumerator AttackPlayer()
    {
        StopEntity();
        attacking = true;
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        currentSpeed = speed;
        animator.SetFloat("Speed", speed);
        attacking = false;
    }
}
