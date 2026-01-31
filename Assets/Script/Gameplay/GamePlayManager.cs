using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

public enum GamePhase
{
    Select,
    Play,
    Buff,
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

    [Header("Info")]
    [SerializeField] private int score;
    [SerializeField] private int enemyScore;
    public int Score => score;
    public int EnemyScore => enemyScore;
    [SerializeField] private int hp;
    [SerializeField] private int enemyHp;
    public int HP => hp;
    public int EnemyHP => enemyHp;
    [SerializeField] private ScoreEffect scoreEffect;
    [SerializeField] private ScoreEffect enemyScoreEffect;

    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI enemyHpText;

    public void StateHeath()
    {
        if (Score >= EnemyScore)
        {
            enemyHp--;
            enemyHpText.text = enemyHp.ToString();
        }
        else
        {
            hp--;
            hpText.text = hp.ToString();
        }
    }

    public void ResetScore()
    {
        score = 0;
        scoreEffect.SetScore(score);
    }

    public void ResetEnemyScore()
    {
        enemyScore = 0;
        enemyScoreEffect.SetScore(score);
    }

    public void AddScore(int amount)
    {
        int newScore = score + amount;
        AudioManager.Instance.Play(GameSound.score);
        scoreEffect.AddScore(score, newScore);
        score = newScore;
    }

    public void AddEnemyScore(int amount)
    {
        int newScore = enemyScore + amount;
        AudioManager.Instance.Play(GameSound.score);
        enemyScoreEffect.AddScore(enemyScore, newScore);
        enemyScore = newScore;
    }

    private void Start()
    {
        ResetScore(); ResetEnemyScore();
        enemyHp = 3;
        enemyHpText.text = enemyHp.ToString();
        hp = 3;
        hpText.text = hp.ToString();
        ChangePhase(GamePhase.Select);
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
            case GamePhase.Select:
                ResetScore(); ResetEnemyScore();
                PhaseSelectManager.Instance.Init();
                break;
            case GamePhase.Play:
                PhasePlayManager.Instance.Init();
                break;
            case GamePhase.Buff:
                // Hiển thị danh sách buff cho người chơi chọn
                break;
            case GamePhase.Result:
                // Tính điểm cuối cùng và hiển thị
                break;
        }
    }

    // Shortcut methods cho UI Buttons
    public void GoToSelectMask() => ChangePhase(GamePhase.Select);
    public void GoToPlay() => ChangePhase(GamePhase.Play);
    public void GoToSelectBuffs() => ChangePhase(GamePhase.Buff);
    public void GoToResult() => ChangePhase(GamePhase.Result);
}