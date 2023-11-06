using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleSelectionHandler : MonoBehaviour
{
    private List<Selectable> _selection = new List<Selectable>();

    public List<Selectable> Selection => _selection;

    public void HandleSelectableClick(Selectable selectable)
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            _selection.Add(selectable);
        }
        else
        {
            foreach(var sel in _selection)
            {
                sel.Deselect();
            }

            _selection.Clear();
            _selection.Add(selectable);
        }

        selectable.Select();
    }

    private void Awake()
    {
        foreach(Transform child in transform)
        {
            if(child.TryGetComponent(out Selectable sel))
            {
                sel.Initialize(this);
            }
        }
    }
}
