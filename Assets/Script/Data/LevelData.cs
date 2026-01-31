using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevel", menuName = "ScriptableObjects/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Selection")]
    public List<MaskEntry> maskEntries;
    public int maxMaskBox1;
    public int maxMaskBox2;

    [Header("Library")]
    public List<GemPrefabMapping> prefabLibrary;

    [Header("Level Layout")]
    public List<LevelEntry> entries;

    public GameObject GetPrefab(GemType type)
    {
        var mapping = prefabLibrary.Find(m => m.gemType == type);
        return mapping?.prefab;
    }
}