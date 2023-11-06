using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Selectable : MonoBehaviour, IPointerClickHandler
{
    private MultipleSelectionHandler _handler;

    public UnityEvent OnSelected;
    public UnityEvent OnDeselect;

    public void OnPointerClick(PointerEventData eventData)
    {
        _handler.HandleSelectableClick(this);
    }

    internal void Initialize(MultipleSelectionHandler multipleSelectionHandler)
    {
        _handler = multipleSelectionHandler;
    }

    public void Select()
    {
        OnSelected.Invoke();
    }

    public void Deselect()
    {
        OnDeselect.Invoke();
    }
}
