using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // This is the variable that will hold the reference to the player's transform, set this in the Inspector
    public Vector3 offset = new Vector3(0, 10, -30); // Default camera offset
    public Vector3 rotation = new Vector3(5, 0, 0); // Camera rotation (fixed value to always face forward)
    public float smoothSpeed = 0.125f;

    private Vector3 activeOffset;

    void Start()
    {
        activeOffset = offset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Determine the target offset (distance) based on the input status of the R/F keys
        Vector3 targetOffset = offset;
        if (Input.GetKey(KeyCode.R)) targetOffset = new Vector3(0, 10, -35);
        else if (Input.GetKey(KeyCode.F)) targetOffset = new Vector3(0, 10, -25);

        // 2. Smoothly transition the offset to completely prevent jitter
        activeOffset = Vector3.Lerp(activeOffset, targetOffset, Time.deltaTime * 5f);

        // 3. Calculate the target position of the camera
        Vector3 desiredPosition = target.TransformPoint(activeOffset);

        // 4. Smoothly follow the position only
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 5. Sync the camera rotation to the target's rotation with a fixed angle to always look forward, without smoothing to prevent lag
        transform.rotation = target.rotation;
    }
}