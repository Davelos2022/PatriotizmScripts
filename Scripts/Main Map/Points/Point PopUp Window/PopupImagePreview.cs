using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class PopupImagePreview : MonoCached, IPooled
{
    public UnityEvent<Texture2D> ImageFetched;

    public void OnSpawn(object data)
    {
        ImageFetched.Invoke(data as Texture2D);
    }
}
