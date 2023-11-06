using SolidUtilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float interval;

    private float timer;

    private void Update()
    {
        if(timer >= interval)
        {
            text.text = (1f / Time.unscaledDeltaTime).ToString("F0");
            timer -= interval;
        }
        else
        {
            timer += Time.unscaledDeltaTime;
        }
    }
}
