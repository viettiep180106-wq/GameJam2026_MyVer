using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private CardData cardData;
    [SerializeField] private bool isFlipped = false;
    [SerializeField] private Image cardFront;
    [SerializeField] private Image cardBack;

    public CardData CardData => cardData;

    public void Init(CardData cardData)
    {
        this.cardData = cardData;
    }

    public void OnClickFlip()
    {
        float duration = 0.5f;
        float targetY = isFlipped ? 0f : 180f;

        // Tạo chuỗi hành động
        Sequence flipSequence = DOTween.Sequence();

        // 1. Xoay thẻ
        flipSequence.Append(transform.DORotate(new Vector3(0, targetY, 0), duration)
                    .SetEase(Ease.OutQuad));

        // 2. Chèn logic swap image tại thời điểm 50% thời gian (giữa quá trình xoay)
        flipSequence.InsertCallback(duration / 2f, () =>
        {
            // Khi xoay đến 90 độ, mặt thẻ vuông góc với camera nên việc swap sẽ mượt nhất
            if (targetY == 180f) // Đang lật ra sau
            {
                cardFront.gameObject.SetActive(false);
                cardBack.gameObject.SetActive(true);
            }
            else // Đang lật về trước
            {
                cardFront.gameObject.SetActive(true);
                cardBack.gameObject.SetActive(false);
            }
        });

        isFlipped = !isFlipped;
    }
}