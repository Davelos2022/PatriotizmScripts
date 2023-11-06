using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class ImageAlbumPreview : MonoCached
{
    [SerializeField] private string imagePreviewButtonTag;
    [SerializeField] private Transform albumRoot;
    [SerializeField] private ImagePreviewer previewer;

    [Inject] private Pooler _pool;

    private List<AlbumData> _currentAlbums;
    private AlbumData _currentOpenedAlbum;

    public List<AlbumData> CurrentAlbums => _currentAlbums;

    public ImagePreviewer Previewer => previewer;

    public void ShowAlbums(List<AlbumData> albums)
    {
        _currentAlbums = albums;

        RefreshAlbum();
        OpenAlbumByIndex(0);
    }

    public void Clear()
    {
        foreach(Transform child in albumRoot)
        {
            _pool.DespawnOrDestroy(child.gameObject);
        }
    }

    public void RefreshAlbum()
    {
        Clear();

        _currentAlbums.ForEach(i => _pool.Spawn(imagePreviewButtonTag, Vector3.zero, Quaternion.identity, albumRoot, this, x => x.transform.localScale = Vector3.one));
    }

    public void OpenAlbumByIndex(int index)
    {
        if(index < 0 || index >= _currentAlbums.Count)
        {
            return;
        }

        _currentOpenedAlbum = _currentAlbums[index];

        previewer.SetList(_currentOpenedAlbum.photos);
    }

}
