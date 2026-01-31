using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

public class DialogManager : Singleton<DialogManager>
{
    [Header("Data")]
    [SerializeField] private List<CharacterData> allCharacters;
    private Dictionary<string, CharacterData> characterCache = new();

    [Header("UI References")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterNameText; // Hiển thị tên nhân vật
    [SerializeField] private TextMeshProUGUI dialogText;

    [Header("Settings")]
    [SerializeField] private float textSpeed = 0.02f;

    private CharacterData currentCharacter;
    private Tween textTween;

    Coroutine typingRoutine;
    public float typingSpeed = 0.4f;

    private void Start()
    {
        // Cache dữ liệu để truy xuất O(1)
        foreach (var charData in allCharacters)
        {
            if (!characterCache.ContainsKey(charData.characterName))
                characterCache.Add(charData.characterName, charData);
        }
    }

    // 1. Log thoại: Tìm nhân vật theo tên, đổi biểu cảm và chạy chữ
    public void LogDialog(string charName, string expressionState, string message)
    {
        // Kiểm tra và cập nhật nhân vật hiện tại
        if (characterCache.TryGetValue(charName, out CharacterData data))
        {
            if (currentCharacter != data)
            {
                currentCharacter = data;
                characterNameText.text = currentCharacter.characterName;
                // Hiệu ứng Fade hoặc Punch khi đổi người nói
                characterImage.transform.DOPunchScale(Vector3.one * 0.05f, 0.2f);
            }

            // Đổi biểu cảm
            Sprite s = currentCharacter.GetSprite(expressionState);
            if (s != null) characterImage.sprite = s;
        }

        // 2. Hiệu ứng Typewriter
        textTween?.Kill();
        dialogText.text = "";
        // textTween = dialogText.DOText(message, message.Length * textSpeed)
        //                       .SetEase(Ease.Linear);
        ShowText(message);
    }

    // Hàm tiện ích để nạp thêm CharacterData lúc runtime nếu cần
    public void AddCharacter(CharacterData data)
    {
        if (!characterCache.ContainsKey(data.characterName))
            characterCache.Add(data.characterName, data);
    }

    public void ShowText(string content)
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeText(content));
    }

    IEnumerator TypeText(string content)
    {
        dialogText.text = "";
        //Debug.Log(content);

        foreach (char c in content)
        {
            dialogText.text += c;
            Debug.Log(c);

            //if (!char.IsWhiteSpace(c))
                //audioSource.PlayOneShot(typeSound);
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}