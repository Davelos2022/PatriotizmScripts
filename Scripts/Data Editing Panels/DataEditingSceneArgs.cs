using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

[Serializable]
[CreateAssetMenu(menuName = "App/Scene Args/Data Editing Scene Args")]
public class DataEditingSceneArgs : SceneArgs
{
    public PointEditType editType;
    public MapPointsContainer container;
    public MapPointData pointData;
    public retere category;
    public AlbumData album;
    public bool isNew = false;
}

public enum PointEditType
{
    Point,
    Category,
    Album,
}
