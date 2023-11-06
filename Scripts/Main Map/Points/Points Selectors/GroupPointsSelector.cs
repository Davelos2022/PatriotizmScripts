using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GroupPointsSelector: PointsSelector, IIndexBasedHandler
{
    private Group _currentFilter;

    public void SetGroup(Group filter)
    {
        _currentFilter = filter;
    }

    public override List<MapPoint> SelectPoints(List<MapPoint> points)
    {
        if (_currentFilter == Group.None)
        {
            return points.Where(p => p.Data.showByDefault).ToList();
        }
        else
        {
            return points.Where(x => x.Data.group == _currentFilter).ToList();
        }

    }

    public void HandleIndex(int index)
    {
        Debug.Log(index);

        if(index < 0)
        {
            SetGroup(Group.None);
            return;
        }

        SetGroup((Group)(index + 1));
    }
}
