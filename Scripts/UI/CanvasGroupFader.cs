using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VolumeBox.Toolbox;

public class CanvasGroupFader: MonoCached
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField, MinValue(0)] private float fadeInDelay = 0;
    [SerializeField, MinValue(0)] private float fadeInDuration = 0.2f;
    [SerializeField, MinValue(0)] private float fadeOutDelay = 0;
    [SerializeField, MinValue(0)] private float fadeOutDuration = 0.2f;
    [SerializeField] private bool controlInteractions = true;

    private CancellationTokenSource _tokenSource;

    public float FadeInDuration => fadeInDuration;
    public float FadeOutDuration => fadeOutDuration;

    public void FadeOut()
    {
        FadeOutAsync();
    }

    public void FadeIn()
    {
        FadeInAsync();
    }

    public async UniTask FadeOutAsync()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        if(_tokenSource.IsCancellationRequested)
        {
            return;
        }

        if(canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null) return;


        if(fadeOutDuration > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(fadeOutDelay));

            if (_tokenSource.IsCancellationRequested) return;
        }

        canvasGroup.DOKill();
        await canvasGroup.DOFade(0, fadeOutDuration).WithCancellation(_tokenSource.Token);

        if(_tokenSource.IsCancellationRequested) return;

        if (controlInteractions)
        {
            canvasGroup.SetInteractions(false);
        }
    }

    public async UniTask FadeInAsync()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        if(_tokenSource.IsCancellationRequested)
        {
            return;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null) return;


        if (fadeInDuration > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(fadeInDelay));

            if (_tokenSource.IsCancellationRequested) return;
        }

        if (controlInteractions)
        {
            canvasGroup.SetInteractions(true);
        }

        canvasGroup.DOKill();
        await canvasGroup.DOFade(1, fadeInDuration).WithCancellation(_tokenSource.Token);
    }

    protected override void Destroyed()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
    }
}
