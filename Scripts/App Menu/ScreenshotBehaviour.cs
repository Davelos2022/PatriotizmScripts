using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using VolumeBox.Toolbox;

public class ScreenshotBehaviour : MonoCached
{
    [SerializeField, Range(1, 50)] private int superSize = 1;

    private EventSystem _evt;

    protected override void Rise()
    {
        GetEventSystem();
        UpdateState();
        Messager.Instance.Subscribe<SettingsChangedMessage>(_ => UpdateState(), gameObject.scene.name);
        Messager.Instance.Subscribe<AdminModeChangedMessage>(_ => UpdateState(), gameObject.scene.name);
    }

    public void UpdateState()
    {
        gameObject.SetActive(GameManager.Instance.IsAdminMode || GameManager.Instance.Settings.screenshotEnabled);
    }

    private void GetEventSystem()
    {
        if(_evt == null)
        {
            _evt = EventSystem.current;
        }
    }

    public void TakeScreenshot()
    {
        TakeScreenshotAsync(GameManager.Instance.Settings.screenshotsPath, "Снимок экрана успешно сохранён", "Не удалось сохранить снимок экрана");
    }

    public async UniTask TakeScreenshotAsync(string path, string message, string failMessage)
    {
        GetEventSystem();

        _evt.enabled = false;
        await Traveler.TryGetSceneHandler<MenuSceneHandler>().Menu.CloseMenuAsync();

        try
        {
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string date = System.DateTime.Now.ToString();
            date = date.Replace("/", "-");
            date = date.Replace(" ", "_");
            date = date.Replace(":", "-");

            var finalPath = $"{path}\\Screenshot-{date}.png";

            ScreenCapture.CaptureScreenshot(finalPath, superSize);

            Notifier.Instance.Notify(NotifyType.Success, message, finalPath.ToLower(), false);
            
            AudioPlayer.Instance.PlaySound("screenshot");
        }
        catch
        {
            Notifier.Instance.Notify(NotifyType.Error, failMessage);
        }

        _evt.enabled = true;

    }
}
