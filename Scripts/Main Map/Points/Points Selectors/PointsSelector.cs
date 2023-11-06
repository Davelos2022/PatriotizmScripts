using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public abstract class PointsSelector: MonoCached
{
    public abstract List<MapPoint> SelectPoints(List<MapPoint> points);
}
