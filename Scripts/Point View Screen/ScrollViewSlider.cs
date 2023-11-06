using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class ScrollViewSlider : MonoCached
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Slider slider;

    protected override void Rise()
    {
        scrollRect.onValueChanged.AddListener(ScrollViewCallback);
    }

    private void ScrollViewCallback(Vector2 value)
    {
        slider.value = value.x;
    }
}
