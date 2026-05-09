using UnityEngine;
using TMPro;

[RequireComponent(typeof(LockOnSystem))]
[RequireComponent(typeof(MissileLauncher))]
[RequireComponent(typeof(PlayerController))]
public class TacticalHUD : MonoBehaviour
{
    [Header("HUD UI Settings (Optional)")]
    public TextMeshProUGUI ammoTMPText;

    private LockOnSystem lockOn;
    private MissileLauncher launcher;
    private PlayerController player;

    void Start()
    {
        lockOn = GetComponent<LockOnSystem>();
        launcher = GetComponent<MissileLauncher>();
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        UpdateTMP();
    }

    void UpdateTMP()
    {
        if (ammoTMPText == null) return;

        // make sure to hide the ammo text when the game is over, cleared, or pause
        if (GameManager.Instance != null &&
           (GameManager.Instance.IsGameOverOrClear() || GameManager.Instance.IsPaused))
        {
            ammoTMPText.text = "";
            return;
        }

        if (launcher.IsReloading)
        {
            float timeLeft = launcher.ReloadTimeLeft;
            if (Mathf.Repeat(Time.time * 3f, 1f) < 0.5f)
            {
                ammoTMPText.text = $"<color=red>RELOADING... {timeLeft:F1}s</color>";
            }
            else
            {
                ammoTMPText.text = $"<color=orange>RELOADING... {timeLeft:F1}s</color>";
            }
        }
        else
        {
            int currentAmmo = launcher.CurrentAmmo;
            string bars = new string('▮', currentAmmo);
            string emptyBars = new string('▯', 4 - currentAmmo);
            ammoTMPText.text = $"<color=green>MSL: {bars}{emptyBars} ({currentAmmo}/4)</color>";
        }
    }

    void OnGUI()
    {

        //This entire HUD will be hidden when the game is over, cleared, or paused
        if (GameManager.Instance != null &&
           (GameManager.Instance.IsGameOverOrClear() || GameManager.Instance.IsPaused))
        {
            return;
        }

        //This section is for the speed display at the top right corner, it will show the current speed in km/h
        if (player != null)
        {
            float speedKmH = (player.currentSpeed * 3600f) / 1000f;

            GUIStyle speedStyle = new GUIStyle();
            speedStyle.fontSize = 18;
            speedStyle.fontStyle = FontStyle.Bold;
            speedStyle.alignment = TextAnchor.MiddleRight;
            speedStyle.normal.textColor = Color.green;

            string speedStr = $"SPD: {speedKmH:0} km/h";
            GUI.Label(new Rect(Screen.width - 220, 20, 200, 30), speedStr, speedStyle);
        }

        // this is for the ammo display at the bottom right corner, it will show the current missile ammo and reloading status
        if (ammoTMPText == null)
        {
            GUIStyle ammoStyle = new GUIStyle();
            ammoStyle.fontSize = 18;
            ammoStyle.fontStyle = FontStyle.Bold;
            ammoStyle.alignment = TextAnchor.MiddleRight;

            string ammoStr;
            if (launcher.IsReloading)
            {
                ammoStyle.normal.textColor = Color.red;
                ammoStr = $"RELOADING... {launcher.ReloadTimeLeft:F1}s";
            }
            else
            {
                ammoStyle.normal.textColor = Color.green;
                ammoStr = $"MSL: {new string('I', launcher.CurrentAmmo)}";
            }

            GUI.Label(new Rect(Screen.width - 220, Screen.height - 45, 200, 35), ammoStr, ammoStyle);
        }
        // this is for the target count and closest target range display at the bottom left corner
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        int enemyCount = allEnemies.Length;
        float closestDistance = -1f;

        if (enemyCount > 0)
        {
            closestDistance = float.MaxValue;
            foreach (GameObject enemy in allEnemies)
            {
                if (enemy == null) continue;
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                }
            }
        }

        GUIStyle leftHUDStyle = new GUIStyle();
        leftHUDStyle.fontSize = 18;
        leftHUDStyle.fontStyle = FontStyle.Bold;
        leftHUDStyle.alignment = TextAnchor.MiddleLeft;
        leftHUDStyle.normal.textColor = Color.green;

        string tgtText = $"Targets: {enemyCount:D2}";
        string rngText = closestDistance > 0 ? $"Closest Enemy: {closestDistance:0}m" : "Closest Enemy: ----m";

        GUI.Label(new Rect(20, Screen.height - 75, 200, 30), tgtText, leftHUDStyle);
        GUI.Label(new Rect(20, Screen.height - 45, 200, 30), rngText, leftHUDStyle);

        // If there is no target information, exit
        if (lockOn.CurrentTarget == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(lockOn.CurrentTarget.position);
        bool isOffScreen = screenPos.z < 0 || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height;

        if (!isOffScreen)
        {
            // This section is for the lock-on status display above the target when it's on screen, it will show "LOCKED" in red when locked, or "LOCKING XX%" in green when still locking
            float flippedY = Screen.height - screenPos.y;

            GUIStyle lockStyle = new GUIStyle();
            lockStyle.fontSize = 14;
            lockStyle.fontStyle = FontStyle.Bold;
            lockStyle.alignment = TextAnchor.MiddleCenter;
            lockStyle.normal.textColor = lockOn.IsLocked ? Color.red : Color.green;

            string lockText = lockOn.IsLocked ? "[ LOCKED ]" : $"[ LOCKING {lockOn.LockProgress * 100:0}% ]";
            GUI.Label(new Rect(screenPos.x - 100, flippedY - 20, 200, 40), lockText, lockStyle);
        }
    }
}