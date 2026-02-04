using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public enum GamePhase
{
    Select,
    Play,
    Buff,
    ResultWin,
    ResultLose
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

    [Header("Level Generation")]
    [SerializeField] private List<LevelPool> allLevelPools; // Gán 3 Pool (Diff 1, 2, 3) trong Inspector
    [SerializeField] private List<LevelData> currentRunLevels = new(); // Chứa 3 level đã chọn cho Run này

    [SerializeField] private Image enemyImg;
    [SerializeField] private List<Sprite> enemySprites;


    public void StateHeath()
    {
        if (Score >= EnemyScore)
        {
            enemyHp--;
            enemyHpText.text = enemyHp.ToString();
            enemyImg.sprite = enemySprites[enemyHp];
            if (enemyHp > 0) UpdateCurrentLevelByEnemyHP();
        }
        else
        {
            hp--;
            hpText.text = hp.ToString();
        }

        CheckGameOver();
    }

    private void CheckGameOver()
    {
        if (enemyHp <= 0)
        {
            GoToResultWin(); // Thắng Run
            SceneFlowManager.Instance.IsRestart = true;
        }
        else if (hp <= 0)
        {
            GoToResultLose(); // Thua Run
            SceneFlowManager.Instance.IsRestart = true;
            Debug.Log("OK");
        }
        else
        {
            DOVirtual.DelayedCall(1f, () => { GamePlayManager.Instance.GoToSelectMask(); });
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
        InitializeNewRun();
        ResetScore(); ResetEnemyScore();
        ChangePhase(GamePhase.Select);
    }

    private void InitializeNewRun()
    {
        // 1. Reset HP
        hp = 3; hpText.text = hp.ToString();
        enemyHp = 3; enemyHpText.text = enemyHp.ToString();

        // 2. Sinh ngẫu nhiên 3 Level cho Run này
        currentRunLevels.Clear();

        for (int diff = 1; diff <= 3; diff++)
        {
            LevelPool pool = allLevelPools.Find(p => p.difficulty == diff);
            if (pool != null && pool.levels.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, pool.levels.Count);
                currentRunLevels.Add(pool.levels[randomIndex]);
            }
        }

        UpdateCurrentLevelByEnemyHP();
    }

    private void UpdateCurrentLevelByEnemyHP()
    {
        // Logic: 3 HP -> Index 0 (Diff 1), 2 HP -> Index 1 (Diff 2), 1 HP -> Index 2 (Diff 3)
        int levelIndex = 3 - enemyHp;

        // Clamp để tránh IndexOutOfRange nếu enemyHp biến động bất thường
        levelIndex = Mathf.Clamp(levelIndex, 0, currentRunLevels.Count - 1);

        currentLevelData = currentRunLevels[levelIndex];
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
            case GamePhase.ResultWin:
                // Tính điểm cuối cùng và hiển thị
                AudioManager.Instance.Play(GameSound.win);
                break;
            case GamePhase.ResultLose:
                // Tính điểm cuối cùng và hiển thị
                AudioManager.Instance.Play(GameSound.lose);
                break;
        }
    }

    // Shortcut methods cho UI Buttons
    public void GoToSelectMask() => ChangePhase(GamePhase.Select);
    public void GoToPlay() => ChangePhase(GamePhase.Play);
    public void GoToSelectBuffs() => ChangePhase(GamePhase.Buff);
    public void GoToResultWin() => ChangePhase(GamePhase.ResultWin);
    public void GoToResultLose() => ChangePhase(GamePhase.ResultLose);
}