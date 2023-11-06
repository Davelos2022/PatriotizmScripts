using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class RotatingLoader : Loader
{
    private List<Image> _images = new List<Image>();

    private Color _transparent = new Color(1, 1, 1, 0);

    private bool _rotating;

    private CancellationTokenSource _tokenSource;

    private void Awake()
    {
        RearrangePieces();
    }

    [Button("Rearrange Pieces")]
    public void RearrangePieces()
    {
        _images.Clear();

        float currentRotation = 0;
        float rotationInterval = 360f / transform.childCount;

        foreach(Transform child in transform)
        {
            _images.Add(child.GetComponent<Image>());

            child.eulerAngles = new Vector3(0, 0, currentRotation);
            currentRotation += rotationInterval;
        }
    }

    public override void StartLoading()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        StartRotationAsync(_tokenSource.Token);    
    }


    private async UniTask StartRotationAsync(CancellationToken token)
    {
        _images.ForEach(i => i.color = _transparent);

        _rotating = true;

        int imageIndex = 0;

        while(_rotating)
        {
            if(token.IsCancellationRequested)
            {
                break;
            }

            await _images[imageIndex].DOFade(1, 0.1f);
            _images[imageIndex].DOFade(0, 1).WithCancellation(token);

            imageIndex++;

            if(imageIndex >= _images.Count)
            {
                imageIndex = 0;
            }
        }
    }

    public override void StopLoading()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();

        StopRotationAsync(_tokenSource.Token);
    }


    private async UniTask StopRotationAsync(CancellationToken token)
    {
        _rotating = false;

        await UniTask.WhenAll(_images.ConvertAll(i => 
        {
            i.DOKill();
            return i.DOFade(0, 0.3f).WithCancellation(token);
        }));
    }
}
