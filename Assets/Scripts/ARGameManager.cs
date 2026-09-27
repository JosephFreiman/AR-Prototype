using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Tap a detected surface to place the arena, then pop as many targets as you can before time runs out.
public class ARGameManager : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] ARRaycastManager raycastManager;
    [SerializeField] ARPlaneManager planeManager;
    [SerializeField] Camera arCamera;

    [Header("Game")]
    [SerializeField] GameObject targetPrefab;
    [SerializeField] float gameDuration = 30f;
    [SerializeField] float spawnInterval = 0.8f;
    [SerializeField] float spawnRadius = 0.5f;
    [SerializeField] float targetLifetime = 2.5f;

    [Header("UI")]
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text infoText;

    static readonly List<ARRaycastHit> hits = new();

    Vector3 arenaCenter;
    bool placed, playing;
    float timeLeft, spawnTimer;
    int score, highScore;

    void Start()
    {
        infoText.text = "Move your phone slowly to scan the floor or a table.\nThen tap a surface to place the arena.";
        scoreText.text = "";
        timerText.text = "";
    }

    void Update()
    {
        if (playing) Tick();

        // Pointer covers both touchscreen (phone) and mouse (editor testing)
        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;
        Vector2 screenPos = pointer.position.ReadValue();

        if (!placed) TryPlaceArena(screenPos);
        else if (playing) TryHitTarget(screenPos);
        else StartGame(); // tap to play again after game over
    }

    void TryPlaceArena(Vector2 screenPos)
    {
        if (!raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon)) return;

        arenaCenter = hits[0].pose.position;
        placed = true;

        // Stop detecting new planes and hide the visualizers so the game is cleaner
        planeManager.enabled = false;
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(false);

        StartGame();
    }

    void StartGame()
    {
        score = 0;
        timeLeft = gameDuration;
        spawnTimer = 0f;
        playing = true;
        infoText.text = "";
        UpdateUI();
    }

    void Tick()
    {
        timeLeft -= Time.deltaTime;
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnTarget();
            spawnTimer = spawnInterval;
        }

        if (timeLeft <= 0f) EndGame();
        UpdateUI();
    }

    void SpawnTarget()
    {
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = arenaCenter + new Vector3(offset.x, Random.Range(0.1f, 0.4f), offset.y);
        GameObject t = Instantiate(targetPrefab, pos, Quaternion.identity);
        Destroy(t, targetLifetime);
    }

    void TryHitTarget(Vector2 screenPos)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 20f) &&
            hit.collider.TryGetComponent(out Target target))
        {
            score += target.points;
            target.Pop();
        }
    }

    void EndGame()
    {
        playing = false;
        timeLeft = 0f;
        if (score > highScore) highScore = score;

        foreach (var t in FindObjectsByType<Target>(FindObjectsSortMode.None))
            Destroy(t.gameObject);

        infoText.text = $"Time's up!\nScore: {score}   Best: {highScore}\nTap to play again";
    }

    void UpdateUI()
    {
        scoreText.text = $"Score: {score}";
        timerText.text = $"Time: {Mathf.CeilToInt(timeLeft)}";
    }
}
