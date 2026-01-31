using UnityEngine;
using System;
using System.Collections.Generic;

public enum GamePhase
{
    SelectMask,
    Play,
    SelectBuffs,
    Result
}

[Serializable]
public class PhaseObject
{
    public GamePhase phase;
    public GameObject container;
}

public class GamePlayManager : Singleton<GamePlayManager>
{
    [Header("Phases Configuration")]
    [SerializeField] private List<PhaseObject> phaseObjects;

    [SerializeField] private GamePhase _currentPhase;
    public GamePhase CurrentPhase => _currentPhase;

    [Header("Level")]
    [SerializeField] private LevelData currentLevelData;
    public LevelData CurrentLevelData => currentLevelData;

    private void Start()
    {
        ChangePhase(GamePhase.SelectMask);
    }

    public void ChangePhase(GamePhase newPhase)
    {
        _currentPhase = newPhase;

        foreach (var p in phaseObjects)
        {
            if (p.container != null)
            {
                // Enable container nếu trùng phase, ngược lại disable
                p.container.SetActive(p.phase == _currentPhase);
            }
        }

        HandlePhaseLogic(newPhase);
    }

    private void HandlePhaseLogic(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.SelectMask:
                break;
            case GamePhase.Play:
                PhasePlayManager.Instance.Init();
                break;
            case GamePhase.SelectBuffs:
                // Hiển thị danh sách buff cho người chơi chọn
                break;
            case GamePhase.Result:
                // Tính điểm cuối cùng và hiển thị
                break;
        }
    }

    // Shortcut methods cho UI Buttons
    public void GoToSelectMask() => ChangePhase(GamePhase.SelectMask);
    public void GoToPlay() => ChangePhase(GamePhase.Play);
    public void GoToSelectBuffs() => ChangePhase(GamePhase.SelectBuffs);
    public void GoToResult() => ChangePhase(GamePhase.Result);
}