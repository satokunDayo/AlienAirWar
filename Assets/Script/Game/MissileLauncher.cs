using UnityEngine;

[RequireComponent(typeof(LockOnSystem))]
[RequireComponent(typeof(PlayerController))]
public class MissileLauncher : MonoBehaviour
{
    [Header("4 Missile Prefabs (Projectのプレハブを登録)")]
    public GameObject[] missilePrefabs = new GameObject[4];

    [Header("4 Wing Missile Models (Hierarchyの翼モデルを登録)")]
    public GameObject[] wingMissileModels = new GameObject[4];

    [Header("4 Spawn Points (Hierarchyの翼モデルを登録)")]
    public Transform[] spawnPoints = new Transform[4];

    [Header("Reload Settings")]
    public float reloadTime = 3.5f;

    private LockOnSystem lockOnSystem;
    private PlayerController playerController;
    private int missilesFired = 0;
    private bool isReloading = false;
    private float reloadTimer = 0f;

    public int CurrentAmmo => 4 - missilesFired;
    public bool IsReloading => isReloading;
    public float ReloadTimeLeft => reloadTime - reloadTimer;

    void Start()
    {
        lockOnSystem = GetComponent<LockOnSystem>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (GameManager.Instance != null &&
           (GameManager.Instance.IsGameOverOrClear() || GameManager.Instance.IsPaused))
            return;

        if (isReloading)
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer >= reloadTime)
            {
                isReloading = false;
                missilesFired = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (wingMissileModels[i] != null) wingMissileModels[i].SetActive(true);
                }
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && lockOnSystem.IsLocked && lockOnSystem.CurrentTarget != null && missilesFired < 4)
        {
            FireMissile();
        }
    }

    void FireMissile()
    {
        int idx = missilesFired;

        if (wingMissileModels[idx] != null) wingMissileModels[idx].SetActive(false);

        if (missilePrefabs[idx] != null && spawnPoints[idx] != null)
        {
            // spawn missile and set target
            GameObject spawnedMissile = Instantiate(missilePrefabs[idx], spawnPoints[idx].position, spawnPoints[idx].rotation);
            Missile missileScript = spawnedMissile.GetComponent<Missile>();

            if (missileScript != null)
            {
                // give the missile the target to missileScript.SetTarget(lockOnSystem.CurrentTarget);
                missileScript.SetTarget(lockOnSystem.CurrentTarget);
            }
        }

        missilesFired++;
        lockOnSystem.ResetLock();

        if (missilesFired >= 4)
        {
            isReloading = true;
            reloadTimer = 0f;
        }
    }
}