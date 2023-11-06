using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VolumeBox.Toolbox;

public class OtherGroupBehaviour : MonoCached
{
    [SerializeField] private TMP_Text label;

    protected override void Rise()
    {
        Messager.Instance.Subscribe<UpdateOtherGroupLabelMessage>(_ => UpdateLabel());

        UpdateLabel();
    }

    private void UpdateLabel()
    {
        label.text = GameManager.Instance.Settings.otherGroupName;
    }
}

public class UpdateOtherGroupLabelMessage: Message { }
