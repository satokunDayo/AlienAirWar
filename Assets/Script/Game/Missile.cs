using UnityEngine;

public class Missile : MonoBehaviour
{
    public float speed = 180f;
    public float turnSpeed = 8f;
    public float lifeTime = 6f;
    public GameObject explosionPrefab;

    [Header("Missile Physics")]
    public float acceleration = 80f;

    [Header("Delay Guide Settings")]
    //after few meter away, the missile will start to guide to target, this is the delay time before guiding
    public float delayGuideTime = 0.4f;

    private Transform target;
    private float elapsed = 0f;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime); //Destroy after lifeTime seconds to prevent infinite existence

        //This is the critical part for the missile's initial speed and direction:
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            PlayerController playerController = GameManager.Instance.player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                //assume the missile is launched from the player's current position,with an initial speed boost based on the player's current speed
                speed = playerController.currentSpeed + 20f;
            }

            // Set the missile's initial position and rotation to match the player's current position and rotation
            transform.rotation = GameManager.Instance.player.transform.rotation;
        }
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        //accelerate the missile over time
        speed += acceleration * Time.deltaTime;

        //use world space forward to ensure the missile moves in the direction it's facing, regardless of its local rotation
        transform.position += transform.forward * speed * Time.deltaTime;

        // start guiding to target after delayGuideTime seconds have passe
        if (target != null && elapsed >= delayGuideTime)
        {
            Vector3 targetDir = (target.position - transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(targetDir);

            // make the missile turn towards the target smoothly, with a turn speed that ramps up over time after the delay
            float currentTurnSpeed = Mathf.Lerp(0f, turnSpeed, (elapsed - delayGuideTime) * 2f);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, currentTurnSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyUfo ufo = other.GetComponent<EnemyUfo>();
            if (ufo != null)
            {
                ufo.Explode();
            }
            Impact();
        }
        else if (other.CompareTag("Ground"))
        {
            Impact();
        }
    }

    void Impact()
    {
        // Instantiate explosion effect at the missile's position and rotation, then destroy the missile
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }
}