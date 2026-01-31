using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RessultManagerUI : Singleton<RessultManagerUI>
{
    [SerializeField] private EmotionalInterfact isStatus;
    public void Update_Score(int diemtang,Character data) {
        int _Score;
        int.TryParse(data._ScoreUI.text, out _Score);
        int total = _Score + diemtang;
        data._ScoreUI.text = (total).ToString();
        data._Score = total;
        if (_Score < total)
        {
            isStatus = EmotionalInterfact.Fun;
        }
        else
        {
            isStatus = EmotionalInterfact.Angry;
        }
    }
    public void Update_CoinPlayer(int CoinIncrease , Player data)
    {
        int _Coin;
        int.TryParse(data._CoinUI.text, out _Coin);
        int total = _Coin + CoinIncrease;
        data._CoinUI.text = (total).ToString();
        data._Coin = total;
        if (_Coin < total)
        {
            isStatus = EmotionalInterfact.Fun;
        }
        else
        {
            isStatus = EmotionalInterfact.Angry;
        }
    }
    public void Update_NumberMask(BoxMask data, int number)
    {
        int Number;
        int.TryParse(data._NumberMaskUI.text, out Number);
        int total = Number + number;
        data._NumberMaskUI.text = (total).ToString();
        data._NumberMask = total;
    }
    public void Update_ImageFace(Image character,Character data)
    {
        data._ImageFace.sprite = character.sprite;
    }
    private void Update()
    {
        Character.Instance.GetImageFace(isStatus);
    }

}
