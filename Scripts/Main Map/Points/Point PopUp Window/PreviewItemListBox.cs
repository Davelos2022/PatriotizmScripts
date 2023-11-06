using AirFishLab.ScrollingList;
using AirFishLab.ScrollingList.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviewItemListBox : ListBox
{
    [SerializeField] private RawImage rawImg;
    [SerializeField] private RawImageAspectPreserver aspectFitter;

    public Texture2D Img { get; private set; }

    public override void UpdateDisplayContent(IListContent content)
    {
        var item = (PreviewData)content;

        Img = item.previewTexture;

        rawImg.texture = Img;
        aspectFitter.SetAspect();
    }
}
