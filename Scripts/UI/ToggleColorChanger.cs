using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ToggleColorChanger : MonoBehaviour
{
    [SerializeField] private List<ColorSwitcher> switchers;

    public void ToggleValueChangedCallback(bool value)
    {
        foreach(var switcher in switchers) 
        {
            switcher.Switch(value);
        }
    }
}
