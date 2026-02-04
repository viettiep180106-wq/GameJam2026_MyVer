
using UnityEngine;
using UnityEngine.UI;

public class RestartGame : MonoBehaviour
{
    [SerializeField] private Button Restart;
    [SerializeField] private bool showed = false;
    [SerializeField] private Canvas UI;
    private void Start()
    {
        UI = GetComponentInChildren<Canvas>(true);
        Restart = GetComponentInChildren<Button>(true);
        Restart.onClick.AddListener(RestartScene);
        UI.enabled = false;
    }
    public void RestartScene()
    {
        SceneFlowManager.Instance.LoadScene(SceneState.PreGameplayScene);
    }
    private void Update()
    {
        if (SceneFlowManager.Instance.IsRestart && showed == false)
        {
            UI.enabled = true;showed = true;
        }
    }


}
