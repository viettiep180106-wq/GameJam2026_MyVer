using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum SceneState
{
    PreGameplayScene,
    SampleScene,
    GamePlay
}
public class SceneFlowManager : Singleton<SceneFlowManager>
{
    public bool IsRestart;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        IsRestart = false;
    }
    public void LoadScene(SceneState scene)
    {
        AudioManager.Instance.Play(GameSound.clickButton);
        SceneManager.LoadSceneAsync(scene.ToString());
    }
    public SceneState CheckScene()
    {
        if (IsRestart == false)
        {
            return SceneState.GamePlay;
        }
        else 
        {
            return SceneState.PreGameplayScene;
        } 
    }
}

