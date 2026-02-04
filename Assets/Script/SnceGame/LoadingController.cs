using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class LoadingController : MonoBehaviour
{
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TextMeshProUGUI loadingText;
    private void Start()
    {
        string TextScene = SceneFlowManager.Instance.CheckScene().ToString();
        StartCoroutine(LoadGameScene(TextScene));
    }
    IEnumerator LoadGameScene(string text)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(text);
        operation.allowSceneActivation = false;
        float timefake = 3f;
        float timer = 0f;
        while (!operation.isDone)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeSlow = Mathf.Clamp01(timer / timefake);
            loadingSlider.value = Mathf.Min(progress,timeSlow);
            if (loadingText != null)
                loadingText.text = (Mathf.Min(progress,timeSlow) * 100).ToString("F0") + "%";

            // Khi load xong 90% thì cho chuyển scene
            if (operation.progress >= 0.9f && timeSlow >= 1f)
            {
                Debug.Log("Ok");
                yield return new WaitForSeconds(0.03f); // delay nhẹ cho mượt
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
