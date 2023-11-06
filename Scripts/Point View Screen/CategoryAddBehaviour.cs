using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class CategoryAddBehaviour: MonoCached, IPooled
{
    private MapPointData _currentPoint;

    public void OnSpawn(object data)
    {
        _currentPoint = (MapPointData)data;
    }

    public void OnClickCallback()
    {
        Messager.Instance.Send(new CreateCategoryMessage { relatedPoint = _currentPoint });
    }
}
