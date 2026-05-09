using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // Coroutineに必要

public class TitleManager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject difficultiesPanel;
    public TMP_Dropdown dropdown;

    [Header("Camera Animation")]
    public Camera mainCamera;
    public float moveDuration = 2f; // the duration of the camera movement in seconds

    private Vector3 targetPos = new Vector3(39.3f, 43.1f, -517.1f);
    private Quaternion targetRot = Quaternion.Euler(-17.74f, -33.05f, -0.225f);

    private Vector3 InitialPos = new Vector3(-9f, 55f, -505f);
    private Quaternion InitialRot = Quaternion.Euler(-14.27f, 0.038f, 0.147f);

    void Start()
    {
        //set the time scale to 1 to ensure that the camera animation works correctly, even if the game was paused and returned to the title screen
        Time.timeScale = 1f;

        if (mainPanel != null) mainPanel.SetActive(true);
        if (difficultiesPanel != null) difficultiesPanel.SetActive(false);
    }

    public void OnFirstStartClick()
    {
        StopAllCoroutines();
        mainPanel.SetActive(false);
        StartCoroutine(MoveCameraSmoothly(targetPos, targetRot, difficultiesPanel));
    }

    public void OnBackClick()
    {
        StopAllCoroutines();
        difficultiesPanel.SetActive(false);
        StartCoroutine(MoveCameraSmoothly(InitialPos, InitialRot, mainPanel));
    }


    public void OnFinalStartClick()
    {
        int[] counts = { 10, 15, 20 };
        GlobalSetting.EnemyCount = counts[dropdown.value];
        SceneManager.LoadScene("GameScene");
    }

    public void OnEndGameClick()
    {
        Application.Quit();
    }


    IEnumerator MoveCameraSmoothly(Vector3 targetPos, Quaternion targetRot, GameObject panel)
    {
        float elapsed = 0f;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        while (elapsed < moveDuration)
        {
            // add the time to the elapsed time to avoid the  loop runnign infinitely and to ensurethe animation progresses
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            float curve = t * t * (3f - 2f * t);

            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, curve);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, curve);

            yield return null;
        }

        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRot;

        panel.SetActive(true);
    }
}