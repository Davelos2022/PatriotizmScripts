using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class EditableSwitcher : MonoCached, IPooled
{
    [SerializeField] private bool pointEditResolve;
    [SerializeField] private bool controlSelf = true;

    public UnityEvent EnabledEdit;
    public UnityEvent DisabledEdit;

    protected override void Rise()
    {
        Messager.Instance.Subscribe<AdminModeChangedMessage>(_ => ResolveEditState(), gameObject.scene.name);
        Messager.Instance.Subscribe<UpdateEditableSwitchersMessage>(_ => ResolveEditState(), gameObject.scene.name);

        ResolveEditState();
    }

    public void ResolveEditState()
    {
        //EnabledEdit?.Invoke();
        if (GameManager.Instance.IsAdminMode)
        {
            if (pointEditResolve)
            {
                if (GameManager.Instance.IsUserPointOpened)
                {
                    EnabledEdit?.Invoke();
                    if (controlSelf) gameObject.SetActive(true);
                }
                else
                {
                    DisabledEdit?.Invoke();
                    if (controlSelf) gameObject.SetActive(false);
                }
            }
            else
            {
                EnabledEdit?.Invoke();
                if (controlSelf) gameObject.SetActive(true);
            }

        }
        else
        {
            DisabledEdit?.Invoke();
            if (controlSelf) gameObject.SetActive(false);
        }
    }

    public void OnSpawn(object data)
    {
        ResolveEditState();
    }
}

public class UpdateEditableSwitchersMessage: Message
{ }

