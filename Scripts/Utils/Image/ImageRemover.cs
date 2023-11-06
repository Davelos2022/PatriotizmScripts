using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageRemover : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private RawImage rawImage;

    public void Remove()
    {
        if(image is not null)
        {
            image.sprite = null;
        }

        if(rawImage is not null)
        {
            rawImage.texture = null;
        }
    }
}
