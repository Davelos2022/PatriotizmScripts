using AirFishLab.ScrollingList;
using AirFishLab.ScrollingList.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PreviewListBank : ListBank
{
    [SerializeField] private PreviewData[] previewItems;

    public void SetListContent(PreviewData[] items)
    {
        previewItems = items.ToArray();
    }

    public override IListContent GetListContent(int index)
    {
        return previewItems[index];
    }

    public override int GetContentCount()
    {
        return previewItems.Length;
    }
}

[Serializable]
public class PreviewData: IListContent
{
    public Texture2D previewTexture;
}
