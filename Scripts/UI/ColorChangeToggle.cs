using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ColorChangeToggle : MonoBehaviour
{
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private float toggleDuration;
    [SerializeField] private Color onColor;
    [SerializeField] private Color offColor;

    private CancellationTokenSource _tokenSource;

    public void On()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        targetGraphic.DOColor(onColor, toggleDuration).WithCancellation(_tokenSource.Token);
    }

    public void Off()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        targetGraphic.DOColor(offColor, toggleDuration).WithCancellation(_tokenSource.Token);
    }
}
