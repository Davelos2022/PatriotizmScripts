using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class SettingsBehaviour : MonoCached
{
    protected override void Rise()
    {
        UpdateState();
        Messager.Instance.Subscribe<AdminModeChangedMessage>(m => UpdateState(), gameObject.scene.name);
    }

    public void UpdateState()
    {
        gameObject.SetActive(GameManager.Instance.IsAdminMode);
    }
}
