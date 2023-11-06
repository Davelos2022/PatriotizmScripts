using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class ColorSwitcher : MonoCached
{
    [SerializeField] private Color onColor;
    [SerializeField] private Color offColor;

    public UnityEvent<Color> switchEvent;

    public void Switch(bool value)
    {
        if (value)
        {
            switchEvent.Invoke(onColor);
        }
        else
        {
            switchEvent.Invoke(offColor);
        }
    }
}
