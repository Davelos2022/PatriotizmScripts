using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class ImagePreviewSlide: MonoCached, IPooled
{
    [SerializeField] private RawImage image;
    [SerializeField] private RawImageAspectPreserver aspectPreserver;
    [SerializeField] private Texture transparentTexture;

    public void OnSpawn(object data)
    {
        image.texture = transparentTexture;

        if (data is not Texture2D || data is null) return;

        image.texture = (Texture2D)data;

        if(image.texture == null)
        {
            image.color = new Color(1, 1, 1, 0);
        }
        else
        {
            image.color = Color.white;
        }

        SpawnAsync();
    }

    private async UniTask SpawnAsync()
    {
        await UniTask.Delay(10);
        aspectPreserver.SetAspect();
    }
}
