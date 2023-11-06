using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class LoaderScreen : Singleton<LoaderScreen>
{
    [SerializeField] private float showDuration;
    [SerializeField] private float hideDuration;
    [SerializeField] private CanvasGroup canvasGroup;

    public UnityEvent ShowEvent;
    public UnityEvent HideEvent;
    [Space]
    public UnityEvent ProgressBarEnabled;
    public UnityEvent ProgressBarDisabled;
    public UnityEvent<float> ProgressValueChanged;

    private static bool _visible;

    [Button("Show")]
    public void Show()
    {
        ShowAsync();
    }

    public static async UniTask ShowAsync()
    {
        if(_visible) return;

        _visible = true;
        Instance.ShowEvent.Invoke();
        Instance.canvasGroup.DOKill();
        Instance.canvasGroup.SetInteractions(true);
        await Instance.canvasGroup.DOFade(1, Instance.showDuration);
    }
    
    public static async UniTask ShowAsync(bool ignoreVisible)
    {
        Instance.ShowEvent.Invoke();
        Instance.canvasGroup.DOKill();
        Instance.canvasGroup.SetInteractions(true);
        await Instance.canvasGroup.DOFade(0, 0f);
    }

    public static void EnableProgressBar()
    {
        Instance.ProgressBarEnabled.Invoke();
    }

    public static void SetProgressBarValue(float value)
    {
        Instance.ProgressValueChanged.Invoke(value);
    }

    public static void DisableProgressbar()
    {
        Instance.ProgressBarDisabled.Invoke();
    }

    [Button("Hide")]
    public void Hide()
    {
        HideAsync();
    }

    public static async UniTask HideAsync()
    {
        if (!_visible) return;

        _visible = false;
        Instance.HideEvent.Invoke();
        Instance.canvasGroup.DOKill();
        await Instance.canvasGroup.DOFade(0, Instance.hideDuration);
        Instance.canvasGroup.SetInteractions(false);
    }
    
    public static async UniTask HideAsync(bool ignoreVisible)
    {
        _visible = false;
        Instance.HideEvent.Invoke();
        Instance.canvasGroup.DOKill();
        await Instance.canvasGroup.DOFade(0, 0f);
        Instance.canvasGroup.SetInteractions(false);
    }
}
