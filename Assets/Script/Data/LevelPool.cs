using System;
using System.Collections.Generic;

[Serializable]
public class LevelPool
{
    public int difficulty; // 1, 2, 3
    public List<LevelData> levels;
}