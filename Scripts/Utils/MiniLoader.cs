using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MiniLoader : MonoBehaviour
{
    public UnityEvent ShowEvent;
    public UnityEvent HideEvent;

    public void Show()
    {
        ShowEvent?.Invoke();
    }

    public void Hide()
    {
        HideEvent?.Invoke();
    }
}
