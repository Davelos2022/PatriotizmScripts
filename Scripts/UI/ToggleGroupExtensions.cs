using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ToggleGroupExtensions
{
    private static System.Reflection.FieldInfo _toggleListMember;

    public static IList<Toggle> GetToggles(this ToggleGroup grp)
    {
        if (_toggleListMember == null)
        {
            _toggleListMember = typeof(ToggleGroup).GetField("m_Toggles", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (_toggleListMember == null)
                throw new System.Exception("UnityEngine.UI.ToggleGroup source code must have changed in latest version and is no longer compatible with this version of code.");
        }
        return _toggleListMember.GetValue(grp) as IList<Toggle>;
    }
}
