using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class ErrorHighlighter : MonoCached
{
    [SerializeField] private Graphic visual;
    [SerializeField] private Color highlightColor = Color.red;
    [SerializeField] private Color transparentColor = new Color(1, 0, 0, 0);

    private CancellationTokenSource _tokenSource;

    public void Highlight()
    {
        HighlightAsync();
    }

    private async UniTask HighlightAsync()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        await visual.DOColor(highlightColor, 0.5f).WithCancellation(_tokenSource.Token);
        await UniTask.Delay(500).AttachExternalCancellation(_tokenSource.Token);
        await visual.DOColor(transparentColor, 1f).WithCancellation(_tokenSource.Token);
    }
}
