using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;
using VolumeBox.Toolbox.UIInformer;

public class MenuSceneHandler: SceneHandler<MenuSceneArgs>
{
    [SerializeField] private Vector2 exitPinPadPosition;
    [SerializeField] private Vector2 adminPinPadPosition;
    [SerializeField] private AppMenu appMenu;
    [SerializeField] private CanvasGroupFader menuFader;
    [SerializeField] private List<StateObjectDefinition> appIcons;
    [SerializeField] private List<StateObjectDefinition> appButtons;
    [SerializeField] private CanvasGroupFader filterButtonsFader;
    [SerializeField] private FilterButtonsGroup filterButtonsGroup;
    public UnityEvent Loaded;
    public UnityEvent Unloading;

    public AppMenu Menu => appMenu;

    protected override void Rise()
    {
        Messager.Instance.Subscribe<AppStateChangedMessage>(m => OnAppStateChanged(m.state), gameObject.scene.name);
        Messager.Instance.Subscribe<ShowMenuMessage>(_ => ShowMenu());
        Messager.Instance.Subscribe<HideMenuMessage>(_ => HideMenu());
    }

    public void AdminModeChangeClickHandler()
    {
        if(GameManager.Instance.IsAdminMode)
        {
            AdminModeChangeCallback();
        }
        else
        {
            PinPad.Instance.OpenAt(adminPinPadPosition, () => AdminModeChangeCallback(), "Для входа в режим Администратора введите пин");
        }
    }

    private void AdminModeChangeCallback()
    {
        if(GameManager.Instance.IsAdminMode)
        {
            GameManager.Instance.DisableAdminMode();
        }
        else
        {
            GameManager.Instance.EnableAdminMode();
        }
    }

    private void OnAppStateChanged(AppState state)
    {
        foreach(var def in appButtons)
        {
            if(def.state == state) 
            {
                def.relatedObject.SetActive(false);
            }
            else
            {
                def.relatedObject.SetActive(true);
            }
        }

        foreach(var icon in appIcons)
        {
            if (icon.state == state)
            {
                icon.relatedObject.SetActive(true);
            }
            else
            {
                icon.relatedObject.SetActive(false);
            }
        }

        UpdateFilterVisual();

        Messager.Instance.Send<CloseAppMenuMessage>();
    }

    public void ShowMenu()
    {
        menuFader.FadeIn();
    }

    public void HideMenu()
    {
        menuFader.FadeOut();
    }

    protected override void SetupScene(MenuSceneArgs args)
    {
        UpdateFilterVisual();

        Loaded.Invoke();
    }

    private void UpdateFilterVisual()
    {
        if (GameManager.Instance.CurrentAppState == AppState.MAIN_MAP)
        {
            filterButtonsGroup.UpdateCurrent();
            filterButtonsFader.FadeIn();
        }
        else
        {
            filterButtonsFader.FadeOut();
        }
    }

    protected override void OnSceneUnload()
    {
        Unloading.Invoke();
    }

    public void ExitClickHandler()
    {
        PinPad.Instance.OpenAt(exitPinPadPosition, () => ExitCallback(), "Для выхода из программы введите пин");
    }

    private void ExitCallback()
    {
        Info.Instance.ShowBox("Вы действительно хотите выйти из приложения?", () => Exit(), null, "Подтвердите действие", "Да", "Отмена", GameManager.Instance.WarningColor);
    }

    private void Exit()
    {
        Application.Quit();
    }
}

[Serializable]
public class StateObjectDefinition
{
    public AppState state;
    public GameObject relatedObject;
}

public class HideMenuMessage: Message { }
public class ShowMenuMessage: Message { }