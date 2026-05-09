using UnityEngine;

public class EnemyUfo : MonoBehaviour
{
    [Header("Effects")]
    public GameObject explosionPrefab; // Invoke this prefab when the UFO is destroyed, if available

    // when the UFO is hit by a missile, this method will be called to handle the explosion effect, report to GameManager, and destroy the UFO object itself
    public void Explode()
    {
        // 1. Generate the explosion effect 
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, transform.rotation);
        }

        // 2. Report to GameManager that one enemy has been destroyed
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDestroyed();
        }

        // 3. Destroy the UFO object itself
        Destroy(gameObject);
    }
}