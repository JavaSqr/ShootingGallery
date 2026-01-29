using CustomInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Space, Header("General")]
    [SerializeField] private List<GameMode> gameModes;

    [Space, Header("References")]
    [SerializeField] private GameplayManager gameplayManager;
    [SerializeField] private AimController aimController;
    [SerializeField] private Animator menuCanvasAnimator;
    [SerializeField] private Counter counter;

    [Space, Header("Test Settings")]
    [SerializeField] private bool isMobileTesting;

    [Space, Header("UI")]
    [SerializeField] private Transform mobileUI;

    [Space, Header("Pause")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseButton;

    [Button(nameof(StartGame), true)] public int GameModeID;

    public void StartGame(int gameModeID)
    {
        gameplayManager.SetGameMode(gameModes[gameModeID], aimController);
        gameplayManager.RunGame();
    }

    private void Awake()
    {
        StaticData.isMobileTesting = isMobileTesting;
        
        gameplayManager ??= FindFirstObjectByType<GameplayManager>();
        aimController ??= FindFirstObjectByType<AimController>();
    }

    private void Start()
    {
        mobileUI.gameObject.SetActive(isMobileTesting || Application.isMobilePlatform);
    }

    public void SetMenuCanvasTrigger(string trigger)
    {
        menuCanvasAnimator.SetTrigger(trigger);
    }

    #region Pause

    public void OpenPause()
    {
        Time.timeScale = 0f;
        pauseButton.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void ClosePause()
    {
        pauseButton.SetActive(true);
        pausePanel.SetActive(false);
        counter.gameObject.SetActive(true);
        counter.StartCountdown();
    }

    #endregion
}

public static class StaticData
{
    public static bool isMobileTesting;

    public static List<LineRenderer> movingLines = new List<LineRenderer>(3);
    public static List<Transform> staticSpawnPositions = new List<Transform>(6);
    public static List<Transform> hangingSpawnPositions = new List<Transform>(6);
}

public enum GMType { Infinite, Challenge }

[Serializable]
public class GameMode
{
    public string name;
    [TextArea(5, 10)]
    public string description;

    [Space]
    public GMType GameModeType;
    public AimPrefs aimPrefs;
    public SpawnPrefs spawnPrefs;
    public int initialHP = 3;
}