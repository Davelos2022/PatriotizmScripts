using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class DrawingBehaviour : MonoCached
{
    [SerializeField] private CanvasGroupFader buttonsFader;
    [SerializeField] private ScreenshotBehaviour screenshot;
    public UnityEvent DrawingStarted;
    public UnityEvent DrawingFinished;

    protected override void Rise()
    {
        UpdateState();
        Messager.Instance.Subscribe<SettingsChangedMessage>(_ => UpdateState(), gameObject.scene.name);
        Messager.Instance.Subscribe<AdminModeChangedMessage>(_ => UpdateState(), gameObject.scene.name);
    }

    public void UpdateState()
    {
        gameObject.SetActive(GameManager.Instance.IsAdminMode || GameManager.Instance.Settings.drawingEnabled);
    }

    public void ScreenshotCallback()
    {
        ScreenshotAsync();
    }

    private async UniTask ScreenshotAsync()
    {
        await buttonsFader.FadeOutAsync();
        await screenshot.TakeScreenshotAsync(GameManager.Instance.Settings.drawingsPath, "Рисунок успешно сохранен", "Не удалось сохранить рисунок");
        await buttonsFader.FadeInAsync();
    }

    public void OnStartDrawing()
    {
        Messager.Instance.Send<CloseAppMenuMessage>();
        Messager.Instance.Send<HideMenuMessage>();

        DrawingStarted.Invoke();
    }

    public void OnFinishDrawing()
    {
        Messager.Instance.Send<ShowMenuMessage>();
        Messager.Instance.Send<OpenAppMenuMessage>();

        DrawingFinished.Invoke();
    }
}
