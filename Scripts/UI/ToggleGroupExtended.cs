using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleGroupExtended : ToggleGroup
{
    public UnityEvent<int> OnToggleChanged;

    protected override void Awake()
    {
        base.Awake();

        m_Toggles.ForEach(x => x.onValueChanged.AddListener(OnTogglesValueChanged));
    }




    public int GetActiveToggleIndex()
    {
        var toggles = ActiveToggles();

        if(toggles.Count() <= 0)
        {
            return -1;
        }

        var toggle = toggles.FirstOrDefault();

        if (toggle == null)
        {
            return -1;
        }
        else
        {
            return m_Toggles.IndexOf(toggle);
        }
    }

    private void OnTogglesValueChanged(bool value)
    {
        if(value || m_Toggles.All(t => !t.isOn))
        {
            OnToggleChanged.Invoke(GetActiveToggleIndex());
        }
    }
}
