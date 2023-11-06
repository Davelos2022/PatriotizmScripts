using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class AnimToggle : MonoCached
{
    [SerializeField] private RectTransform toggleViewPivot;
    [SerializeField] private float toggleDuration;
    [SerializeField] private Image colorView;
    [SerializeField] private Color onColor = Color.green;
    [SerializeField] private Color offColor = Color.red;
    [SerializeField] private ToggleButton externalControlButton;

    private CancellationTokenSource _tokenSource;

    protected override void Rise()
    {
        if(externalControlButton != null)
        {
            externalControlButton.ToggledOn.AddListener(ToggleButtonOnWrapper);
            externalControlButton.ToggledOff.AddListener(ToggleButtonOffWrapper);

            if(externalControlButton.IsEnabled)
            {
                On();
            }
            else
            {
                Off();
            }
        }
        else
        {
            Off();
        }
    }

    private void ToggleButtonOnWrapper(Sprite s)
    {
        On();
    }

    private void ToggleButtonOffWrapper(Sprite s)
    {
        Off();
    }

    public void On()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        var old = toggleViewPivot.localPosition;

        toggleViewPivot.anchorMax = new Vector2(1, 0.5f);
        toggleViewPivot.anchorMin = new Vector2(1, 0.5f);

        toggleViewPivot.localPosition = old;

        toggleViewPivot.SetPivot(new Vector2(1, 0.5f));

        colorView.DOColor(onColor, toggleDuration).WithCancellation(_tokenSource.Token);

        toggleViewPivot.DOAnchorPos(Vector2.zero, toggleDuration).WithCancellation(_tokenSource.Token);
    }

    public void Off()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        var old = toggleViewPivot.localPosition;

        toggleViewPivot.anchorMax = new Vector2(0, 0.5f);
        toggleViewPivot.anchorMin = new Vector2(0, 0.5f);

        toggleViewPivot.localPosition = old;

        toggleViewPivot.SetPivot(new Vector2(0, 0.5f));

        colorView.DOColor(offColor, toggleDuration).WithCancellation(_tokenSource.Token);

        toggleViewPivot.DOAnchorPos(Vector2.zero, toggleDuration).WithCancellation(_tokenSource.Token);
    }
}
