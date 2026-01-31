using System.Collections;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class EnemyTypingDialog : MonoBehaviour
{
    public TMP_Text dialogText;
    public float typingSpeed = 0.4f;

    Coroutine typingRoutine;

    void Start()
    {
        ShowText("Hộp bên trái là của tao, ha ha ha :))");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Skip("Hộp bên trái là của tao, ha ha ha :))");
        }
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
    public void Skip(string content)
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        dialogText.text = content;
    }
}
