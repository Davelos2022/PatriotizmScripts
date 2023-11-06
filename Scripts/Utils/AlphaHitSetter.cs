using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class AlphaHitSetter : MonoCached
{
    [SerializeField, Range(0, 1)] private float alphaThreshold;

    private Image graphic;

    protected override void Rise()
    {
        graphic = GetComponent<Image>();

        UpdateThreshold();
    }

    private void UpdateThreshold()
    {
        graphic.alphaHitTestMinimumThreshold = alphaThreshold;
    }

    private void OnValidate()
    {
        if(graphic == null)
        {
            graphic = GetComponent<Image>();
        }

        UpdateThreshold();
    }
}
