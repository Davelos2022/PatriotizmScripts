using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class DrawMenuSelector : MonoCached
{
    [SerializeField] private Color selectionColor;
    [SerializeField] private Color normalColor;
    [SerializeField] private List<DrawerSelectionInfo> drawers;
    [SerializeField] private List<DrawerSelectionInfo> colors;

    protected override void Rise()
    {
        SetSelection(drawers.First(), true);
        SetSelection(colors.First(), true);
    }

    public void SetPen(string tag)
    {
        HandleList(drawers, tag);
    }

    public void SetColor(string tag) 
    {
        HandleList(colors, tag);   
    }

    private void HandleList(List<DrawerSelectionInfo> list, string tag)
    {
        list.ForEach(d =>
        {
            SetSelection(d, false);
        });

        var sel = list.FirstOrDefault(x => x.tag == tag);

        if(sel != null)
        {
            SetSelection(sel, true);
        }
    }

    private void SetSelection(DrawerSelectionInfo drawer, bool selected)
    {
        Color col = selected ? selectionColor : normalColor;

        drawer.mainButton.color = col;
        drawer.image.color = col;
    }
}

[Serializable]
public class DrawerSelectionInfo
{
    public string tag;
    public Image mainButton;
    public Image image;
}
