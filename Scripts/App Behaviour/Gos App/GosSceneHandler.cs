using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VolumeBox.Toolbox;

public class GosSceneHandler: SceneHandler<GosSceneArgs>
{
    [SerializeField] private GameObject backButton;

    protected override void SetupScene(GosSceneArgs args)
    {
        GameManager.Instance.SetAppState(AppState.GOS);
        AudioPlayer.Instance.StopMusic();
        AudioPlayer.Instance.PlayMusic("gosMusic", 0.15f, true);
    }
    
    private void OnDisable()
    {
        UnloadScene("GosudarstMainScene");
        UnloadScene("GosudarstMenu");
    }
    
    private void UnloadScene(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded) {
            SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}
