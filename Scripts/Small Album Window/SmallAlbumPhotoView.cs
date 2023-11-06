using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VolumeBox.Toolbox;
using VolumeBox.Toolbox.UIInformer;

public class SmallAlbumPhotoView: MonoCached, IPooled
{
    [SerializeField] private Toggle toggle;

    public UnityEvent<Texture2D> OnTextureSet;

    private SmallAlbum _album;

    public void OnSpawn(object data)
    {
        var d = (AlbumPhotoViewData)data;

        _album = d.album;

        OnTextureSet.Invoke(d.texture);

        if(d.toggleGroup != null)
        {
            toggle.group = d.toggleGroup;
        }
    }

    public void DeleteHandle()
    {
        Info.Instance.ShowBox("Вы действительно хотите удалить фото?", Delete, null, "Подтвердите действие", "Отмена");
    }

    private void Delete()
    {
        _album.RemoveByIndex(transform.GetSiblingIndex());
    }
}
