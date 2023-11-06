using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class PointPhotoPreviewButton: MonoCached, IPooled
{
    public UnityEvent<Texture2D> OnSpawnTextureEvent;

    private Texture2D _currentTexture;

    public void OnSpawn(object data)
    {
        _currentTexture = data as Texture2D;

        TextureSetAsync();
    }

    private async UniTask TextureSetAsync()
    {
        if (_currentTexture == null) return;

        await UniTask.Delay(10);

        OnSpawnTextureEvent.Invoke(_currentTexture);
    }

    public void OnClickHandler()
    {
        Messager.Instance.Send(new OpenImagePreviewByIndexMessage { index = transform.GetSiblingIndex() });
    }

    protected override void Rise()
    {

    }
}


