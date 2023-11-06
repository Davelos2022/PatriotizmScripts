using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class TogglesHandler : MonoCached
{
    [SerializeField] private List<Toggle> toggles;

    public UnityEvent<int> OnToggleChanged;

    protected override void Rise()
    {
        toggles.ForEach(t => t.onValueChanged.AddListener(OnToggleValueChanged));
    }

    private void OnToggleValueChanged(bool value)
    {
        if(toggles.All(t => !t.isOn))
        {
            OnToggleChanged.Invoke(-1);
        }

        if (value)
        {
            OnToggleChanged.Invoke(toggles.IndexOf(toggles.First(x => x.isOn)));
        }
    }
}
