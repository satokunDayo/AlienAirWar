using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float currentSpeed = 100f;
    public float accelPower = 30f;
    public float minSpeed = 100f;
    public float maxSpeed = 500f;

    [Header("Flight Dynamics")]
    public float turnSpeed = 30f; 
    public float rollSpeed = 60f;    
    public float maxRollAngle = 50f; 
    public float rollReturnSpeed = 3f; 

    [Header("Hinges (親オブジェクト)")]
    public Transform leftElevatorHinge;
    public Transform rightElevatorHinge;
    public Transform leftRudderHinge;
    public Transform rightRudderHinge;
    public Transform leftFlapHinge;
    public Transform rightFlapHinge;

    private float curEv, curRd, curFl;
    private float currentRoll = 0f;
    private bool isCrashed = false;

    void Update()
    {
        if (isCrashed) return;

        // obtain horizontal and vertical input based on key presses (D/A for horizontal, S/W for vertical)
        float h = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        float v = (Input.GetKey(KeyCode.S) ? 1f : 0f) - (Input.GetKey(KeyCode.W) ? 1f : 0f);

        // handle acceleration and forward movement
        HandleEngine();

        //update the whole plane's rotation based on input
        UpdateWholePlaneAngle(h, v);

        // update hinge rotations for each part
        UpdateHingeRotations(h, v);
    }

    void HandleEngine()
    {
        if (Input.GetKey(KeyCode.R)) currentSpeed += accelPower * Time.deltaTime;
        if (Input.GetKey(KeyCode.F)) currentSpeed -= accelPower * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);

        // move the parent object (the one this script is attached to) forward
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }

    // function to directly control the roll, pitch, and yaw of the parent object
    void UpdateWholePlaneAngle(float h, float v)
    {
        // 1. pitch based on x-axis input (W/S keys): rotate around the local right axis to control the nose up/down
        transform.Rotate(Vector3.right * v * turnSpeed * Time.deltaTime, Space.Self);

        // 2. yaw based on y-axis input (A/D keys): rotate around the world's up axis to turn horizontally
        transform.Rotate(Vector3.up * h * turnSpeed * Time.deltaTime, Space.World);

        // 3. roll based on z-axis input (A/D keys): tilt the plane when input is present, smoothly return to 0 when no input
        float targetRoll = -h * maxRollAngle;

        // calculate the current roll angle (technique to handle angles in the range of -180 to 180)
        float anglesZ = transform.localEulerAngles.z;
        if (anglesZ > 180) anglesZ -= 360;

        // smoothly approach the target roll angle using Lerp
        float nextRoll = Mathf.Lerp(anglesZ, targetRoll, Time.deltaTime * rollReturnSpeed);

        // Maintain the current X and Y rotation while only applying the Z-axis as the "tilt"
        Vector3 currentRot = transform.localEulerAngles;
        transform.localRotation = Quaternion.Euler(currentRot.x, currentRot.y, nextRoll);
    }

    void UpdateHingeRotations(float h, float v)
    {
        // Lerp control for hinges (partSmoothSpeed is assumed to be around 15f)
        curEv = Mathf.Lerp(curEv, v * 20f, Time.deltaTime * 15f);
        curRd = Mathf.Lerp(curRd, h * 15f, Time.deltaTime * 15f);
        curFl = Mathf.Lerp(curFl, h * 25f, Time.deltaTime * 15f);

        if (leftElevatorHinge) leftElevatorHinge.localRotation = Quaternion.Euler(curEv, 0, 0);
        if (rightElevatorHinge) rightElevatorHinge.localRotation = Quaternion.Euler(curEv, 0, 0);
        if (leftRudderHinge) leftRudderHinge.localRotation = Quaternion.Euler(0, curRd, 0);
        if (rightRudderHinge) rightRudderHinge.localRotation = Quaternion.Euler(0, curRd, 0);
        if (leftFlapHinge) leftFlapHinge.localRotation = Quaternion.Euler(curFl, 0, 0);
        if (rightFlapHinge) rightFlapHinge.localRotation = Quaternion.Euler(-curFl, 0, 0);
    }

void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground") && !isCrashed)
        {
            isCrashed = true;
            GameManager.Instance?.OnPlayerCrashed();
        }
    }
}