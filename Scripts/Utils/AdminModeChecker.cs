using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class AdminModeChecker : MonoCached
{
    public UnityEvent OnAdminEnabled;
    public UnityEvent OnAdminDisabled;

    protected override void Rise()
    {
        Messager.Instance.Subscribe<AdminModeChangedMessage>(m => OnModeChangedCallback(m.enabled));
    }

    private void OnModeChangedCallback(bool value)
    {
        if(value)
        {
            OnAdminEnabled.Invoke();
        }
        else
        {
            OnAdminDisabled.Invoke();
        }
    }
}
