using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using VolumeBox.Toolbox;

public class LongTapBehaviour: MonoCached, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler
{
    [SerializeField] private float preDuration;
    [SerializeField] private float pressDuration;
    [SerializeField] private Canvas canvas;

    public UnityEvent<Vector2> TapBeginPositionedEvent;
    public UnityEvent<float> NormalizedPressStateEvent;
    public UnityEvent<Vector2> OnLongTapEvent;

    private bool beginPress;
    private bool cancelledPress;
    private float timer;
    private float preTimer;
    private Vector2 _tapPosition;

    public bool Enabled { get; set; } = true;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!Enabled) return;

        cancelledPress = true;
        timer = 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Enabled) return;

        beginPress = true;
        timer = 0;
        preTimer = 0;

        //_tapPosition = ScreenUtils.PointerPositionToCanvas(eventData.position);
        //_tapPosition /= canvas.scaleFactor;
        _tapPosition = eventData.position / canvas.scaleFactor;
        if (Is4_3()) _tapPosition.y -= 178;
        //_tapPosition.x *= Screen.width;
        //_tapPosition.y *= Screen.height;
        TapBeginPositionedEvent.Invoke(_tapPosition);
    }

    bool Is4_3() {
        return ((float)Screen.width / (float)Screen.height) < 1.5f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!Enabled) return;

        beginPress = false;
        cancelledPress = false;
        timer = 0;
        preTimer = 0;
    }

    protected override void Tick()
    {
        if(beginPress && !cancelledPress && Enabled)
        {
            if(preTimer < preDuration)
            {
                preTimer += delta;
            }

            if(preTimer >= preDuration)
            {
                if(timer >= pressDuration)
                {
                    beginPress = false;
                    timer = 0;
                    preTimer = 0;
                    OnLongTapEvent.Invoke(_tapPosition);
                }
                timer += delta;
            }
        }

        NormalizedPressStateEvent.Invoke(timer / pressDuration);
    }
}
