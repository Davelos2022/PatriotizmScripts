using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using VolumeBox.Toolbox;

public class SelectableResizer: MonoCached, ISelectHandler, IDeselectHandler
{
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private Vector2 onSelectSize;
    [SerializeField] private Vector2 onDeselectSize;
    [SerializeField] private float resizeDuration;
    [SerializeField] private bool disableX;

    private CancellationTokenSource _tokenSource;

    public void OnDeselect(BaseEventData eventData)
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        targetRect.DOSizeDelta(onDeselectSize, resizeDuration).WithCancellation(_tokenSource.Token);
    }

    public void OnSelect(BaseEventData eventData)
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        targetRect.DOSizeDelta(onSelectSize, resizeDuration).WithCancellation(_tokenSource.Token);
    }
}
