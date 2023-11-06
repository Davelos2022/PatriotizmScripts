using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class ToggleButton : MonoCached
{
    [SerializeField] private bool autoResolveImage;
    [SerializeField] private bool enabledOnStart;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    public UnityEvent<Sprite> ToggledOn;
    public UnityEvent<Sprite> ToggledOff;

    private Image img;

    private bool _on = false;

    public bool IsEnabled => _on;

    private void OnEnable()
    {
        if (img == null)
            Rise();
    }

    protected override void Rise()
    {
        _on = enabledOnStart;

        var toggle = GetComponent<Toggle>();

        if(toggle != null)
        {
            toggle.isOn = _on;
            toggle.onValueChanged.AddListener(SetState);
        }

        img = GetComponent<Image>();
    }

    public void OnClick()
    {
        _on = !_on;

        ResolveEvent();
    }

    private void ResolveEvent()
    {
        if(_on)
        {
            if(autoResolveImage && img != null)
            {
                img.sprite = onSprite;
            }

            ToggledOn.Invoke(onSprite);
        }
        else
        {
            if (autoResolveImage && img != null)
            {
                img.sprite = offSprite;
            }

            ToggledOff.Invoke(offSprite);
        }
    }
    
    public void SetState(bool value)
    {
        _on = value;

        ResolveEvent();
    }


    public void SetStateSilently(bool value) 
    {
        _on = value;
    }
}
