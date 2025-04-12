using UnityEngine;
using System.Collections; // Needed for Coroutines

public class BombController : MonoBehaviour
{
    private float fuseTime;
    private GameObject explosionEffectPrefab;
    private bool isInitialized = false;
    private bool hasExploded = false;
    private float explosionDamage = 50f;
    
    // Explosive settings
    [Header("Explosion Settings")]
    public float explosionRadius = 3f;  // 爆炸半径
    public LayerMask damageLayerMask;   // 伤害图层掩码

    // Bomb's Collider2D to detect explosion range
    private CircleCollider2D bombCollider;

    /// <summary>
    /// Sets up the bomb's parameters after instantiation.
    /// </summary>
    /// <param name="fuse">Time in seconds until explosion.</param>
    /// <param name="explosionPrefab">The visual effect to play on explosion.</param>
    public void Initialize(float fuse, GameObject explosionPrefab, 
    float damage = 50f, float explosionRadius = 3f, LayerMask damageLayerMask = default)
    {
        explosionDamage = damage;
        this.explosionRadius = explosionRadius;
        this.damageLayerMask = damageLayerMask;
        fuseTime = fuse;
        explosionEffectPrefab = explosionPrefab;
        isInitialized = true;

        bombCollider = GetComponent<CircleCollider2D>(); // Getting the CircleCollider2D component

        if (bombCollider != null)
        {
            bombCollider.radius = explosionRadius; // Set the radius of the collider to the explosion radius
            bombCollider.isTrigger = true; // Make sure it's a trigger to detect objects
        }

        StartCoroutine(ExplosionTimer());
    }

    private IEnumerator ExplosionTimer()
    {
        // Wait for the fuse time
        yield return new WaitForSeconds(fuseTime);

        // Explode (if not already exploded somehow)
        Explode();
    }

    /// <summary>
    /// Triggers the explosion effect and destroys the bomb.
    /// </summary>
    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // ✅ 1. Create explosion effect
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // ✅ 2. Execute damage logic via collider's trigger zone
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageLayerMask);
        foreach (Collider2D hit in hitObjects)
        {
            Debug.Log($"Collided with {hit.name}");
            var health = hit.GetComponent<PlayerController>(); // Replace with your damageable component
            if (health != null)
            {
                health.TakeDamage(explosionDamage);
                Debug.Log($"{hit.name} took {explosionDamage} damage from bomb explosion.");
            }
        }

        // ✅ 3. Destroy the bomb itself
        Destroy(gameObject);
    }

    // Optional: To trigger the explosion when the bomb collides with an object
    // If you still want to use OnCollisionEnter or OnTriggerEnter for certain objects before explosion
    /*
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isInitialized && !hasExploded) // Only explode if initialized and hasn't exploded yet
        {
            Debug.Log("Bomb collided with " + collision.gameObject.name + ", exploding!");
            StopCoroutine(ExplosionTimer()); // Stop the timer coroutine
            Explode();
        }
    }
    */
}
