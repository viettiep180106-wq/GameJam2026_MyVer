using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Dialog/Character")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public List<Expression> expressions;

    [System.Serializable]
    public struct Expression
    {
        public string stateName; // ví dụ: "Vui", "Buon", "Cuoi"
        public Sprite sprite;
    }

    public Sprite GetSprite(string state)
    {
        return expressions.Find(x => x.stateName == state).sprite;
    }
}