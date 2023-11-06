using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndexedColorChanger : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private List<Color> colors;

    public void SetColor(int index)
    {
        image.color = colors[index];
    }
}
