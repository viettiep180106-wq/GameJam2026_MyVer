using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class BoxMaskSlot : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    [SerializeField] private int maxMask;
    public int MaxMask => maxMask;

    public int currentCount;
    public TMP_Text textCounter;

    public void Init(int maxMask)
    {
        this.maxMask = maxMask;
    }

    void OnEnable()
    {
        UpdateUI();
    }

    public bool CanAdd()
    {
        return currentCount < maxMask;
    }

    public void AddMark(MaskDrag mark)
    {
        currentCount++;
        UpdateUI();
        PhaseSelectManager.Instance.OnMarkAdded();
    }

    public void RemoveMark(MaskDrag mark)
    {
        currentCount--;
        UpdateUI();
        PhaseSelectManager.Instance.OnMarkRemoved();
    }

    public Vector3 GetSnapPosition()
    {
        // đơn giản: snap vào giữa box
        return transform.position;
    }

    void UpdateUI()
    {
        textCounter.SetText(currentCount + "/" + maxMask);
    }

    public void PlaceMarkInside(MaskDrag mask)
    {
        Bounds bB = GetWorldBounds();
        Bounds mB = mask.GetComponent<BoxCollider2D>().bounds;

        Vector3 offset = Vector3.zero;

        if (mB.min.x < bB.min.x) offset.x = bB.min.x - mB.min.x + 0.05f;
        else if (mB.max.x > bB.max.x) offset.x = bB.max.x - mB.max.x - 0.05f;

        if (mB.min.y < bB.min.y) offset.y = bB.min.y - mB.min.y + 0.05f;
        else if (mB.max.y > bB.max.y) offset.y = bB.max.y - mB.max.y - 0.05f;

        mask.transform.position += offset;
        mask.transform.SetParent(transform);
        mask.currentBox = this;
    }

    public Bounds GetWorldBounds()
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // corners: 0: BottomLeft, 2: TopRight
        Vector3 size = new Vector3(corners[2].x - corners[0].x, corners[2].y - corners[0].y, 0.1f);
        Vector3 center = corners[0] + size * 0.5f;

        return new Bounds(center, size);
    }
}
