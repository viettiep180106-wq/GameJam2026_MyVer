using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuController : Singleton<MenuController>
{
    [SerializeField] private Button play;
    private void Awake()
    {
        play = GetComponentInChildren<Button>();
    }
    private void Start()
    {
        play.onClick.AddListener(PlayGame);
    }
    public void PlayGame()
    {
        SceneFlowManager.Instance.LoadScene(SceneState.SampleScene);
    }
}
