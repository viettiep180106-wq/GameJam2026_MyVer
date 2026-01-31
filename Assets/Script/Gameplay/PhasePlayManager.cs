using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using System.Security.Cryptography;
using UnityEngine.SocialPlatforms.Impl;
using TMPro;
using System;

public class PhasePlayManager : Singleton<PhasePlayManager>
{
    [Header("Bound")]
    [SerializeField] private Vector2 minBound = new Vector2(-3, -3);
    [SerializeField] private Vector2 maxBound = new Vector2(3, 3);
    [SerializeField] private Vector3 center = Vector3.zero;
    public Vector2 MinBound => minBound;
    public Vector2 MaxBound => maxBound;

    [Header("Spawn")]
    [SerializeField] private float _spawnBoundRadius = 15f;
    [SerializeField] private float _distanceMultiplier = 1.5f;
    [SerializeField] private float _duration = 1.2f;
    [SerializeField] private Ease _moveEase = Ease.OutQuart;

    [SerializeField] private float _cellSize = 0.5f;

    public float CellSize => _cellSize;

    [SerializeField] private Transform boxMaskSlot;
    [SerializeField] private List<ObjectMask> masks;
    [SerializeField] private List<ObjectItem> objects;

    public List<ObjectMask> Masks => masks;
    public List<ObjectItem> Objects => objects;

    [Header("Status")]
    [SerializeField] private Dictionary<ObjectItem, bool> _coverageResults = new();
    private bool _isProcessing = false;

    [Header("GemBuff")]
    [SerializeField] private List<GemBuff> currentBuffs;

    [Header("Load Level")]
    [SerializeField] private Transform objectContainer;
    [SerializeField] private LevelData _currentLevelData;

    [SerializeField] private Button playMaskButton;
    [SerializeField] private bool _isGameStart;
    public bool IsGameStart => _isGameStart;

    [Header("Status")]
    [SerializeField] private Image timerFillBar;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float timeRemaining = 30f;
    [SerializeField] private float maxTime = 30f;
    //sprivate bool _isTimerRunning = false;

    bool timer3sWarning = false;

    public bool IsObjectCovered(ObjectItem obj)
            => _coverageResults.TryGetValue(obj, out bool covered) && covered;

    public void Init()
    {
        if (GamePlayManager.Instance.CurrentPhase != GamePhase.Play) return;

        playMaskButton.onClick.RemoveAllListeners();
        playMaskButton.onClick.AddListener(PlayMask);

        _isGameStart = false;
        timer3sWarning = false;
        timeRemaining = maxTime;
        _currentLevelData = GamePlayManager.Instance.CurrentLevelData;
        currentBuffs = new();
        _coverageResults = new();
        timerText.text = $"{(int)maxTime}s";

        for (int i = 0; i < PhaseSelectManager.Instance.Cards.Count; i++)
        {
            Card card = PhaseSelectManager.Instance.Cards[i];
            if (card == null) continue;
            // Chạy hiệu ứng lật thẻ tại Play Phase
            DOVirtual.DelayedCall(i * 0.15f, () => card.OnClickFlip());
            currentBuffs.Add(card.CardData.gemBuff);
        }

        DOVirtual.DelayedCall(1f, () =>
        {
            LoadLevel(_currentLevelData);
            ExecuteSpawnInward();
        });
    }

    private void Update()
    {
        if (!_isGameStart) return;

        if (!_isProcessing && objects.Count > 0)
        {
            StartCoroutine(CheckAllCoverageRoutine());
        }

        HandleTimer();
    }

    private void HandleTimer()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            timerFillBar.fillAmount = timeRemaining / maxTime;
            timerText.text = $"{(int)timeRemaining}s";

            if (timeRemaining < 3f && !timer3sWarning)
            {
                timer3sWarning = true;
                AudioManager.Instance.Play(GameSound.clock3sLast);
            }

            return;
        }

        timeRemaining = 0;
        timerText.text = $"{(int)timeRemaining}s";
        if (timerFillBar != null) timerFillBar.fillAmount = 0;

        // Tự động kết thúc trò chơi khi hết giờ
        PlayMask();
    }

    public void LoadLevel(LevelData data)
    {
        // 1. Dọn dẹp cũ
        foreach (var obj in objects) if (obj != null) Destroy(obj.gameObject);
        objects.Clear();
        _coverageResults.Clear();

        // 2. Spawn theo số lượng count trong từng entry
        foreach (var entry in data.entries)
        {
            GameObject prefab = data.GetPrefab(entry.gemData.gemType);
            if (prefab == null) continue;

            for (int i = 0; i < entry.count; i++)
            {
                // Spawn tất cả tại tâm để chuẩn bị nổ
                GameObject go = Instantiate(prefab);
                go.transform.SetParent(objectContainer);
                ObjectItem item = go.GetComponent<ObjectItem>();
                if (item != null)
                {
                    item.Init(entry.gemData);
                    objects.Add(item);
                }
            }
        }

        // mask
        foreach (var m in masks)
        {
            if (m != null) Destroy(m.gameObject);
        }
        masks.Clear();
        foreach (ObjectMask mask in boxMaskSlot.GetComponentsInChildren<ObjectMask>())
        {
            masks.Add(mask);
        }
    }

    public void PlayMask()
    {
        if (!_isGameStart) return;
        // Dừng cập nhật coverage tự động để cố định kết quả tại thời điểm Play
        _isGameStart = false;

        // Đảm bảo Coroutine quét cuối cùng đã xong hoặc ép xung quét một lần cuối
        // Ở đây ta gọi hàm tính điểm

        AudioManager.Instance.Play(GameSound.clickButton);

        ForceUpdateAllCoverage();
        StartCoroutine(CalculateFinalScoreRoutine());

        //Debug.Log($"Final Score: {GamePlayManager.Instance.Score}");

        // Hiệu ứng cho các item được chọn (ví dụ: biến mất hoặc bay về kho)
        foreach (var obj in objects)
        {
            if (!IsObjectCovered(obj))
            {
                DOVirtual.DelayedCall(UnityEngine.Random.Range(0f, 0.6f), () =>
                {
                    obj.transform.DOScale(0, 0.5f).SetEase(Ease.InBack);
                });
            }
        }
    }

    private IEnumerator CalculateFinalScoreRoutine()
    {
        // --- PHẦN 1: PHÂN LOẠI ITEM ---
        List<ObjectItem> playerItems = new List<ObjectItem>();
        List<ObjectItem> enemyItems = new List<ObjectItem>();

        foreach (var obj in objects)
        {
            if (IsObjectCovered(obj))
                playerItems.Add(obj);
            else
            {
                enemyItems.Add(obj);
                DOVirtual.DelayedCall(UnityEngine.Random.Range(0f, 0.5f), () =>
                {
                    obj.transform.DOScale(0, 0.5f).SetEase(Ease.InBack);
                });
            }
        }

        // --- PHẦN 3: TÍNH ĐIỂM CHO ENEMY (ITEM BỊ THU NHỎ) ---
        yield return StartCoroutine(ProcessScoreGroup(enemyItems, isPlayer: false));

        // Nghỉ một nhịp giữa hai lần tính điểm
        yield return new WaitForSeconds(0.5f);

        // --- PHẦN 2: TÍNH ĐIỂM CHO NGƯỜI CHƠI ---
        yield return StartCoroutine(ProcessScoreGroup(playerItems, isPlayer: true));

        Debug.Log("All calculations finished.");

        GamePlayManager.Instance.StateHeath();

        foreach (var obj in objects) if (obj != null) Destroy(obj.gameObject);
        objects.Clear();
        foreach (var m in masks)
        {
            if (m != null) Destroy(m.gameObject);
        }
    }

    private IEnumerator ProcessScoreGroup(List<ObjectItem> items, bool isPlayer)
    {
        // Gom nhóm theo Type
        Dictionary<GemType, int> typeScores = new Dictionary<GemType, int>();
        foreach (var item in items)
        {
            if (!typeScores.ContainsKey(item.GemType)) typeScores[item.GemType] = 0;
            typeScores[item.GemType] += item.GemBaseValue;
        }

        if (isPlayer) GamePlayManager.Instance.ResetScore();
        else GamePlayManager.Instance.ResetEnemyScore();


        if (isPlayer)
        {
            foreach (var kvp in typeScores)
            {
                GemType type = kvp.Key;
                float finalScore = kvp.Value;

                // Áp dụng Buff (Ví dụ hiện tại dùng chung List Buff, có thể tách nếu cần)
                List<GemBuff> relevantBuffs = currentBuffs.FindAll(b => b.gemType == type);

                foreach (var buff in relevantBuffs)
                    if (buff.gemBuffType == GemBuffType.Plus) finalScore += buff.value;

                foreach (var buff in relevantBuffs)
                    if (buff.gemBuffType == GemBuffType.Multiple) finalScore *= buff.value;

                int scoreValue = Math.Max(Mathf.RoundToInt(finalScore), 0);

                GamePlayManager.Instance.AddScore(scoreValue);

                Debug.Log($"{(isPlayer ? "Player" : "Enemy")} added {scoreValue} pts for {type}");
                yield return new WaitForSeconds(0.4f);
            }
        }
        else
        {
            foreach (var kvp in typeScores)
            {
                float finalScore = kvp.Value;
                int scoreValue = Mathf.RoundToInt(finalScore);
                GamePlayManager.Instance.AddEnemyScore(scoreValue);
                yield return new WaitForSeconds(0.4f);
            }
        }
    }

    private void ExecuteSpawnInward()
    {
        AudioManager.Instance.Play(GameSound.releaseItem);
        foreach (var obj in objects)
        {
            DOVirtual.DelayedCall(UnityEngine.Random.Range(0f, 0.5f), () =>
            {
                Rigidbody2D rb = obj.Body;
                if (rb == null) return;
                // Kích hoạt vật lý
                rb.simulated = true;
                rb.velocity = Vector2.zero;

                // Tạo hướng bắn ngẫu nhiên 360 độ
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 forceDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                // Đẩy nhẹ vị trí ban đầu để tránh kẹt collider tại tâm
                obj.transform.position = center + (Vector3)forceDir * UnityEngine.Random.Range(0.1f, 2f);

                // Tác động lực văng và lực xoay
                rb.AddForce(forceDir * UnityEngine.Random.Range(4f, 8f), ForceMode2D.Impulse);
                rb.AddTorque(UnityEngine.Random.Range(-10f, 10f), ForceMode2D.Impulse);
            });
        }

        DOVirtual.DelayedCall(1.5f, () => {
            // Logic thực thi tại đây
            foreach (var obj in objects)
            {
                obj.Body.simulated = false;
            }
            _isGameStart = true;
        });
    }

    private void ForceUpdateAllCoverage()
    {
        HashSet<Vector2Int> maskedCells = GetMaskedCells(masks);

        foreach (var target in objects)
        {
            if (target == null) continue;

            Vector2[] vertices = target.GetWorldPoints();
            if (vertices.Length < 3)
            {
                _coverageResults[target] = false;
                continue;
            }

            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var v in vertices)
            {
                if (v.y < minY) minY = v.y;
                if (v.y > maxY) maxY = v.y;
            }

            int startGY = Mathf.FloorToInt(minY / _cellSize);
            int endGY = Mathf.FloorToInt(maxY / _cellSize);

            bool isCurrentCovered = true;

            for (int gy = startGY; gy <= endGY; gy++)
            {
                float yCenter = (gy + 0.5f) * _cellSize;
                if (TryGetXRange(vertices, yCenter, out float xMin, out float xMax))
                {
                    int startGX = Mathf.FloorToInt((xMin + 0.001f) / _cellSize);
                    int endGX = Mathf.FloorToInt((xMax - 0.001f) / _cellSize);

                    for (int gx = startGX; gx <= endGX; gx++)
                    {
                        if (!maskedCells.Contains(new Vector2Int(gx, gy)))
                        {
                            isCurrentCovered = false;
                            break;
                        }
                    }
                }
                if (!isCurrentCovered) break;
            }
            _coverageResults[target] = isCurrentCovered;
        }
    }

    private IEnumerator CheckAllCoverageRoutine()
    {
        _isProcessing = true;

        // 1. Thu thập dữ liệu các ô đã bị che phủ (dùng chung cho tất cả target)
        HashSet<Vector2Int> maskedCells = GetMaskedCells(masks);

        const int MAX_POINTS_PER_FRAME = 300;
        int pointsProcessed = 0;

        foreach (var target in objects)
        {
            if (target == null) continue;

            Vector2[] vertices = target.GetWorldPoints();
            if (vertices.Length < 3)
            {
                _coverageResults[target] = false;
                continue;
            }

            // Tìm giới hạn của object hiện tại
            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var v in vertices) {
                if (v.y < minY) minY = v.y;
                if (v.y > maxY) maxY = v.y;
            }

            int startGY = Mathf.FloorToInt(minY / _cellSize);
            int endGY = Mathf.FloorToInt(maxY / _cellSize);

            bool isCurrentCovered = true;

            for (int gy = startGY; gy <= endGY; gy++)
            {
                float yCenter = (gy + 0.5f) * _cellSize;
                if (TryGetXRange(vertices, yCenter, out float xMin, out float xMax))
                {
                    int startGX = Mathf.FloorToInt((xMin + 0.001f) / _cellSize);
                    int endGX = Mathf.FloorToInt((xMax - 0.001f) / _cellSize);

                    for (int gx = startGX; gx <= endGX; gx++)
                    {
                        if (!maskedCells.Contains(new Vector2Int(gx, gy)))
                        {
                            isCurrentCovered = false;
                            break;
                        }

                        pointsProcessed++;
                        if (pointsProcessed >= MAX_POINTS_PER_FRAME)
                        {
                            pointsProcessed = 0;
                            yield return null;
                        }
                    }
                }
                if (!isCurrentCovered) break;
            }

            _coverageResults[target] = isCurrentCovered;
        }

        _isProcessing = false;
    }

    private bool TryGetXRange(Vector2[] poly, float y, out float xMin, out float xMax) {
        xMin = float.MaxValue; xMax = float.MinValue;
        int intersections = 0;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++) {
            if ((poly[i].y <= y && poly[j].y > y) || (poly[j].y <= y && poly[i].y > y)) {
                float x = poly[i].x + (y - poly[i].y) * (poly[j].x - poly[i].x) / (poly[j].y - poly[i].y);
                xMin = Mathf.Min(xMin, x); xMax = Mathf.Max(xMax, x);
                intersections++;
            }
        }
        return intersections >= 2;
    }

    private HashSet<Vector2Int> GetMaskedCells(List<ObjectMask> masks) {
        HashSet<Vector2Int> cells = new HashSet<Vector2Int>();
        foreach (var m in masks) {
            Rect r = m.GetRect();
            int minX = Mathf.FloorToInt((r.xMin + 0.001f) / _cellSize);
            int maxX = Mathf.FloorToInt((r.xMax - 0.001f) / _cellSize);
            int minY = Mathf.FloorToInt((r.yMin + 0.001f) / _cellSize);
            int maxY = Mathf.FloorToInt((r.yMax - 0.001f) / _cellSize);
            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    cells.Add(new Vector2Int(x, y));
        }
        return cells;
    }

    private void OnDrawGizmos()
    {
        // 1. Vẽ Center
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(center, 0.05f);
        Gizmos.DrawLine(center + Vector3.left, center + Vector3.right);
        Gizmos.DrawLine(center + Vector3.up, center + Vector3.down);

        // 2. Vẽ Bound giới hạn di chuyển
        Gizmos.color = Color.black;
        Vector3 size = new Vector3(maxBound.x - minBound.x, maxBound.y - minBound.y, 0);
        Vector3 pos = new Vector3((minBound.x + maxBound.x) / 2f, (minBound.y + maxBound.y) / 2f, 0);

        // // Vẽ khung bao ngoài
        Gizmos.DrawWireCube(pos, size);

        // // Vẽ vùng mờ bên trong để dễ quan sát phạm vi
        Gizmos.color = new Color(1, 1, 0, 0.05f);
        Gizmos.DrawCube(pos, size);
    }
}