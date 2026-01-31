using UnityEngine;

public class PhaseSelectInputManager : MonoBehaviour
{
    [SerializeField] private LayerMask maskLayer;

    [Header("Selection")]
    [SerializeField] private MaskDrag selectedMask;
    [SerializeField] private Vector3 offset;

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        // Chỉ cho phép tương tác khi Game đang ở phase SelectMask
        if (!PhaseSelectManager.Instance.IsSelectionStart) return;

        if (Input.GetMouseButtonDown(0)) PointerDown();
        if (Input.GetMouseButton(0)) PointerHold();
        if (Input.GetMouseButtonUp(0)) PointerUp();
    }

    private void PointerDown()
    {
        Vector2 mouseWorldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, maskLayer);

        if (hit.collider == null) return;

        selectedMask = hit.collider.GetComponent<MaskDrag>();
        if (selectedMask == null) return;

        // Nếu mask đang nằm trong một box, báo box đó giải phóng mask
        if (selectedMask.currentBox != null)
        {
            selectedMask.currentBox.RemoveMark(selectedMask);
            selectedMask.currentBox = null;
        }

        offset = selectedMask.transform.position - (Vector3)mouseWorldPos;
    }

    private void PointerHold()
    {
        if (selectedMask == null) return;

        Vector3 mouseWorldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
        selectedMask.transform.position = new Vector3(mouseWorldPos.x + offset.x, mouseWorldPos.y + offset.y, 0);
    }

    private void PointerUp()
    {
        if (selectedMask == null) return;

        BoxMaskSlot targetBox = GetBoxUnderMask(selectedMask);

        // Kiểm tra nếu tìm thấy box và box còn chỗ trống
        if (targetBox != null && targetBox.CanAdd())
        {
            targetBox.AddMark(selectedMask);
            targetBox.PlaceMarkInside(selectedMask);
        }
        else
        {
            // Trả về vị trí khay chứa ban đầu
            selectedMask.ReturnToStart();
        }

        selectedMask = null;
    }

    private BoxMaskSlot GetBoxUnderMask(MaskDrag mask)
    {
        // Lấy tọa độ tâm của Mask từ BoxCollider2D (World Space)
        Vector3 maskCenter = mask.GetComponent<BoxCollider2D>().bounds.center;

        var boxes = PhaseSelectManager.Instance.GetActiveBoxes();

        foreach (var box in boxes)
        {
            if (box == null) continue;

            RectTransform boxRect = box.GetComponent<RectTransform>();

            // Lấy 4 góc của Box trong World Space
            Vector3[] corners = new Vector3[4];
            boxRect.GetWorldCorners(corners);

            // corners[0]: Bottom-Left, corners[2]: Top-Right
            Vector3 min = corners[0];
            Vector3 max = corners[2];

            // Kiểm tra xem tâm của Mask có nằm trong hình chữ nhật của Box không
            bool isPointInside = maskCenter.x >= min.x &&
                                maskCenter.x <= max.x &&
                                maskCenter.y >= min.y &&
                                maskCenter.y <= max.y;

            if (isPointInside) return box;
        }
        return null;
    }
}