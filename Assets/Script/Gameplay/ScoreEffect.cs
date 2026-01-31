using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScoreEffect : MonoBehaviour
{
    public TMP_Text scoreText;

    public float countDuration = 0.3f;
    public float punchScale = 0.25f;
    public float punchDuration = 0.15f;

    private Tween countTween;

    //void Awake()
    //{
        //currentScore = 0;
        //scoreText.text = "0";
    //}

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }

    public void AddScore(int startScore, int targetScore)
    {
        // Kill animation cũ
        countTween?.Kill();
        scoreText.transform.DOKill();

        // 1️ Count up số
        countTween = DOTween.To(
            () => startScore,
            x => scoreText.text = x.ToString(),
            targetScore,
            countDuration
        ).SetEase(Ease.OutQuad);

        // 2️ Phồng + lắc kiểu Balatro
        scoreText.transform
            .DOPunchScale(Vector3.one * punchScale, punchDuration, 8, 0.8f);

        // 3️ Nhẹ rung thêm cho "đã tay"
        scoreText.transform
            .DOShakeRotation(0.12f, new Vector3(0, 0, 10));
    }
}