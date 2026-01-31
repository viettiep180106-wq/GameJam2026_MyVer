using UnityEngine;

public class ObjectMask : MonoBehaviour
{
    [Header("Object")]
    [SerializeField] public Vector2 size = Vector2.one;
    [SerializeField] public BoxCollider2D boxCollider;

    [Header("Border")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _lineWidth = 0.01f;
    [SerializeField] private Color _lineColor = Color.cyan;

    [Space(5)]
    private float _gridSize;

    void OnEnable() {
        _gridSize = PhasePlayManager.Instance.CellSize;
        boxCollider.size = size;
        SnapToGrid();
    }

    public void SnapToGrid() {
        Vector3 pos = transform.position;

        // Snap vi tri de tam hoac goc khop voi o luoi
        pos.x = Mathf.Round(pos.x / _gridSize) * _gridSize;
        pos.y = Mathf.Round(pos.y / _gridSize) * _gridSize;

        transform.position = pos;

        // Snap kich thuoc de luon la boi so cua gridSize
        // size.x = Mathf.Max(_gridSize, Mathf.Round(size.x / _gridSize) * _gridSize);
        // size.y = Mathf.Max(_gridSize, Mathf.Round(size.y / _gridSize) * _gridSize);
    }

    public Rect GetRect() {
        Vector2 pos = transform.position;
        // Rect tinh tu tam sau khi da snap
        return new Rect(pos.x - size.x / 2f, pos.y - size.y / 2f, size.x, size.y);
    }

    public void LoadBound()
    {
        Rect r = GetRect();

        _lineRenderer.startWidth = _lineWidth;
        _lineRenderer.endWidth = _lineWidth;
        _lineRenderer.startColor = _lineColor;
        _lineRenderer.endColor = _lineColor;
        _lineRenderer.loop = true;

        _lineRenderer.positionCount = 4;
        Vector3[] corners = new Vector3[] {
            new Vector3(r.xMin, r.yMin, 0),
            new Vector3(r.xMax, r.yMin, 0),
            new Vector3(r.xMax, r.yMax, 0),
            new Vector3(r.xMin, r.yMax, 0)
        };

        _lineRenderer.SetPositions(corners);
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

    private void OnDrawGizmos() {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Rect r = GetRect();
        Gizmos.DrawCube(new Vector3(r.center.x, r.center.y, 0), new Vector3(r.width, r.height, 0.1f));
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(r.center.x, r.center.y, 0), new Vector3(r.width, r.height, 0.1f));
    }
}