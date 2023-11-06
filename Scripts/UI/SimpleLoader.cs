using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLoader: Loader
{
    [SerializeField] private Transform rotatingRoot;

    private Tween _tween;

    protected override void Rise()
    {
        _tween = rotatingRoot.DOLocalRotate(new Vector3(0, 0, 359), 1, RotateMode.FastBeyond360).SetAutoKill(false).SetLoops(-1);
    }

    public override void StartLoading()
    {
        _tween.Restart();
    }

    public override void StopLoading()
    {
        _tween.Pause();
    }
}
