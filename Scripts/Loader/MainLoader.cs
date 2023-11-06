using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class MainLoader : MonoCached
{
    [SerializeField] private RectTransform point;

    protected override void Rise()
    {
        point.DORotate(Vector3.forward * -359, 2, RotateMode.LocalAxisAdd).SetAutoKill(false).SetLoops(-1).SetEase(Ease.Linear);
    }

    private void OnDestroy()
    {
        point.DOKill();
    }
}
