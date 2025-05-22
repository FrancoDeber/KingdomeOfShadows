using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour, ILoreProvider
{
    public static Player Instance;

    // Info
    public string characterName;
    public string characterTitleName;
    public string characterSecondTitleName;
    public string GetLoreName() => characterName;
    public string lore;
    public string GetLore() => lore;
    public float lateralScale;
    public float centerScale;
    public float gameScale;
    public bool isPreview = true;
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;
    public int lifes = 3;
    [SerializeField] private GameObject dashEffect;
    private Animator animator;

    // Fórmula de escalado (opcional)
    private float growthFactor = 1.2f;   // 20 % más XP por nivel

    // Movement
    public float moveSpeed = 3f;
    public Rigidbody2D rb;
    // Dash
    public float dashSpeed = 20f;
    public float dashDistance = 0.3f;
    public int maxDashCount = 2;
    private int maxDashLimit = 6;
    public int currentDashCount;
    public float dashRechargeTime = 2f;
    private bool isDashing = false;
    private Queue<float> dashRechargeQueue = new Queue<float>();
    private bool isRecharging = false;

    // Health
    public int maxHealth = 5;
    public int maxHealthLimit = 10;
    public int currentHealth = 3;
    private SpriteRenderer spriteRenderer;
    private bool isInvulnerable = false;
    public float invulnerabilityTime = 0.5f;
    public float knockbackResistance = 1f;
    private bool isPassiveActive = false;

    // Shot
    public GameObject bulletPrefab;
    public float shotSpeed = 10f;
    public float shotSize = 0.2f; // Tamaño escalable desde el Inspector
    public float shotRate = 0.8f; // Tiempo de disparo
    private float lastShotTime;
    private Vector2 shotDirection = Vector2.zero;
    private Vector2 moveDirection;
    public float damage = 1f;
    public float shotRange = 5f;
    public float shotKnockback = 1f;
    public BulletPassiveType bulletPassive; // Pasiva de la bala

    // Stats
    public int bossKills = 0;
    public int kills = 0;
    public float damageDone = 0;
    public int shotsFired = 0;
    public int shotsHit = 0;
    public int dashesDone = 0;
    public float damageReceived = 0;
    public float healReceived = 0;
    public int powerUpsTaken = 0;
    public int deaths = 0;
    public float playtime = 0;



    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Init()
    {
        Instance = this;
        ResetPlayer();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        transform.position = new Vector3(0f, -5f, 0f);
        rb.position = new Vector2(0f, -5f);
        rb.linearVelocity = Vector2.zero;
        StartCoroutine(MovePlayerToTarget(new Vector2(0f, -1.5f), 2f));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            maxHealth = 100;
            currentHealth = 100;
        }

        if (isPreview) return; // Si esta en la escena de seleccion de personaje no hace nada


        if (GameManager.Instance.cutscene) return; // Si esta en cutscene el personaje no hace nada


        // ** MOVIMIENTO GENERAL **
        if (isStunned) return;
        if (!isDashing)
        {
            float speed = rb.linearVelocity.magnitude;
            animator.SetFloat("Speed", speed);
            // Movimiento vertical y lateral con input
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");

            if (inputX > 0)
                spriteRenderer.flipX = false;  // mirando a la derecha (normal)
            else if (inputX < 0)
                spriteRenderer.flipX = true;   // mirando a la izquierda (espejado)

            Vector2 moveDir = new Vector2(inputX * moveSpeed, inputY * moveSpeed);
            rb.linearVelocity = moveDir;

            // Dirección de movimiento normalizada (para el momentum)
            moveDirection = new Vector2(inputX, inputY).normalized;

        }

        // ** MOVIMIENTO GENERAL **

        // ** DASH **
        if (Input.GetKeyDown(KeyCode.E) && currentDashCount > 0 && !isDashing)
        {
            StartCoroutine(PerformDash());
        }

        // ** DASH **        

        // ** DISPARO **

        shotDirection = Vector2.zero;

        if (Input.GetKey(KeyCode.UpArrow))
            shotDirection += Vector2.up;
        if (Input.GetKey(KeyCode.DownArrow))
            shotDirection += Vector2.down;
        if (Input.GetKey(KeyCode.LeftArrow))
            shotDirection += Vector2.left;
        if (Input.GetKey(KeyCode.RightArrow))
            shotDirection += Vector2.right;

        shotDirection = shotDirection.normalized;

        // Disparar constantemente si se mantiene la tecla
        if (shotDirection != Vector2.zero && Time.time - lastShotTime >= shotRate)
        {
            // Compruebo si hay algun menu que este abierto para no disparar
            if (HUDManager.Instance.IsAnyMenuOpen()) return;
            Shoot(shotDirection);
            lastShotTime = Time.time;
        }

        // ** DISPARO **
    }

    private void ResetPlayer()
    {
        StopPlayer();
        currentHealth = maxHealth;
        currentDashCount = maxDashCount;
        isDashing = false;
        isRecharging = false;
        dashRechargeQueue.Clear();
        isInvulnerable = false;
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
            bulletScript.knockbackForce = shotKnockback;
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
                finalDirection += moveDirection * 0.1f;
            }

            bulletRb.linearVelocity = finalDirection.normalized * shotSpeed;
            shotsFired++;
        }
    }

    public IEnumerator MovePlayerToTarget(Vector3 targetPos, float duracion)
    {
        Vector3 origen = transform.position;
        float tiempo = 0f;
        animator.SetFloat("Speed", 1f);

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / duracion;

            transform.position = Vector3.Lerp(origen, targetPos, t);
            yield return null;
        }
        StopPlayer();
        transform.position = targetPos;

        // Restaurar control del jugador

        // Por ejemplo, desactivar bloqueo o disparar evento
        // GameManager.Instance.isPlayerMovementBlocked = false;
    }

    public void StopPlayer()
    {
        animator.SetFloat("Speed", 0f);
        rb.linearVelocity = new Vector2(0, 0);
    }


    public void TakeDamage(int damage, Vector2 hitDirection, float knockbackForceReceived)
    {
        if (isInvulnerable) return;
        animator.SetTrigger("Hurt");
        StartCoroutine(Stun(0.3f)); // Pausa el movimiento por 0.2 segundos
        if (knockbackForceReceived > 0f)
            ApplyKnockback(hitDirection, knockbackForceReceived);
        
        currentHealth -= damage;

        IncrementarStat(StatType.DamageReceived, damage);

        StartCoroutine(HitFeedback(Color.red));

        HUDManager.Instance.UpdateHealthUI();

        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }
    private bool isStunned = false;
    private IEnumerator Stun(float time)
    {
        isStunned = true;
        StopPlayer();
        yield return new WaitForSeconds(time);
        rb.linearVelocity = Vector2.zero; // Detiene el movimiento residual del knockback
        isStunned = false;
    }

    public void ApplyKnockback(Vector2 direction, float knockbackForce)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);
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
            TakeDamage((int)damagePerTick, Vector2.zero, 0f);
            yield return new WaitForSeconds(tickRate);
            elapsed += tickRate;
        }
        isPassiveActive = false;
    }

    IEnumerator Die()
    {
        Debug.Log("Jugador muerto");
        StopPlayer();
        GameManager.Instance.cutscene = true;
        // Deja que el personaje siga animando
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        // Pausa el tiempo global
        Time.timeScale = 0f;
        // Iniciamos Barras de Cutscene
        StartCoroutine(CutsceneManager.Instance.ShowCinematicBars());
        // Acercamiento al player
        yield return StartCoroutine(ZoomToPlayer(1.5f));
        // Reproduce animación de muerte
        animator.SetTrigger("Death");
        // Espera tiempo real (ya que timeScale = 0)
        yield return new WaitForSecondsRealtime(2f);
        // Ahora sí quieres que el personaje también se "congele"
        animator.updateMode = AnimatorUpdateMode.Normal;
        IncrementarStat(StatType.Deaths, 1);
        lifes--;
        if (lifes <= 0)
        {
            yield return StartCoroutine(CutsceneManager.Instance.DeathCutSceneSequence("Game Over", "Press Enter To Main Menu"));
            // Volver al MainMenu
            GameManager.Instance.BackToMainMenu();

        }
        else
        {
            // Cutscene de muerte
            yield return StartCoroutine(CutsceneManager.Instance.DeathCutSceneSequence("You Died", "Press Enter To Continue"));
            ResetPlayer();
            animator.SetTrigger("Live");
            Time.timeScale = 1f;

            // Recargar escena
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }



    public IEnumerator ZoomToPlayer(float duration)
    {
        CameraFollowVertical.Instance.enableCameraFollow = false;

        float startSize = Camera.main.orthographicSize;
        float targetSize = 2f;

        Vector3 startPos = Camera.main.transform.position;
        Vector3 targetPos = new Vector3(startPos.x, transform.position.y, startPos.z);

        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;

            // Interpolar tamaño
            Camera.main.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

            // Interpolar posición
            Camera.main.transform.position = Vector3.Lerp(startPos, targetPos, t);

            time += Time.unscaledDeltaTime; // Para que funcione incluso si timeScale = 0
            yield return null;
        }

        // Asegura que queda en destino exacto
        Camera.main.orthographicSize = targetSize;
        Camera.main.transform.position = targetPos;
    }



    IEnumerator HitFeedback(Color color)
    {
        isInvulnerable = true;

        if (spriteRenderer != null)
            spriteRenderer.color = color;
        yield return new WaitForSeconds(0.5f);
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(invulnerabilityTime - 0.5f);
        isInvulnerable = false;
    }

    IEnumerator PerformDash()
    {
        isDashing = true;
        currentDashCount--;
        HUDManager.Instance.UpdateDashUI();
        IncrementarStat(StatType.DashesDone, 1f);

        // Encola un dash pendiente
        dashRechargeQueue.Enqueue(dashRechargeTime);

        // Si no hay recarga activa, arrancar la cadena
        if (!isRecharging)
            StartCoroutine(HandleDashRechargeQueue());

        Vector2 dashDirection = moveDirection;
        if (dashDirection == Vector2.zero) dashDirection = Vector2.right; // Default


        // Efecto visual del dash
        GameObject efecto = Instantiate(dashEffect, dashEffect.transform.position, dashEffect.transform.rotation);
        efecto.transform.SetParent(null, true);
        Destroy(efecto, 0.5f);

        Vector2 startPosition = rb.position;

        // Lanzar raycast para ver hasta dónde puedo llegar sin chocar
        float maxDistance = dashDistance;
        RaycastHit2D hit = Physics2D.Raycast(startPosition, dashDirection, dashDistance, LayerMask.GetMask("Obstacle"));

        if (hit.collider != null)
        {
            // Reducimos la distancia para parar justo antes del collider (por ejemplo, 0.05f de margen)
            maxDistance = hit.distance - 0.05f;
            if (maxDistance < 0) maxDistance = 0;
        }

        // Ahora hacemos el dash moviendo el Rigidbody2D a la posición objetivo suavemente
        Vector2 targetPosition = startPosition + dashDirection * maxDistance;
        float elapsed = 0f;
        float duration = maxDistance / dashSpeed; // tiempo que tardará el dash según velocidad

        while (elapsed < duration && !GameManager.Instance.cutscene)
        {
            elapsed += Time.deltaTime;
            // Interpolamos la posición
            Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, elapsed / duration);
            rb.MovePosition(newPosition);
            yield return null;
        }

        // Aseguramos la posición final exacta
        rb.MovePosition(targetPosition);

        isDashing = false;
    }

    IEnumerator HandleDashRechargeQueue()
    {
        isRecharging = true;

        while (dashRechargeQueue.Count > 0)
        {
            float waitTime = dashRechargeQueue.Dequeue();
            // Lanzamos la animación de la barra con ese tiempo
            StartCoroutine(HUDManager.Instance.AnimateDashCooldownBar(waitTime));
            yield return new WaitForSeconds(waitTime);

            if (currentDashCount < maxDashCount)
            {
                currentDashCount++;
                HUDManager.Instance.UpdateDashUI();
            }
        }

        isRecharging = false;
    }

    public void AddExperience(int amount)
    {
        currentXP += amount;

        // Bucle por si el jugador gana mucha XP de golpe
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;   // o currentXP = 0 si prefieres reiniciar
            level++;

            PowerUpType[] options = new PowerUpType[3];
            // Rellená con 3 tipos aleatorios
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = (PowerUpType)Random.Range(0, System.Enum.GetValues(typeof(PowerUpType)).Length);
            }
            HUDManager.Instance.ShowPowerUpMenu(options, this, true);

            // Recalcula la XP del siguiente nivel
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * growthFactor);
            Debug.Log($"¡Subiste a nivel {level}! Próximo nivel en {xpToNextLevel} XP");
        }
        float exp = Mathf.Clamp01((float)currentXP / xpToNextLevel);
        HUDManager.Instance.AnimateExpBar(exp);

    }

    public void ApplyItem(ItemType type, float value)
    {
        switch (type)
        {
            case ItemType.HEAL:
                ApplyPowerUp(PowerUpType.HealOne);
                IncrementarStat(StatType.HealReceived, 1f);
                break;
            case ItemType.LIFE:
                ApplyPowerUp(PowerUpType.Life);
                IncrementarStat(StatType.Lifes, 1f);
                break;
            case ItemType.DASH:
                currentDashCount++;
                HUDManager.Instance.UpdateDashUI();
                break;
            case ItemType.EXP:
                AddExperience((int)value);
                break;
            default:
                break;
        }
    }

    public void ApplyPowerUp(PowerUpType type)
    {
        float value = PowerUpInfo.Data[type].value;


        switch (type)
        {// asd

            // Health
            case PowerUpType.Life:
                lifes += (int)value;
                break;
            case PowerUpType.HealOne:
                if (currentHealth < maxHealth)
                {
                    currentHealth += (int)value;
                    HUDManager.Instance.UpdateHealthUI();
                    IncrementarStat(StatType.HealReceived, 1f);
                }
                break;
            case PowerUpType.HealFull:
                IncrementarStat(StatType.HealReceived, maxHealth - currentHealth);
                currentHealth = maxHealth;
                HUDManager.Instance.UpdateHealthUI();
                break;
            case PowerUpType.MaxHealth:
                if (maxHealth < maxHealthLimit)
                {
                    maxHealth += (int)value;
                    currentHealth += (int)value;
                    HUDManager.Instance.UpdateHealthUI();
                }
                break;
            case PowerUpType.InvulnerableTime:
                if (invulnerabilityTime + value > 0f)
                invulnerabilityTime += value;
                break;
            case PowerUpType.KnockbackResistance:
                knockbackResistance += value;
                break;
            // Movement
            case PowerUpType.MoveSpeed:
                moveSpeed += value;
                break;
            case PowerUpType.MaxDashCount:
                if (maxDashCount < maxDashLimit)
                {
                    maxDashCount += (int)value;
                    dashRechargeQueue.Enqueue(dashRechargeTime);
                    // Si no hay recarga activa, arrancar la cadena
                    if (!isRecharging)
                        StartCoroutine(HandleDashRechargeQueue());
                    HUDManager.Instance.UpdateDashUI();
                }
                break;
            case PowerUpType.DashSpeed:
                dashSpeed += value;
                break;
            case PowerUpType.DashDistance:
                if (dashDistance + value < 5f)
                dashDistance += value;
                break;
            case PowerUpType.DashRechargeTime:
                if (dashRechargeTime + value > 0f)
                dashRechargeTime += value;
                break;

            // Damage
            case PowerUpType.Damage:
                damage += value;
                break;

            // Shot
            case PowerUpType.ShotRange:
                shotRange += value;
                break;
            case PowerUpType.ShotSpeed:
                shotSpeed += value;
                break;
            case PowerUpType.ShotSize:
                if (shotSize + value < 6f)
                shotSize += value;
                break;
            case PowerUpType.ShotRate:
                if (shotRate + value > 0f)
                shotRate += value;
                break;
            case PowerUpType.ShotKnockback:
                shotKnockback += value;
                break;
            default:
                break;
        }

        Debug.Log($"PowerUp aplicado: {type}, {value}");
    }


    public void IncrementarStat(StatType type, float value)
    {


        switch (type)
        {
            // Health
            case StatType.InvulnerabilityTime:
                invulnerabilityTime += value;
                break;
            case StatType.KnockbackResistance:
                knockbackResistance += value;
                break;

            // Movement
            case StatType.MoveSpeed:
                moveSpeed += value;
                break;
            case StatType.DashDistance:
                if (dashDistance + value < 5f)
                dashDistance += value;
                break;

            // Damage
            case StatType.Damage:
                damage += value;
                break;

            // Shot
            case StatType.ShotRange:
                shotRange += value;
                break;
            case StatType.ShotSpeed:
                shotSpeed += value;
                break;
            case StatType.ShotSize:
                if (shotSize + value < 6f)
                shotSize += value;
                break;
            case StatType.ShotRate:
                if (shotRate + value > 0f)
                shotRate += value;
                break;
            case StatType.ShotKnockback:
                shotKnockback += value;
                break;
            // Kills Stats
            case StatType.BossKills:
                bossKills += (int)value;
                break;
            case StatType.Kills:
                kills += (int)value;
                break;
            case StatType.DamageDone:
                damageDone += value;
                break;
            case StatType.ShotsFired:
                shotsFired += (int)value;
                break;
            case StatType.ShotsHit:
                shotsHit += (int)value;
                break;
            case StatType.DashesDone:
                dashesDone += (int)value;
                break;
            case StatType.DamageReceived:
                damageReceived += value;
                break;
            case StatType.HealReceived:
                healReceived += value;
                break;
            case StatType.PowerUps:
                powerUpsTaken += (int)value;
                break;
            case StatType.Deaths:
                deaths += (int)value;
                break;
            case StatType.Playtime:
                playtime += value;
                break;
        }
    }

    public void SetInvulnerable(bool state)
    {
        isInvulnerable = state;
    }

    
}
