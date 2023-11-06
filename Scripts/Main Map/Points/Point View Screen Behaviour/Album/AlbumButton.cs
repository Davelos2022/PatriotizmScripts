using Cysharp.Threading.Tasks;
using Spine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class AlbumButton: MonoCached, IPooled
{
    [SerializeField] private Animator animator;
    [SerializeField] private ToggleButton toggleButton;

    public UnityEvent<Texture2D> FirstImageLoaded;
    public UnityEvent<string> NameLoaded;

    private AlbumRelatedData _currentAlbumData;

    public AlbumData CurrentAlbum => _currentAlbumData.albumData;

    private bool lockClose = false;

    public void OnSpawn(object data)
    {
        if (data is not AlbumRelatedData) return;

        _currentAlbumData = (AlbumRelatedData)data;

        Messager.Instance.Subscribe<CloseAlbumsMessage>(_ => CloseAlbumView(), gameObject.scene.name);
        CloseAlbumView();
        SpawnAsync();
    }

    public void OpenAlbumView()
    {
        lockClose = true;
        toggleButton.SetStateSilently(true);
        Messager.Instance.Send<CloseAlbumsMessage>();
        animator.ResetTrigger("Close");
        animator.SetTrigger("Open");
        Messager.Instance.Send(new AlbumOpenMessage { album = _currentAlbumData.albumData });

        PushView();
    }

    private async UniTask PushView()
    {
        await UniTask.Delay(200);
        Messager.Instance.Send(new PushScrollMessage { obj = transform });
    }

    public void OnCloseClick()
    {
        Messager.Instance.Send(new AlbumCloseMessage { category = _currentAlbumData.albumData.relatedCategory });

        CloseAlbumView();
    }

    public void CloseAlbumView()
    {
        if(lockClose)
        {
            lockClose = false;
            return;
        }

        toggleButton.SetStateSilently(false);

        animator.ResetTrigger("Open");
        animator.SetTrigger("Close");
    }

    public void EditAlbum()
    {
        Messager.Instance.Send(new AlbumEditMessage { album = _currentAlbumData.albumData, relatedCategory = _currentAlbumData.categoryData });
    }

    public async UniTask SpawnAsync()
    {
        await _currentAlbumData.albumData.LoadCover();

        FirstImageLoaded.Invoke(_currentAlbumData.albumData.cover);
        NameLoaded.Invoke(_currentAlbumData.albumData.albumName);
    }

    protected override void OnDeactivate()
    {
        NameLoaded.Invoke(string.Empty);
        FirstImageLoaded.Invoke(null);
    }
}

public class CloseAlbumsMessage: Message { }
public class AlbumCloseMessage: Message 
{
    public retere category;
}
