using UnityEngine;
using System.Collections.Generic;

public class PhasePlayInputManager : MonoBehaviour
{
    [SerializeField] private LayerMask objectMaskLayer;

    [Space(5)]
    [SerializeField] private ObjectMask selectedMask;
    [SerializeField] private Vector3 offset;

    [SerializeField] private Vector2 _minBound;
    [SerializeField] private Vector2 _maxBound;

    private void OnEnable()
    {
        _minBound = PhasePlayManager.Instance.MinBound;
        _maxBound = PhasePlayManager.Instance.MaxBound;
    }

    private void Update()
    {
        if (!PhasePlayManager.Instance.IsGameStart)
        {
            HideAllBound();
            return;
        }

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
        Vector3 targetPos = mouseWorldPos + offset;
        targetPos = new Vector3(targetPos.x, targetPos.y, 0);

        float halfWidth = selectedMask.size.x / 2f;
        float halfHeight = selectedMask.size.y / 2f;
        targetPos.x = Mathf.Clamp(targetPos.x, _minBound.x + halfWidth, _maxBound.x - halfWidth);
        targetPos.y = Mathf.Clamp(targetPos.y, _minBound.y + halfHeight, _maxBound.y - halfHeight);

        selectedMask.transform.position = targetPos;
        selectedMask.SnapToGrid();

        ShowAllBound();
    }

    private void PointerUp()
    {
        if (selectedMask == null) return;
        AudioManager.Instance.Play(GameSound.throwMask);
        selectedMask.SnapToGrid();
        selectedMask = null;

        HideAllBound();
    }

    private void ShowAllBound()
    {
        foreach (ObjectMask mask in PhasePlayManager.Instance.Masks)
        {
            mask.ShowBound();
        }
        foreach (ObjectItem obj in PhasePlayManager.Instance.Objects)
        {
            obj.ShowBound();
        }
    }

    private void HideAllBound()
    {
        foreach (ObjectMask mask in PhasePlayManager.Instance.Masks)
        {
            if (mask == null) continue;
            mask.HideBound();
        }
        foreach (ObjectItem obj in PhasePlayManager.Instance.Objects)
        {
            if (obj == null) continue;
            obj.HideBound();
        }
    }
}