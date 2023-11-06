using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ToggleValueEventWrapper : MonoBehaviour
{
    public UnityEvent OnEvent;
    public UnityEvent OffEvent;

    public void ToggleValueChangedCallback(bool value)
    {
        if(value)
        {
            OnEvent.Invoke();
        }
        else
        {
            OffEvent.Invoke();
        }
    }
}
