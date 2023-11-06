using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VolumeBox.Toolbox;
using VolumeBox.Toolbox.UIInformer;

public class AppHandler : CachedSingleton<AppHandler>
{
    [SerializeField, Scene] private string mainSceneName;
    [SerializeField] private float fadeInDuration;
    [SerializeField] private float fadeOutDuration;
    [SerializeField] private List<AppStateDefinition> definitions;

    public UnityEvent OnAppOpening;
    public UnityEvent OnAppClosed;

    private string _currentOpenedAppSceneName;

    private bool unloadCurrent = false;

    private MainMapSceneHandler _mapSceneHandler;

    protected override void Rise()
    {
        Messager.Instance.Subscribe<OpenAppMessage>(m => Open(m.appSceneName));
        Messager.Instance.Subscribe<CloseCurrentAppMessage>(_ => CloseCurrent());
    }

    private async UniTask<MainMapSceneHandler> GetMainMap()
    {
        _mapSceneHandler = Traveler.TryGetSceneHandler<MainMapSceneHandler>();

        if(_mapSceneHandler is null)
        {
            await Traveler.LoadScene(mainSceneName);
            _mapSceneHandler = Traveler.TryGetSceneHandler<MainMapSceneHandler>();
        }

        return _mapSceneHandler;
    }

    public async UniTask Open(string appSceneName)
    {
        if (_currentOpenedAppSceneName.IsValuable())
        {
            Info.Instance.ShowBox("Вы уверены что хотите закрыть текущее приложение?", () => CloseOpened(appSceneName), null, "Подтвердите действие", "Закрыть", "Отмена", GameManager.Instance.WarningColor);
        }
        else
        {
            await OpenedCloseCheckCallback(appSceneName);
        }
    }

    public async UniTask OpenAppByState(AppState state, bool askClose)
    {
        if(GameManager.Instance.CurrentAppState == state)
        {
            return;
        }

        var def = definitions.FirstOrDefault(d => d.state == state);

        if(def != null)
        {
            if(def.state == AppState.MAIN_MAP)
            {
                if(askClose)
                {
                    CloseCurrent();
                }
                else
                {
                    await CloseCurrentHandle();
                }
            }
            else
            {
                await Open(def.sceneName);
            }
        }
    }
    
    private async UniTask CloseOpened(string appSceneName)
    {
        unloadCurrent = true;
        await OpenedCloseCheckCallback(appSceneName);
    }

    private async UniTask OpenedCloseCheckCallback(string appSceneName)
    {
        OnAppOpening.Invoke();

        await Fader.In(fadeInDuration);
        await LoaderScreen.ShowAsync();

        if (unloadCurrent && _currentOpenedAppSceneName.IsValuable())
        {
            await Traveler.UnloadScene(_currentOpenedAppSceneName);
            OnAppClosed.Invoke();
        }

        Messager.Instance.Send<ClearFilterMessage>();
        Messager.Instance.Send<CloseAppMenuMessage>();

        _mapSceneHandler ??= await GetMainMap();

        _mapSceneHandler.HideMap();

        await Traveler.LoadScene(appSceneName);

        _currentOpenedAppSceneName = appSceneName;

        await LoaderScreen.HideAsync();
        await Fader.Out(fadeOutDuration);
    }

    public void CloseCurrent()
    {
        Info.Instance.ShowBox("Вы уверены что хотите закрыть текущее приложение?", () => CloseCurrentHandle(), null, "Подтвердите действие", "Закрыть", "Отмена", GameManager.Instance.WarningColor);
    }

    private async UniTask CloseCurrentHandle()
    {
        Messager.Instance.Send<CloseAppMenuMessage>();
        await Fader.In(fadeInDuration);
        await LoaderScreen.ShowAsync();

        try
        {
            await Traveler.UnloadScene(_currentOpenedAppSceneName);
        }
        catch
        {
            print("Empty object, returning to map!");
            UnloadScene("ViktorinaScene");
            UnloadScene("Gos");
            UnloadScene("People");
            UnloadScene("PradznikiMainScene");
        }
        finally
        {
            var args = ScriptableObject.CreateInstance<MainMapSceneArgs>();
            args.launchStartAnimation = false;

            _mapSceneHandler ??= await GetMainMap();

            _mapSceneHandler.ShowMap();
            _currentOpenedAppSceneName = string.Empty;
            GameManager.Instance.SetAppState(AppState.MAIN_MAP);
            AudioPlayer.Instance.PlayMusic("mapMusic", 0.05f, true);
            await LoaderScreen.HideAsync();
            await Fader.Out(fadeInDuration);
            OnAppClosed.Invoke();
        }
    }
    
    private void UnloadScene(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded) {
            SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}

[Serializable]
public class OpenAppMessage: Message
{
    public string appSceneName;
}

[Serializable]
public class CloseCurrentAppMessage: Message
{

}

[Serializable]
public class AppStateDefinition
{
    public string sceneName;
    public AppState state;
}
