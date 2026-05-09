using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // for the Button component
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UFO Settings")]
    public GameObject ufoPrefab;
    public Transform player;
    public float spawnRadius = 800f;

    [Header("Canvas Panels")]
    public GameObject gameOverPanel;  // GameOver
    public GameObject gameClearPanel; // GameClear
    public GameObject escPanel;       // Pause screen (ESC)

    [Header("TextMeshPro Texts")]
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameClearScoreText;
    public TextMeshProUGUI gameClearTimeText;

    [Header("Buttons")]
    public Button gameOverTitleButton;
    public Button gameOverContinueButton;
    public Button gameClearTitleButton;
    public Button gameClearContinueButton;

    [Header("ESC Panel Buttons)")]
    // Pause screen buttons
    public Button escTitleButton;
    public Button escContinueButton;

    private float playTime = 0f;
    private int initialEnemiesCount = 0;
    private int remainingEnemies = 0;
    private bool isGameOver = false;
    private bool isGameClear = false;
    private bool isPaused = false; // Pause flag

    // Property to monitor the pause state from outside
    public bool IsPaused => isPaused;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);
        if (escPanel != null) escPanel.SetActive(false); // Initially hidden

        // Automatically wire up button events
        if (gameOverTitleButton != null) gameOverTitleButton.onClick.AddListener(LoadTitle);
        if (gameOverContinueButton != null) gameOverContinueButton.onClick.AddListener(RestartGame);
        if (gameClearTitleButton != null) gameClearTitleButton.onClick.AddListener(LoadTitle);
        if (gameClearContinueButton != null) gameClearContinueButton.onClick.AddListener(RestartGame);

        // Pause screen button events
        if (escTitleButton != null) escTitleButton.onClick.AddListener(LoadTitle);
        if (escContinueButton != null) escContinueButton.onClick.AddListener(TogglePause); // Continue button to unpause

        int enemySpawnCount = GlobalSetting.EnemyCount > 0 ? GlobalSetting.EnemyCount : 10;
        initialEnemiesCount = enemySpawnCount;
        remainingEnemies = enemySpawnCount;

        for (int i = 0; i < enemySpawnCount; i++)
        {
            SpawnUFO();
        }
    }

    void Update()
    {
        // Only increment time and accept Esc input during gameplay
        if (!isGameOver && !isGameClear)
        {
            if (!isPaused)
            {
                playTime += Time.deltaTime;
            }

            // Toggle pause when the Esc key is pressed
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
    }

    void SpawnUFO()
    {
        // Ensure we have the necessary references before spawning
        if (ufoPrefab == null || player == null) return;

        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
        float randomHeight = Random.Range(100f, 300f);
        Vector3 spawnPos = new Vector3(player.position.x + randomCircle.x, randomHeight, player.position.z + randomCircle.y);

        GameObject ufo = Instantiate(ufoPrefab, spawnPos, Quaternion.identity);
        ufo.tag = "Enemy";
    }

    public void OnEnemyDestroyed()
    {
        remainingEnemies--;
        if (remainingEnemies <= 0 && !isGameOver)
        {
            TriggerGameClear();
        }
    }

    public void OnPlayerCrashed()
    {
        // The method will be called when the player crashes, but if the game is already over or cleared, we don't want to trigger game over again
        if (isGameOver || isGameClear) return;

        isGameOver = true;
        Time.timeScale = 0f;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (escPanel != null) escPanel.SetActive(false); 

        int killed = initialEnemiesCount - remainingEnemies;
        int score = killed * 1000;

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = $"SCORE: {score}\n(UFO KILLED: {killed})";
        }
    }

    void TriggerGameClear()
    {
        isGameClear = true;
        Time.timeScale = 0f;

        if (gameClearPanel != null) gameClearPanel.SetActive(true);
        if (escPanel != null) escPanel.SetActive(false); // Force close the pause screen when clearing

        int killScore = initialEnemiesCount * 1000;
        int timeBonus = Mathf.Max(0, 10000 - Mathf.RoundToInt(playTime * 50));
        int totalScore = killScore + timeBonus;

        if (gameClearScoreText != null)
        {
            gameClearScoreText.text = $"FINAL SCORE: {totalScore}";
        }

        if (gameClearTimeText != null)
        {
            gameClearTimeText.text = $"CLEAR TIME: {playTime:F2}s";
        }
    }

    // Toggle pause function
    public void TogglePause()
    {
        if (isGameOver || isGameClear) return;

        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // Stop time
            if (escPanel != null) escPanel.SetActive(true); // Show pause panel
            escPanel.transform.SetAsLastSibling();

        }
        else
        {
            Time.timeScale = 1f; // Resume time
            if (escPanel != null) escPanel.SetActive(false); // Hide pause panel
        }
    }

    public void LoadTitle()
    {
        SceneManager.LoadScene("Start");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool IsGameOverOrClear()
    {
        return isGameOver || isGameClear;
    }
}