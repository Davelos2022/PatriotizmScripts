using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class AppMenu : MonoCached
{
    [SerializeField] private Image menuIcon;
    [SerializeField] private Image bottomImage;
    [SerializeField] private Color defaultColor;
    [SerializeField] private FilterButtonsGroup filterButtonsGroup;
    [Space]
    [SerializeField] private AppButtonsHandler buttonsHandler;
    [SerializeField] private CanvasGroupFader backFader;
    [SerializeField] private BlurPanelFader backBlurFader;
    [Space]
    [SerializeField] private GameObject mapOpenedView;
    [SerializeField] private GameObject mapClosedView;
    [Space]
    [SerializeField] private GameObject appOpenedView;
    [SerializeField] private GameObject appClosedView;
    [Space]
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private float animDuration;
    [SerializeField] private ToggleButton toggle;
    [SerializeField] private List<StatePositionDefinition> positions;

    public UnityEvent OpenedEvent;
    public UnityEvent ClosedEvent;

    private CancellationTokenSource _tokenSource;

    private bool _opened;

    protected override void Rise()
    {
        Messager.Instance.Subscribe<OpenAppMenuMessage>(_ => OpenMenu(), gameObject.scene.name);
        Messager.Instance.Subscribe<CloseAppMenuMessage>(_ => CloseMenu(), gameObject.scene.name);
        CloseMenu();
    }

    private StatePositionDefinition GetDefinition()
    {
        return positions.FirstOrDefault(x => x.state == GameManager.Instance.CurrentAppState);
    }

    public void OpenMenu()
    {
        var def = GetDefinition();


        if(def != null)
        {
            if(def.state == AppState.MAIN_MAP)
            {
                menuIcon.DOKill();
                menuIcon.DOFade(1, 0.2f);
                bottomImage.DOKill();
                bottomImage.DOColor(defaultColor, 0.2f);

                if(filterButtonsGroup.CurrentPressed != null)
                {
                    filterButtonsGroup.CurrentPressed.MenuIcon.DOKill();
                    filterButtonsGroup.CurrentPressed.MenuIcon.DOFade(0, 0.2f);
                }
            }
            
            buttonsHandler.Show();
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();

            targetRect.DOAnchorPos(def.firstPosition, animDuration).WithCancellation(_tokenSource.Token);

            if (toggle != null)
            {
                toggle.SetStateSilently(true);
            }

            _opened = true;
            Messager.Instance.Send<CloseFilterVisualsMessage>();
            OpenedEvent.Invoke();
            backFader.FadeIn();
            ResolveVisuals(true);
        }
    }

    public async UniTask OpenMenuAsync()
    {
        OpenMenu();
        await UniTask.Delay(TimeSpan.FromSeconds(animDuration)).AttachExternalCancellation(_tokenSource.Token);
    }

    public void CloseMenu()
    {
        var def = GetDefinition();
        
        if(def != null)
        {
            if (def.state == AppState.MAIN_MAP)
            {
                filterButtonsGroup.UpdateVisual();
            }

            buttonsHandler.Hide();
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();

            targetRect.DOAnchorPos(def.secondPosition, animDuration).WithCancellation(_tokenSource.Token);

            if(toggle != null)
            {
                toggle.SetStateSilently(false);
            }

            _opened = false;
            Messager.Instance.Send<OpenFilterVisualsMessage>();
            ClosedEvent.Invoke();
            backFader.FadeOut();
            backBlurFader.FadeOut();
            ResolveVisuals(false);
        }
    }

    public async UniTask CloseMenuAsync()
    {
        CloseMenu();
        await UniTask.Delay(TimeSpan.FromSeconds(animDuration)).AttachExternalCancellation(_tokenSource.Token);
    }

    private void ResolveVisuals(bool open)
    {
        if(GameManager.Instance.CurrentAppState == AppState.MAIN_MAP)
        {
            appClosedView.SetActive(false);
            appOpenedView.SetActive(false);

            if(_opened)
            {
                mapClosedView.SetActive(false);
                mapOpenedView.SetActive(true);
            }
            else
            {
                mapClosedView.SetActive(true);
                mapOpenedView.SetActive(false);
            }
        }
        else
        {
            if(open)
            {
                backBlurFader.FadeIn();
            }

            mapClosedView.SetActive(false);
            mapOpenedView.SetActive(false);

            if (_opened)
            {
                appClosedView.SetActive(false);
                appOpenedView.SetActive(true);
            }
            else
            {
                appClosedView.SetActive(true);
                appOpenedView.SetActive(false);
            }
        }
    }
}

public class OpenAppMenuMessage: Message
{

}

public class CloseAppMenuMessage: Message
{

}

public class HideAppMenuMessage: Message { }

[Serializable]
public class StatePositionDefinition
{
    public AppState state;
    public Vector2 firstPosition;
    public Vector2 secondPosition;
}

