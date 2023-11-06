using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClampedContentSizeFitter: UIBehaviour, ILayoutSelfController
{
    [SerializeField] private bool useMaxWidth;
    [SerializeField, ShowIf(nameof(useMaxWidth))] private float maxWidth;
    [SerializeField] private bool useMaxHeight;
    [SerializeField, ShowIf(nameof(useMaxHeight))] private float maxHeight;

    [System.NonSerialized] private RectTransform m_Rect;
    private RectTransform rectTransform
    {
        get
        {
            if (m_Rect == null)
                m_Rect = GetComponent<RectTransform>();
            return m_Rect;
        }
    }

    public void SetLayoutHorizontal()
    {
        if(!useMaxWidth) return;

        float preferred = LayoutUtility.GetPreferredSize(rectTransform, 0);

        float width = Mathf.Clamp(preferred, 0, maxWidth);

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    public void SetLayoutVertical()
    {
        if (!useMaxHeight) return;

        float preferred = LayoutUtility.GetPreferredSize(rectTransform, 1);

        float height = Mathf.Clamp(preferred, 0, maxHeight);

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    [ContextMenu("Set Dirty")]
    protected void SetDirty()
    {
        if (!IsActive())
            return;

        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    protected override void OnRectTransformDimensionsChange()
    {
        SetDirty();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetDirty();
    }

#endif
}
