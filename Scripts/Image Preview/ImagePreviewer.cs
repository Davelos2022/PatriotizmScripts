using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using VolumeBox.Toolbox;

public class ImagePreviewer : MonoCached
{
    [SerializeField] private string imagePreviewSlideTag;
    [SerializeField] private RectTransform slidesRoot;
    [SerializeField] private ScrollSnap scroll;
    [SerializeField] private string imagePreviewPointTag;
    [SerializeField] private Transform pointsRoot;

    private int _currentSelectedIndex;
    private List<Texture2D> _imageList;

    public Texture2D CurrentSelectedTexture => _imageList[_currentSelectedIndex];
    public int CurrentSelectedIndex => _currentSelectedIndex;
    public List<Texture2D> ImageList => _imageList;

    public UnityEvent<Texture2D> TextureChangedEvent;
    public UnityEvent ListChangedEvent;

    protected override void Rise()
    {
        _currentSelectedIndex = 0;
        Messager.Instance.Subscribe<ImagePreviewListSetMessage>(m => SetList(m.texturesList), gameObject.scene.name);
        Messager.Instance.Subscribe<OpenImagePreviewByIndexMessage>(m => scroll.ChangePage(m.index), gameObject.scene.name);
    }

    public void SetList(List<Texture2D> list)
    {
        Clear();

        for (int i = 0; i < list.Count; i++)
        {
            var slide = Pooler.Instance.Spawn(imagePreviewSlideTag, Vector3.zero, Quaternion.identity, slidesRoot, list[i], g => g.transform.localScale = Vector3.one);
            var slideP = slide.transform.localPosition;
            slideP.z = 0;
            slide.transform.localPosition = slideP;

            var p = Pooler.Instance.Spawn(imagePreviewPointTag, Vector3.zero, Quaternion.identity, pointsRoot, i, g => g.transform.localScale = Vector3.one);
            p.transform.SetAsLastSibling();
            p.transform.localScale = Vector3.one;
            var pointP = p.transform.localPosition;
            pointP.z = 0;
            p.transform.localPosition = pointP;
        }

        SetListAsync();
    }

    private async UniTask SetListAsync()
    {
        await UniTask.Delay(100);
        scroll.ChangePage(0);
        LayoutRebuilder.ForceRebuildLayoutImmediate(slidesRoot);
    }

    public void Clear()
    {
        while(slidesRoot.childCount > 0)
        {
            Pooler.Instance.DespawnOrDestroy(slidesRoot.GetChild(0).gameObject);
        }

        while(pointsRoot.childCount > 1)
        {
            Pooler.Instance.DespawnOrDestroy(pointsRoot.GetChild(1).gameObject);
        }
    }
}

[Serializable]
public class OpenImagePreviewByIndexMessage: Message
{
    public int index;
}

[Serializable]
public class ImagePreviewListSetMessage: Message
{
    public List<Texture2D> texturesList;
}


