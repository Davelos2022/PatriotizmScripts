using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VolumeBox.Toolbox;

public class InOutTweener : MonoCached
{
    [SerializeField] private InitialBehaviour initialBehaviour;
    [SerializeField] private Vector2 inPosition;
    [SerializeField] private Vector2 outPosition;
    [SerializeField] private float duration;

    private CancellationTokenSource _tokenSource;

    protected override void Rise()
    {
        switch (initialBehaviour)
        {
            case InitialBehaviour.None:
                break;
            case InitialBehaviour.Out:
                Out();
                break;
            case InitialBehaviour.In:
                In();
                break;
            default:
                break;
        }
    }

    public void In()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        Rect.DOAnchorPos(inPosition, duration).WithCancellation(_tokenSource.Token);
    }

    public void Out()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        Rect.DOAnchorPos(outPosition, duration).WithCancellation(_tokenSource.Token);
    }

    private enum InitialBehaviour
    {
        None,
        Out,
        In,
    }
}
