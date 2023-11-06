using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class PointViewScreen: MonoCached
{
    [SerializeField] private PointCategoriesView categoriesView;
    [SerializeField] private ScrollableLayoutHandler scrollableLayout;

    private MapPointData _currentViewPoint;

    public UnityEvent ShowEvent;
    public UnityEvent CloseEvent;

    public void OpenPoint(MapPointData data)
    {
        _currentViewPoint = data;

        ShowEvent.Invoke();
    }

    public void ClosePanel()
    {
        CloseEvent.Invoke();
    }
}
