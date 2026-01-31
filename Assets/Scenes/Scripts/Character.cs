using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Character : Singleton<Character>
{
    public int _Score;
    public TextMeshProUGUI _ScoreUI;
    public Image _ImageFace;
    [SerializeField] private List<Image> _ImagesPrefab = new List<Image>();
    public void GetImageFace(EmotionalInterfact type)
    {
        for (int i = 0; i < _ImagesPrefab.Count; i++)
        {
            if (i == (int)type)
            {
                RessultManagerUI.Instance.Update_ImageFace(_ImagesPrefab[i], this);
            }
        }
    }
}
