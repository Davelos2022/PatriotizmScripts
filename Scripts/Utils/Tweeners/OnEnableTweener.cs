using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class OnEnableTweener : MonoCached
{
    [SerializeField] private Vector2 startPosition;
    [SerializeField] private Vector2 endPosition;
    [SerializeField] private float duration;

    protected override void OnActivate()
    {
        Rect.anchoredPosition = startPosition;
        Rect.DOAnchorPos(endPosition, duration);
    }

    protected override void OnDeactivate()
    {
        Rect.DOKill();
    }
}
