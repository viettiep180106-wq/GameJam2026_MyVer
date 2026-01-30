using UnityEngine;

public static class Utilities
{
    public static Vector3 GetMouseWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = 0f;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}
