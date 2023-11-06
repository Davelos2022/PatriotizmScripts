using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueRestorer : MonoBehaviour
{
    [SerializeField] private bool restoreFullIfZero;
    [SerializeField] private Slider slider;

    private bool _canCapture = true;

    private float _capturedValue;

    public void CaptureAvailable(bool value)
    {
        _canCapture = value;
    }

    public void CaptureValue()
    {
        if (!_canCapture) return;

        _capturedValue = slider.value;
    }

    public void RestoreValue()
    {
        if(restoreFullIfZero && _capturedValue <= 0)
        {
            slider.value = 1;
        }
        else
        {
            slider.value = _capturedValue;
        }

    }
}
