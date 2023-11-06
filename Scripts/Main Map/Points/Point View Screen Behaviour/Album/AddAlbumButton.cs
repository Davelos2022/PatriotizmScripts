using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class AddAlbumButton : MonoCached, IPooled
{
    private AlbumRelatedData _currentData;

    public void OnSpawn(object data)
    {
        if (data is not AlbumRelatedData) return;

        _currentData = (AlbumRelatedData)data;
    }

    public void OnClick()
    {
        Messager.Instance.Send(new CreateAlbumMessage { RelatedCategory = _currentData.categoryData, RelatedPoint = _currentData.mapPointData });
    }
}
