using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoverManager : Singleton<CoverManager>
{
    [SerializeField] private float _cellSize = 0.5f;

    public float CellSize => _cellSize;

    [SerializeField] private List<ObjectMask> masks;
    [SerializeField] private List<ObjectItem> objects;

    public List<ObjectMask> Masks => masks;
    public List<ObjectItem> Objects => objects;

    [Header("Status")]
    [SerializeField] private Dictionary<ObjectItem, bool> _coverageResults = new();
    private bool _isProcessing = false;

    //public bool IsFullyCovered => _isFullyCovered;
    public bool IsObjectCovered(ObjectItem obj)
            => _coverageResults.TryGetValue(obj, out bool covered) && covered;

    private void Update()
    {
        if (!_isProcessing && objects.Count > 0)
        {
            StartCoroutine(CheckAllCoverageRoutine());
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
}