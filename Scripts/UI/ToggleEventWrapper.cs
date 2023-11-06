using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class ToggleEventWrapper : MonoCached
{
    public UnityEvent ToggledOn;
    public UnityEvent ToggledOff;

    protected override void Rise()
    {
        var toggle = GetComponent<Toggle>();

        if(toggle != null)
        {
            toggle.onValueChanged.AddListener(ToggleCallback);
        }
    }

    public void ToggleCallback(bool value)
    {
        if(value)
        {
            ToggledOn.Invoke();
        }
        else
        {
            ToggledOff.Invoke();
        }

    }
}
