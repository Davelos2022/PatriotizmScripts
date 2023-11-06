using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using VolumeBox.Toolbox;

public class LogoScreen : MonoCached
{
    [SerializeField] private float stayDuration;
    [SerializeField] private float fadeOutDuration;
    [SerializeField] private float fadeInDuration;
    [SerializeField, Scene] private string nextSceneName;
    [SerializeField, Scene] private string menuSceneName;
    [SerializeField] private SceneArgs args;

    protected override void Rise()
    {
        LogoDelay();
    }

    private async void LogoDelay()
    {
        await Fader.In(0);
        await Fader.Out(fadeOutDuration);
        await GameManager.Instance.InitializeData();
        await Task.Delay(TimeSpan.FromSeconds(stayDuration));
        await Fader.In(fadeInDuration);
        await Traveler.LoadScene(nextSceneName, args);
        await Traveler.LoadScene(menuSceneName, args);
        await Traveler.UnloadScene(gameObject.scene.name);
    }
}
