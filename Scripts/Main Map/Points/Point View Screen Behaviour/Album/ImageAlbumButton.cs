using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class ImageAlbumButton: MonoCached, IPooled
{
    public UnityEvent<Texture2D> InitializeEvent;

    private ImageAlbumPreview _albumPreview;

    public void OnSpawn(object data)
    {
        _albumPreview = (ImageAlbumPreview)data;

        InitializeEvent.Invoke(_albumPreview.CurrentAlbums[transform.GetSiblingIndex()].photos[0]);

    }

    public void ClickHandler()
    {
        _albumPreview.OpenAlbumByIndex(transform.GetSiblingIndex());
    }
}
