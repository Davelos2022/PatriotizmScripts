using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class VolumeSliderBehaviour : MonoCached
{
    protected override void Rise()
    {
        Messager.Instance.Subscribe<SettingsChangedMessage>(_ => UpdateState(), gameObject.scene.name);
        Messager.Instance.Subscribe<AdminModeChangedMessage>(_ => UpdateState(), gameObject.scene.name);
    }

    private void UpdateState()
    {
        gameObject.SetActive(GameManager.Instance.IsAdminMode || GameManager.Instance.Settings.volumeSliderEnabled);
    }
}
