using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class EditSwitcherVariation : MonoCached, IPooled
{
    [SerializeField] private bool pointEditResolve;
    [SerializeField] private bool controlSelf = true;

    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
    [SerializeField] private Image _icon;
    
    public UnityEvent EnabledEdit;
    public UnityEvent DisabledEdit;

    protected override void Rise()
    {
        Messager.Instance.Subscribe<AdminModeChangedMessage>(_ => ResolveEditState(), gameObject.scene.name);

        ResolveEditState();
    }

    public void ResolveEditState()
    {
        if(GameManager.Instance.IsAdminMode)
        {
            if(pointEditResolve)
            {
                if (GameManager.Instance.IsUserPointOpened)
                {
                    EnabledEdit?.Invoke();
                    //if (controlSelf) gameObject.SetActive(true);
                    if (controlSelf) ToggleElements(true);
                }
                else
                {
                    DisabledEdit?.Invoke();
                    //if (controlSelf) gameObject.SetActive(false);
                    if (controlSelf) ToggleElements(false);
                }
            }
            else
            {
                EnabledEdit?.Invoke();
                //if (controlSelf) gameObject.SetActive(true);
                if (controlSelf) ToggleElements(true);
            }

        }
        else
        {
            DisabledEdit?.Invoke();
            //if (controlSelf) gameObject.SetActive(false);
            if (controlSelf) ToggleElements(false);
        }
    }

    void ToggleElements(bool value)
    {
        _button.image.enabled = value;
        _textMeshProUGUI.gameObject.SetActive(value);
        _icon.gameObject.SetActive(value);
    }

    public void OnSpawn(object data)
    {
        ResolveEditState();
    }
}
