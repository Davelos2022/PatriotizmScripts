using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using VolumeBox.Toolbox;

public class ClickDragResolver: MonoCached, IPointerClickHandler, IBeginDragHandler, IEndDragHandler
{
    public UnityEvent Clicked;

    private bool _dragging = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _dragging = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!_dragging)
        {
            Clicked.Invoke();
        }
    }
}
