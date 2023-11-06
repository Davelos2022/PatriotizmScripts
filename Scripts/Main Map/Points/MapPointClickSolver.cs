using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using VolumeBox.Toolbox;

public class MapPointClickSolver : MonoCached
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform mapRect;
    [SerializeField] private float distanceToClick;
    [SerializeField] private MapPointsContainer container;

    public void TryClickPoint(BaseEventData eventData)
    {
        TryClickPoint(eventData as PointerEventData);
    }

    public bool TryClickPoint(PointerEventData eventData)
    {
        if (container.DraggingPoints) return false;

        var pos = eventData.position / canvas.scaleFactor;
        MapPoint closestPoint = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < container.DrawedPoints.Count; i++)
        {
            if (!container.DrawedPoints[i].Visible)
            {
                continue;
            }

            float currentDistance = Vector3.Distance(container.DrawedPoints[i].Rect.anchoredPosition, pos);

            if (currentDistance < minDistance && currentDistance <= distanceToClick)
            {
                minDistance = currentDistance;
                closestPoint = container.DrawedPoints[i];
            }
        }

        if (closestPoint != null)
        {
            closestPoint.OnClick();
            return true;
        }
        else
        {
            return false;
        }
    }
}
