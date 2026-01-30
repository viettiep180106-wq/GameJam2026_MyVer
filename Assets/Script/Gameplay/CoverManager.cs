using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class CoverManager : Singleton<CoverManager>
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
    [SerializeField] private LevelData currentLevelData;
    [SerializeField] private Transform objectContainer;

    public bool IsObjectCovered(ObjectItem obj)
            => _coverageResults.TryGetValue(obj, out bool covered) && covered;

    private bool _isGameStart;
    public bool IsGameStart => _isGameStart;

    private void Start()
    {
        _isGameStart = false;
        LoadLevel(currentLevelData);
        ExecuteSpawnInward();
    }

    private void Update()
    {
        if (!_isGameStart) return;

        if (!_isProcessing && objects.Count > 0)
        {
            StartCoroutine(CheckAllCoverageRoutine());
        }
    }

    public void LoadLevel(LevelData data)
    {
        // 1. Dọn dẹp danh sách cũ
        foreach (var obj in objects)
        {
            if (obj != null) Destroy(obj.gameObject);
        }
        objects.Clear();
        _coverageResults.Clear();

        // 2. Nạp dữ liệu mới
        foreach (var entry in data.entries)
        {
            GameObject prefab = data.GetPrefab(entry.gemData.gemType);

            if (prefab == null) continue;

            // Instantiate prefab tại vị trí lưu trong data
            GameObject go = Instantiate(prefab, entry.position, Quaternion.identity, objectContainer);
            ObjectItem item = go.GetComponent<ObjectItem>();

            if (item != null)
            {
                // Gán dữ liệu Gem từ entry vào ObjectItem
                // Cần đảm bảo ObjectItem có hàm/trường public để gán GemData
                item.Init(entry.gemData);
                objects.Add(item);
            }
        }
    }

    public void PlayMask()
    {
        // Dừng cập nhật coverage tự động để cố định kết quả tại thời điểm Play
        _isGameStart = false;

        // Đảm bảo Coroutine quét cuối cùng đã xong hoặc ép xung quét một lần cuối
        // Ở đây ta gọi hàm tính điểm
        int finalScore = CalculateFinalScore();

        Debug.Log($"Final Score: {finalScore}");

        // Hiệu ứng cho các item được chọn (ví dụ: biến mất hoặc bay về kho)
        foreach (var obj in objects)
        {
            if (IsObjectCovered(obj))
            {
                obj.transform.DOScale(0, 0.5f).SetEase(Ease.InBack);
            }
        }
    }

    private int CalculateFinalScore()
    {
        // 1. Lọc ra các item đã bị phủ kín hoàn toàn
        List<ObjectItem> coveredItems = new List<ObjectItem>();
        foreach (var obj in objects)
        {
            if (IsObjectCovered(obj)) coveredItems.Add(obj);
        }

        // 2. Gom nhóm giá trị theo GemType
        Dictionary<GemType, int> typeScores = new Dictionary<GemType, int>();
        foreach (ObjectItem item in coveredItems)
        {
            if (!typeScores.ContainsKey(item.GemType)) typeScores[item.GemType] = 0;
            typeScores[item.GemType] += item.GemBaseValue;
        }

        int totalScore = 0;

        // 3. Áp dụng Buff theo từng loại GemType
        foreach (var kvp in typeScores)
        {
            GemType type = kvp.Key;
            float currentTypeScore = kvp.Value;

            // Tìm tất cả buff liên quan đến GemType này
            List<GemBuff> relevantBuffs = currentBuffs.FindAll(b => b.gemType == type);

            // Ưu tiên tính các buff Cộng trước, Nhân sau
            // Cộng (Plus)
            foreach (var buff in relevantBuffs)
            {
                if (buff.gemBuffType == GemBuffType.Plus)
                    currentTypeScore += buff.value;
            }

            // Nhân (Multiple)
            foreach (var buff in relevantBuffs)
            {
                if (buff.gemBuffType == GemBuffType.Multiple)
                    currentTypeScore *= buff.value;
            }

            totalScore += Mathf.RoundToInt(currentTypeScore);
        }

        return totalScore;
    }

    private void ExecuteSpawnInward()
    {
        foreach (var obj in objects)
        {
            if (obj == null) continue;

            Vector3 targetPos = obj.transform.position;
            Vector3 direction = ((targetPos - center) * -1).normalized;
            Vector3 directionRand = new(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
            float distToCenter = Vector3.Distance(targetPos, center);

            // Tính toán vị trí xuất phát ngoài biên
            Vector3 startPos = targetPos + (direction + directionRand) * (_spawnBoundRadius + distToCenter * _distanceMultiplier);

            // Set vị trí ban đầu và bắt đầu Tween
            obj.transform.position = startPos;
            obj.transform.DOMove(targetPos, _duration)
                .SetEase(_moveEase)
                .OnComplete(() => {
                    // Đánh dấu hoàn tất khi object cuối cùng về đích
                    if (obj == objects[objects.Count - 1]) _isGameStart = true;
                });
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