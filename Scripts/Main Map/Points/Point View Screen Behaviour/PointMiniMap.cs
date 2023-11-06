using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class PointMiniMap : MonoCached
{
    [SerializeField] private Vector2 referenceResolution;
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private RectTransform point;

    public void UpdatePoint(MapPointData data)
    {
        var pos = new Vector2(data.anchoredPositionX, data.anchoredPositionY);

        var aspect = targetRect.rect.width / referenceResolution.x;

        pos *= aspect;

        point.anchoredPosition = pos;
    }
}
