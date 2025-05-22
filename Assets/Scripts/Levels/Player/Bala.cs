using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class Bala : MonoBehaviour
{

    private Animator animator;

    public float damage = 1;
    public float shotRange = 5f; // Distancia máxima que la bala puede recorrer
    public float knockbackForce = 1f;
    private Vector2 startPosition;
    public bool fromEnemy;

    // Pasiva DoT
    public BulletPassiveType passive;

    void Start()
    {
        animator = GetComponent<Animator>();
        startPosition = transform.position; // Guardamos la posición inicial de la bala
    }


    void Update()
    {
        if (GameManager.Instance.cutscene) StartCoroutine(DestroyBullet()); // Si esta en cutscene el personaje no hace nada
        // Calculamos la distancia recorrida desde el inicio
        float distanceTravelled = Vector2.Distance(startPosition, transform.position);

        // Si la distancia recorrida supera el máximo permitido, destruimos la bala
        if (distanceTravelled >= shotRange)
        {
            StartCoroutine(DestroyBullet());
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        bool collide = false;
        if (fromEnemy)
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                Vector2 hitDirection = collision.transform.position - transform.position;
                player.TakeDamage((int)damage, hitDirection, knockbackForce);

                if (passive != BulletPassiveType.None)
                    player.ApplyPassive(passive);
                collide = true;
                
            }
        }
        else
        {

            EnemyIA enemy = collision.GetComponent<EnemyIA>();
            if (enemy != null)
            {
                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                enemy.TakeDamage(damage, Color.red, knockbackDirection, knockbackForce);

                if (passive != BulletPassiveType.None)
                    enemy.ApplyPassive(passive);
                collide = true;
            }

            BossIA booss = collision.GetComponent<BossIA>();
            if (booss != null && !booss.isDead)
            {
                booss.TakeDamage(damage, Color.red);

                if (passive != BulletPassiveType.None)
                    booss.ApplyPassive(passive);
                collide = true;
            }
        }
        if(collide) StartCoroutine(DestroyBullet());
        
        
    }

    IEnumerator DestroyBullet(){
        animator.SetTrigger("Explode");
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject); // Destruye la bala tras impactar
    }
}
