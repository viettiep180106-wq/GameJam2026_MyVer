using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    [SerializeField] private LayerMask objectMaskLayer;

    [Space(5)]
    [SerializeField] private ObjectMask selectedMask;
    [SerializeField] private Vector3 offset;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) PointerDown();
        if (Input.GetMouseButton(0)) PointerHold();
        if (Input.GetMouseButtonUp(0)) PointerUp();
    }

    private void PointerDown()
    {
        Vector2 mouseWorldPos = Utilities.GetMouseWorldPos();
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, objectMaskLayer);

        if (hit.collider == null) return;

        selectedMask = hit.collider.GetComponent<ObjectMask>();
        if (selectedMask == null) return;

        offset = selectedMask.transform.position - (Vector3)mouseWorldPos;
    }

    private void PointerHold()
    {
        if (selectedMask == null) return;

        Vector3 mouseWorldPos = Utilities.GetMouseWorldPos();
        selectedMask.transform.position = mouseWorldPos + offset;
        selectedMask.transform.position =
                new Vector3(selectedMask.transform.position.x, selectedMask.transform.position.y, 0);
        selectedMask.SnapToGrid();

        ShowAllBound();
    }

    private void PointerUp()
    {
        if (selectedMask == null) return;

        selectedMask.SnapToGrid();
        selectedMask = null;

        HideAllBound();
    }

    private void ShowAllBound()
    {
        foreach (ObjectMask mask in CoverManager.Instance.Masks)
        {
            mask.ShowBound();
        }
        foreach (ObjectItem obj in CoverManager.Instance.Objects)
        {
            obj.ShowBound();
        }
    }

    private void HideAllBound()
    {
        foreach (ObjectMask mask in CoverManager.Instance.Masks)
        {
            mask.HideBound();
        }
        foreach (ObjectItem obj in CoverManager.Instance.Objects)
        {
            obj.HideBound();
        }
    }
}