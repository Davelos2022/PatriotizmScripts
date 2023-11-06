using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

[RequireComponent(typeof(RawImage))]
public class RawImageAspectPreserver : MonoCached
{
    [SerializeField] private bool fill;
    [SerializeField] private bool setInUpdate;

    private AspectRatioFitter arf;
    private RawImage img;

    [Button("Set Aspect")]
    public void SetAspect()
    {
        if(img == null)
        {
            img = GetComponent<RawImage>();
        }

        if (img == null || img.texture == null) return;

        if(fill)
        {
            if(arf == null)
            {
                arf = gameObject.GetComponent<AspectRatioFitter>();

                if(arf == null)
                {
                    arf = gameObject.AddComponent<AspectRatioFitter>();
                }
            }

            float aspect = (float)img.texture.width / (float)img.texture.height;
            Debug.Log($"{aspect} {img.texture.width} {img.texture.height}");
            Rect.anchorMax = Vector2.one;
            Rect.anchorMin = Vector2.zero;

            arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            arf.aspectRatio = aspect;
        }
        else
        {
            Rect.anchorMax = Vector2.one * 0.5f;
            Rect.anchorMin = Vector2.one * 0.5f;

            if (arf != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(arf);
#else
                Destroy(arf);
#endif
            }

            if(img != null)
            {
                img.SizeToParent();
            }
        }
    }

    protected override void Tick()
    {
        try
        {
            if(setInUpdate)
            {
                //SetAspect();
            }
        }
        catch
        {

        }
    }
}
