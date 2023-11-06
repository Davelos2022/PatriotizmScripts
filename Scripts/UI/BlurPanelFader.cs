using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class BlurPanelFader : MonoCached
{
    [SerializeField] private Image image;
    [SerializeField] private string shaderParameterName = "_Size";
    [SerializeField] private float blurAmount = 2.83f;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private Color fadedOutColor;
    [SerializeField] private Color fadedInColor;

    private Material tweakingMaterial;

    public float FadeInDuration => fadeInDuration;
    public float FadeOutDuration => fadeOutDuration;

    public void FadeOut()
    {
        FadeOutAsync();
    }

    public async UniTask FadeOutAsync()
    {
        if(image == null)
        {
            image = GetComponent<Image>();
        }

        if(image == null)
        {
            return;
        }

        if(tweakingMaterial == null)
        {
            tweakingMaterial = Instantiate(image.material);
        }

        image.material = tweakingMaterial;

        tweakingMaterial.DOKill();
        tweakingMaterial.DOFloat(0, shaderParameterName, fadeOutDuration);
        image.DOKill();
        image.DOColor(fadedOutColor, fadeOutDuration);
        await UniTask.Delay(TimeSpan.FromSeconds(fadeOutDuration));
    }

    public void FadeIn()
    {
        FadeInAsync();
    }

    public async UniTask FadeInAsync()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (image == null)
        {
            return;
        }

        if (tweakingMaterial == null)
        {
            tweakingMaterial = Instantiate(image.material);
        }

        image.material = tweakingMaterial;

        image.material.DOKill();
        image.material.DOFloat(blurAmount, shaderParameterName, fadeInDuration);
        image.DOKill();
        image.DOColor(fadedInColor, fadeInDuration);
        await UniTask.Delay(TimeSpan.FromSeconds(fadeInDuration));
    }
}
