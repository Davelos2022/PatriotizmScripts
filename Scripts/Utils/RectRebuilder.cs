using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class RectRebuilder : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private float rebuildDelay;

    [Button("Rebuild")]
    public void Rebuild()
    {
        RebuildAsync();
    }

    public async UniTask RebuildAsync()
    {
        if (rect == null)
        {
            rect = GetComponent<RectTransform>();
        }

        if (rect == null)
        {
            return;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(rebuildDelay));

        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        Canvas.ForceUpdateCanvases();
    }
}
