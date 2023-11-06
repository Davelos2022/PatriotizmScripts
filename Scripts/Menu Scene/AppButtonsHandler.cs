using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VolumeBox.Toolbox;

public class AppButtonsHandler : MonoCached
{
    [SerializeField] private float animDuration;
    [SerializeField] private CanvasGroupFader canvasGroup;
    [SerializeField] private List<AppButton> buttons;

    private CancellationTokenSource _tokenSource;

    public void Hide()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        Rect.DOAnchorPosX(-700, animDuration).WithCancellation(_tokenSource.Token);
        canvasGroup.FadeOut();
    }

    public void Show()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        buttons.ForEach(b => b.UpdateVisuals());

        Rect.DOAnchorPosX(0, animDuration).WithCancellation(_tokenSource.Token);
        canvasGroup.FadeIn();
    }
}
