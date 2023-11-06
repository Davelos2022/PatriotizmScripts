using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VolumeBox.Toolbox;

public class OtherGroupLabelBinder : MonoCached
{
    [SerializeField] private int otherGroupIndex;
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Dropdown dropdown;

    protected override void Rise()
    {
        Messager.Instance.Subscribe<UpdateOtherGroupLabelMessage>(_ => UpdateLabel(), gameObject.scene.name);
    }

    private void UpdateLabel()
    {
        if(dropdown.value == otherGroupIndex)
        {
            label.text = GameManager.Instance.Settings.otherGroupName;
        }
    }
}
