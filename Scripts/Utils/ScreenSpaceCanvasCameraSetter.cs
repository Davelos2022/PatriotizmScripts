using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSpaceCanvasCameraSetter : MonoBehaviour
{
    private void Awake()
    {
        Set();
    }

    [Button("Set")]
    public void Set()
    {
        var canvas = GetComponent<Canvas>();

        if(canvas != null)
        {
            canvas.worldCamera = Camera.main;
        }
    }
}
