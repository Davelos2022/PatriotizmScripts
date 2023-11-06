using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PinInput : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite enteredSprite;

    private char? _currentvalue;

    public char? CurrentValue => _currentvalue;

    public void SetValue(char value)
    {
        image.sprite = enteredSprite;
        _currentvalue = value;
    }

    public void ClearValue()
    {
        image.sprite = emptySprite;
        _currentvalue = null;
    }
}
