using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class PointAlbumPreviewButton : MonoCached, IPooled
{
    [SerializeField] private TMP_Text nameText;

    public UnityEvent<Texture2D> SpawnedWithTextureEvent;
    
    private AlbumData _currentAlbum;

    public void OnSpawn(object data)
    {
        _currentAlbum = data as AlbumData;

        if(_currentAlbum != null)
        {
            SpawnedWithTextureEvent.Invoke(_currentAlbum.photos[Random.Range(0, _currentAlbum.photos.Count)]);
            nameText.text = _currentAlbum.albumName;
        }
    }

    public void OpenAlbum()
    {
        if (_currentAlbum == null) return;

        Messager.Instance.Send(new AlbumOpenMessage { album = _currentAlbum });
    }

    public void EditAlbum()
    {
        Messager.Instance.Send(new AlbumEditMessage { album = _currentAlbum });
    }
}
