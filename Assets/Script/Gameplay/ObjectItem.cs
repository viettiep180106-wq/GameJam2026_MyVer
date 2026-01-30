using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ObjectItem : MonoBehaviour
{
    public Vector2[] localPoints;
    [SerializeField] private float _cellSize;

    [Header("Object")]
    [SerializeField] private LineRenderer _lineRenderer;

    [Header("Line Settings")]
    [SerializeField] private float _boundaryWidth = 0.01f;
    [SerializeField] private float _pointMarkerSize = 0.1f;
    [SerializeField] private Color _boundaryColor_selected = Color.green;
    [SerializeField] private Color _boundaryColor_unselected = Color.red;
    [SerializeField] private Color _pointColor = Color.red;

    private void Start()
    {
        _cellSize = CoverManager.Instance.CellSize;
    }

    private void Update()
    {
        //ShowBound(); //
    }

    public void LoadBound()
    {
        if (localPoints == null || localPoints.Length < 3) return;
        Vector2[] vertices = GetWorldPoints();

        // Cập nhật LineRenderer
        Color _boundaryColor = CoverManager.Instance.IsObjectCovered(this) ?
                _boundaryColor_selected : _boundaryColor_unselected;
        _lineRenderer.startWidth = _boundaryWidth;
        _lineRenderer.endWidth = _boundaryWidth;
        _lineRenderer.startColor = _boundaryColor;
        _lineRenderer.endColor = _boundaryColor;
        _lineRenderer.positionCount = vertices.Length;
        _lineRenderer.loop = true;

        for (int i = 0; i < vertices.Length; i++)
            _lineRenderer.SetPosition(i, new Vector3(vertices[i].x, vertices[i].y, 0));

        // Quét Grid Points
        float minY = float.MaxValue, maxY = float.MinValue;
        foreach (var v in vertices) {
            if (v.y < minY) minY = v.y;
            if (v.y > maxY) maxY = v.y;
        }

        int startGY = Mathf.FloorToInt(minY / _cellSize);
        int endGY = Mathf.FloorToInt(maxY / _cellSize);

        for (int gy = startGY; gy <= endGY; gy++)
        {
            float yCenter = (gy + 0.5f) * _cellSize;
            if (TryGetXRange(vertices, yCenter, out float xMin, out float xMax))
            {
                int startGX = Mathf.FloorToInt((xMin + 0.001f) / _cellSize);
                int endGX = Mathf.FloorToInt((xMax - 0.001f) / _cellSize);

                for (int gx = startGX; gx <= endGX; gx++)
                {
                    Vector3 pos = new Vector3((gx + 0.5f) * _cellSize, yCenter, 0);
                    DrawRuntimeCross(pos, _cellSize * _pointMarkerSize, _pointColor);
                }
            }
        }
    }

    public void ShowBound()
    {
        LoadBound();
        _lineRenderer.enabled = true;
    }

    public void HideBound()
    {
        _lineRenderer.enabled = false;
    }

    private void DrawRuntimeCross(Vector3 pos, float size, Color color)
    {
        // Debug.DrawLine không có thuộc tính width, size ở đây điều chỉnh độ dài chữ thập
        Debug.DrawLine(pos + Vector3.left * size, pos + Vector3.right * size, color);
        Debug.DrawLine(pos + Vector3.up * size, pos + Vector3.down * size, color);
    }

    public Vector2[] GetWorldPoints()
    {
        if (localPoints == null) return new Vector2[0];
        Vector2[] worldPoints = new Vector2[localPoints.Length];
        for (int i = 0; i < localPoints.Length; i++)
            worldPoints[i] = transform.TransformPoint(localPoints[i]);
        return worldPoints;
    }

    public bool TryGetXRange(Vector2[] poly, float y, out float xMin, out float xMax)
    {
        xMin = float.MaxValue; xMax = float.MinValue;
        int intersections = 0;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            if ((poly[i].y <= y && poly[j].y > y) || (poly[j].y <= y && poly[i].y > y))
            {
                float x = poly[i].x + (y - poly[i].y) * (poly[j].x - poly[i].x) / (poly[j].y - poly[i].y);
                xMin = Mathf.Min(xMin, x);
                xMax = Mathf.Max(xMax, x);
                intersections++;
            }
        }
        return intersections >= 2;
    }

    // private void OnDrawGizmos()
    // {
    //     if (localPoints == null || localPoints.Length < 3) return;
    //     Vector2[] vertices = GetWorldPoints();
    //     Gizmos.color = _boundaryColor;
    //     for (int i = 0; i < vertices.Length; i++)
    //         Gizmos.DrawLine(vertices[i], vertices[(i + 1) % vertices.Length]);

    //     float minY = float.MaxValue, maxY = float.MinValue;
    //     foreach (var v in vertices) {
    //         if (v.y < minY) minY = v.y;
    //         if (v.y > maxY) maxY = v.y;
    //     }

    //     int startGY = Mathf.FloorToInt(minY / _cellSize);
    //     int endGY = Mathf.FloorToInt(maxY / _cellSize);

    //     Gizmos.color = _pointColor;
    //     for (int gy = startGY; gy <= endGY; gy++)
    //     {
    //         float yCenter = (gy + 0.5f) * _cellSize;
    //         if (TryGetXRange(vertices, yCenter, out float xMin, out float xMax))
    //         {
    //             int startGX = Mathf.FloorToInt((xMin + 0.001f) / _cellSize);
    //             int endGX = Mathf.FloorToInt((xMax - 0.001f) / _cellSize);
    //             for (int gx = startGX; gx <= endGX; gx++)
    //             {
    //                 Vector3 pos = new Vector3((gx + 0.5f) * _cellSize, yCenter, 0);
    //                 Gizmos.DrawSphere(pos, _cellSize * _pointMarkerSize);
    //             }
    //         }
    //     }
    // }
}

// using UnityEngine;
// using System.Collections.Generic;

// [RequireComponent(typeof(LineRenderer))]
// public class ComplexObject : MonoBehaviour
// {
//     public Vector2[] localPoints;
//     [SerializeField] private float _cellSize;
//     private LineRenderer _lineRenderer;

//     private void Awake()
//     {
//         _lineRenderer = GetComponent<LineRenderer>();
//         // Thiết lập cơ bản cho LineRenderer
//         _lineRenderer.loop = true;
//         _lineRenderer.startWidth = 0.05f;
//         _lineRenderer.endWidth = 0.05f;
//         _lineRenderer.useWorldSpace = true;
//     }

//     private void Start()
//     {
//         if (CoverManager.Instance != null)
//             _cellSize = CoverManager.Instance.CellSize;
//     }

//     private void Update()
//     {
//         UpdateRuntimeVisuals();
//     }

//     private void UpdateRuntimeVisuals()
//     {
//         if (localPoints == null || localPoints.Length < 3) return;
//         Vector2[] vertices = GetWorldPoints();

//         // 1. Vẽ biên đa giác bằng LineRenderer
//         _lineRenderer.positionCount = vertices.Length;
//         for (int i = 0; i < vertices.Length; i++)
//         {
//             _lineRenderer.SetPosition(i, new Vector3(vertices[i].x, vertices[i].y, 0));
//         }

//         // 2. Vẽ các điểm quét (Grid Points) bằng Debug.DrawLine để quan sát trong Runtime
//         float minY = float.MaxValue, maxY = float.MinValue;
//         foreach (var v in vertices) {
//             if (v.y < minY) minY = v.y;
//             if (v.y > maxY) maxY = v.y;
//         }

//         int startGY = Mathf.FloorToInt(minY / _cellSize);
//         int endGY = Mathf.FloorToInt(maxY / _cellSize);

//         for (int gy = startGY; gy <= endGY; gy++)
//         {
//             float yCenter = (gy + 0.5f) * _cellSize;
//             if (TryGetXRange(vertices, yCenter, out float xMin, out float xMax))
//             {
//                 int startGX = Mathf.FloorToInt((xMin + 0.001f) / _cellSize);
//                 int endGX = Mathf.FloorToInt((xMax - 0.001f) / _cellSize);

//                 for (int gx = startGX; gx <= endGX; gx++)
//                 {
//                     Vector3 pos = new Vector3((gx + 0.5f) * _cellSize, yCenter, 0);
//                     // Vẽ chữ thập nhỏ thay cho Sphere vì Runtime không có Gizmos.DrawSphere
//                     DrawRuntimeCross(pos, _cellSize * 0.1f, Color.red);
//                 }
//             }
//         }
//     }

//     private void DrawRuntimeCross(Vector3 pos, float size, Color color)
//     {
//         Debug.DrawLine(pos + Vector3.left * size, pos + Vector3.right * size, color);
//         Debug.DrawLine(pos + Vector3.up * size, pos + Vector3.down * size, color);
//     }

//     public Vector2[] GetWorldPoints()
//     {
//         if (localPoints == null) return new Vector2[0];
//         Vector2[] worldPoints = new Vector2[localPoints.Length];
//         for (int i = 0; i < localPoints.Length; i++)
//             worldPoints[i] = transform.TransformPoint(localPoints[i]);
//         return worldPoints;
//     }

//     public bool TryGetXRange(Vector2[] poly, float y, out float xMin, out float xMax)
//     {
//         xMin = float.MaxValue;
//         xMax = float.MinValue;
//         int intersections = 0;
//         for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
//         {
//             if ((poly[i].y <= y && poly[j].y > y) || (poly[j].y <= y && poly[i].y > y))
//             {
//                 float x = poly[i].x + (y - poly[i].y) * (poly[j].x - poly[i].x) / (poly[j].y - poly[i].y);
//                 xMin = Mathf.Min(xMin, x);
//                 xMax = Mathf.Max(xMax, x);
//                 intersections++;
//             }
//         }
//         return intersections >= 2;
//     }

//     private void OnDrawGizmos()
//     {
//         if (localPoints == null || localPoints.Length < 3) return;
//         Vector2[] vertices = GetWorldPoints();
//         Gizmos.color = Color.green;
//         for (int i = 0; i < vertices.Length; i++)
//             Gizmos.DrawLine(vertices[i], vertices[(i + 1) % vertices.Length]);

//         float minY = float.MaxValue, maxY = float.MinValue;
//         foreach (var v in vertices) {
//             if (v.y < minY) minY = v.y;
//             if (v.y > maxY) maxY = v.y;
//         }

//         int startGY = Mathf.FloorToInt(minY / _cellSize);
//         int endGY = Mathf.FloorToInt(maxY / _cellSize);

//         Gizmos.color = Color.red;
//         for (int gy = startGY; gy <= endGY; gy++)
//         {
//             float yCenter = (gy + 0.5f) * _cellSize;
//             if (TryGetXRange(vertices, yCenter, out float xMin, out float xMax))
//             {
//                 int startGX = Mathf.FloorToInt((xMin + 0.001f) / _cellSize);
//                 int endGX = Mathf.FloorToInt((xMax - 0.001f) / _cellSize);
//                 for (int gx = startGX; gx <= endGX; gx++)
//                 {
//                     Vector3 pos = new Vector3((gx + 0.5f) * _cellSize, yCenter, 0);
//                     Gizmos.DrawSphere(pos, _cellSize * 0.1f);
//                 }
//             }
//         }
//     }
// }


// using UnityEngine;

// public class ComplexObject : MonoBehaviour
// {
//     public Vector2[] localPoints;
//     [SerializeField] private float _cellSize;

//     private void Start()
//     {
//         _cellSize = CoverManager.Instance.CellSize;
//     }

//     public Vector2[] GetWorldPoints()
//     {
//         if (localPoints == null) return new Vector2[0];
//         Vector2[] worldPoints = new Vector2[localPoints.Length];
//         for (int i = 0; i < localPoints.Length; i++)
//             worldPoints[i] = transform.TransformPoint(localPoints[i]);
//         return worldPoints;
//     }

//     private void OnDrawGizmos()
//     {
//         if (localPoints == null || localPoints.Length < 3) return;
//         Vector2[] vertices = GetWorldPoints();

//         // Ve canh da giac
//         Gizmos.color = Color.green;
//         for (int i = 0; i < vertices.Length; i++)
//             Gizmos.DrawLine(vertices[i], vertices[(i + 1) % vertices.Length]);

//         // Xac dinh gioi han quet
//         float minY = float.MaxValue, maxY = float.MinValue;
//         foreach (var v in vertices) {
//             if (v.y < minY) minY = v.y;
//             if (v.y > maxY) maxY = v.y;
//         }

//         int startGY = Mathf.FloorToInt(minY / _cellSize);
//         int endGY = Mathf.FloorToInt(maxY / _cellSize);

//         Gizmos.color = Color.red;
//         for (int gy = startGY; gy <= endGY; gy++)
//         {
//             float yCenter = (gy + 0.5f) * _cellSize;
//             if (TryGetXRange(vertices, yCenter, out float xMin, out float xMax))
//             {
//                 // Dung epsilon 0.001f de loai bo cac o nam ngoai bien do sai so so thuc
//                 int startGX = Mathf.FloorToInt((xMin + 0.001f) / _cellSize);
//                 int endGX = Mathf.FloorToInt((xMax - 0.001f) / _cellSize);

//                 for (int gx = startGX; gx <= endGX; gx++)
//                 {
//                     Vector3 pos = new Vector3((gx + 0.5f) * _cellSize, yCenter, 0);
//                     Gizmos.DrawSphere(pos, _cellSize * 0.1f);
//                 }
//             }
//         }
//     }

//     public bool TryGetXRange(Vector2[] poly, float y, out float xMin, out float xMax)
//     {
//         xMin = float.MaxValue;
//         xMax = float.MinValue;
//         int intersections = 0;

//         for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
//         {
//             // Kiem tra dong quet y co cat canh ij khong
//             if ((poly[i].y <= y && poly[j].y > y) || (poly[j].y <= y && poly[i].y > y))
//             {
//                 float x = poly[i].x + (y - poly[i].y) * (poly[j].x - poly[i].x) / (poly[j].y - poly[i].y);
//                 xMin = Mathf.Min(xMin, x);
//                 xMax = Mathf.Max(xMax, x);
//                 intersections++;
//             }
//         }
//         return intersections >= 2;
//     }
// }