using UnityEngine;
using System.Collections.Generic;

public class LockOnSystem : MonoBehaviour
{
    [Header("LockOn Settings")]
    public float lockRange = 600f;
    public float lockAngle = 25f;
    public float lockTimeRequired = 3f;

    private List<Transform> targetsInSight = new List<Transform>();
    private int targetIndex = 0;
    private Transform currentTarget;
    private float lockTimer = 0f;
    private bool isLocked = false;

    // this is the property to access the current target from outside
    public Transform CurrentTarget => currentTarget;
    public bool IsLocked => isLocked;
    public float LockProgress => lockTimer / lockTimeRequired;

    void Update()
    {
        if (GameManager.Instance != null &&
           (GameManager.Instance.IsGameOverOrClear() || GameManager.Instance.IsPaused))
            return;

        targetsInSight.Clear();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        // This loop checks all enemies in the scene to see if they are within the lock-on range and angle. If they are, they are added to the targetsInSight list.
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            Vector3 toEnemy = enemy.transform.position - transform.position;
            if (toEnemy.magnitude < lockRange)
            {
                float angle = Vector3.Angle(transform.forward, toEnemy);
                if (angle < lockAngle)
                {
                    targetsInSight.Add(enemy.transform);
                }
            }
        }

        if (targetsInSight.Count == 0)
        {
            ResetLock();
            return;
        }

        if (targetIndex >= targetsInSight.Count)
        {
            targetIndex = 0;
        }

        Transform activeTarget = targetsInSight[targetIndex];

        if (Input.GetKeyDown(KeyCode.Q) && targetsInSight.Count > 1)
        {
            targetIndex = (targetIndex + 1) % targetsInSight.Count;
            activeTarget = targetsInSight[targetIndex];
            lockTimer = 0f;
            isLocked = false;
        }

        if (currentTarget != activeTarget)
        {
            currentTarget = activeTarget;
            lockTimer = 0f;
            isLocked = false;
        }

        if (!isLocked)
        {
            lockTimer += Time.deltaTime;
            if (lockTimer >= lockTimeRequired)
            {
                isLocked = true;
            }
        }
    }

    // This function resets the lock-on state, clearing the current target and resetting the timer and lock status. It is called when there are no valid targets in sight or when the player switches targets.
    public void ResetLock()
    {
        currentTarget = null;
        lockTimer = 0f;
        isLocked = false;
    }
}