using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectTransformUndoRecorder: UndoRecorder
{
    private RectTransformInfo _initialInfo;

    protected override void Initialize()
    {
        _initialInfo = ConstructInfo();
    }

    protected override void ClearHandle()
    {
        RestoreStateHandle(_initialInfo);
    }

    private RectTransformInfo ConstructInfo()
    {
        var pos = transform.position;
        var rot = transform.rotation;
        var s = (transform as RectTransform).sizeDelta;

        return new RectTransformInfo { position = pos, rotation = rot, size = s };
    }

    protected override object RecordHandle()
    {
        return ConstructInfo();
    }

    protected override void RestoreStateHandle(object state)
    {
        RectTransformInfo info = (RectTransformInfo)state;

        RectTransform rect = transform as RectTransform;

        transform.position = info.position;
        transform.rotation = info.rotation;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, info.size.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, info.size.y);
    }
}

public class RectTransformInfo
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector2 size;
}
