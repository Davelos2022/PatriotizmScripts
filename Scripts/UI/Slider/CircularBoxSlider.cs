using AirFishLab.ScrollingList;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class CircularBoxSlider : MonoBehaviour
{
    [SerializeField] private string imagePreviewPointTag;
    [SerializeField] private Transform pointsRoot;
    [SerializeField] private PreviewListBank listBank;
    [SerializeField] private Slider slider;

    public void UpdateContent()
    {
        while (pointsRoot.childCount > 1)
        {
            Pooler.Instance.DespawnOrDestroy(pointsRoot.GetChild(1).gameObject);
        }

        if(listBank.GetContentCount() > 1)
        {
            for (int i = 0; i < listBank.GetContentCount(); i++)
            {
                var p = Pooler.Instance.Spawn(imagePreviewPointTag, Vector3.zero, Quaternion.identity, pointsRoot, i, g => g.transform.localScale = Vector3.one * 0.2f);
                var pointP = p.transform.localPosition;
                pointP.z = 0;
                p.transform.localPosition = pointP;
                p.transform.SetAsLastSibling();
                p.transform.localScale = Vector3.one * 0.2f;
            }
        }
    }

    public void ItemChangedCallback(ListBox box1, ListBox box2)
    {
        slider.value = (float)box2.ContentID / (float)listBank.GetContentCount();
    }
}
