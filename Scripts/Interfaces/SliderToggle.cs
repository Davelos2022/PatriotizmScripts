using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class SliderToggle: MonoCached
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private RectTransform toggleTransform;
    [SerializeField] private float toggleDuration = 0.2f;
    [SerializeField] private Image background;
    [SerializeField] private Color onColor;
    [SerializeField] private Color offColor;

    public void OnToggleCallback(bool value)
    {
        toggleTransform.DOKill();

        if(value)
        {
            background.DOColor(onColor, toggleDuration);
            toggleTransform.DOAnchorMax(new Vector2(1, 0.5f), toggleDuration);
            toggleTransform.DOAnchorMin(new Vector2(1, 0.5f), toggleDuration);
        }
        else
        {
            background.DOColor(offColor, toggleDuration);
            toggleTransform.DOAnchorMax(new Vector2(0, 0.5f), toggleDuration);
            toggleTransform.DOAnchorMin(new Vector2(0, 0.5f), toggleDuration);
        }
    }
}
