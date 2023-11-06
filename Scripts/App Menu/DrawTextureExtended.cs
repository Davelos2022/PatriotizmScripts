using LS.DrawTexture.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DrawTextureExtended : DrawTextureUI
{
    [SerializeField] private List<Color> colors;

    private bool _drawEnabled;
    private bool _blocked;

    public void SetDrawEnabled(bool value)
    {
        _drawEnabled = value;
    }

    protected override void UpdateInternal()
    {
        if(_drawEnabled)
        {
            base.UpdateInternal();
        }
    }

    public void SetColor(int index)
    {
        color = colors[index];
        ChangeColor();
    }

    public void SetPen(float size)
    {
        type = BrushType.none;
        this.size = size;
        ChangeSize();
        ChangeBrush();
    }

    public void SetEraser(float size)
    {
        type = BrushType.eraser;
        this.size = size;
        ChangeSize();
        ChangeBrush();
    }
}
