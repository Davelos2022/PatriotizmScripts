using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class ImagePreviewPoint: MonoCached, IPooled
{
    [SerializeField] private bool useSelfIndexInHierarchy;

    private OpenImagePreviewByIndexMessage msg;

    public void OnSpawn(object data)
    {
        msg = new OpenImagePreviewByIndexMessage { index = useSelfIndexInHierarchy ? transform.GetSiblingIndex() : (int)data };
    }

    public void OnClick()
    {
        Messager.Instance.Send(msg);
    }
}
