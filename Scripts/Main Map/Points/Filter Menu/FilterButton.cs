using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class FilterButton : MonoCached
{
    [SerializeField] private Group filterType;
    [Space]
    [SerializeField] private Color buttonColor;
    [SerializeField] private Image menuIcon;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private FilterButtonsGroup filtersGroup;
    [SerializeField] private Sprite pressedButtonSprite;
    [SerializeField] private Sprite normalButtonSprite;
    [SerializeField] private Image stateDotImage; 
    [SerializeField] private RectTransform backgroundGradient;
    [SerializeField] private Image backroundImage;
    [SerializeField] private Vector2 normalSize;
    [SerializeField] private Vector2 pressedSize;
    [SerializeField] private float animationsDuration;

    private CancellationTokenSource _tokenSource = new CancellationTokenSource();

    public Group FilterType => filterType;
    public Image MenuIcon => menuIcon;
    public Color ButtonColor => buttonColor;

    protected override void Rise()
    {
        filtersGroup.RegisterFilter(this);
        UpdateSize();
        SetColors();
    }

    public void DisableVisuals()
    {
        UpdateSize();
        canvasGroup.blocksRaycasts = false;
        nameText.gameObject.SetActive(false);
        Hide();
    }

    public void EnableVisuals(bool show)
    {
        UpdateSize();
        canvasGroup.blocksRaycasts = true;
        nameText.gameObject.SetActive(true);

        if(show)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void UpdateSize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
        pressedSize.x = Rect.sizeDelta.x;
    }

    public async UniTask Show()
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
        _tokenSource = new CancellationTokenSource();

        stateDotImage.sprite = pressedButtonSprite;
        UpdateSize();
        
        await backgroundGradient.DOSizeDelta(pressedSize, animationsDuration).WithCancellation(_tokenSource.Token);
    }

    public async UniTask Hide()
    {
        UpdateSize();
        _tokenSource.Cancel();
        _tokenSource.Dispose();
        _tokenSource = new CancellationTokenSource();

        stateDotImage.sprite = normalButtonSprite;

        await backgroundGradient.DOSizeDelta(normalSize, animationsDuration).WithCancellation(_tokenSource.Token);
    }

    public void OnClickCallback()
    {
        UpdateSize();
        filtersGroup.OnFilterPressedCallback(this);
    }

    [Button("Try Set Colors in Edit Mode")]
    public void SetColors()
    {
        UpdateSize();
        var mat = Instantiate(backroundImage.material);

        var gr = GroupInfoContainer.GetGradient(filterType);

        mat.SetColor("_TopLeftColor", gr.Evaluate(0));
        mat.SetColor("_BottomLeftColor", gr.Evaluate(0));
        mat.SetColor("_TopRightColor", gr.Evaluate(1));
        mat.SetColor("_BottomRightColor", gr.Evaluate(1));

        backroundImage.material = mat;
    }
}

public class EnableFilterButtonsMessage: Message { }
public class DisableFilterButtonsMessage: Message { }
