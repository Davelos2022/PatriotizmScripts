using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class MainMapSceneHandler: SceneHandler<MainMapSceneArgs>
{
    [SerializeField] private CanvasGroupFader loadingCanvas;
    [SerializeField] private Animator loadingAnimator;
    [SerializeField] private VideoWrapper videoWrapper;
    [SerializeField] private SkeletonGraphic openingAnimationSkeleton;
    [SerializeField] private MapPointsContainer container;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField, Scene] private string editSceneName;
    [SerializeField, Scene] private string detailedViewSceneName;
    [SerializeField] private string openingAnimationName;
    [SerializeField] private string pointDataViewSceneName;

    private Color _transparent = new Color(1, 1, 1, 0);

    public MapPointsContainer Container => container;
    public Canvas MainCanvas => mainCanvas;

    protected override void SetupScene(MainMapSceneArgs args)
    {
        if(Args == null || Args.launchStartAnimation)
        {
            openingAnimationSkeleton.color = Color.white;
        }
        else
        {
            openingAnimationSkeleton.color = _transparent;
        }

        Messager.Instance.Subscribe<ExtendedMapPointClickedMessage>(m => OnMapPointClick(m.point, m.preloaded));
        
        GameManager.Instance.SetAppState(AppState.MAIN_MAP);
        AudioPlayer.Instance.StopMusic();
        AudioPlayer.Instance.PlayMusic("mapMusic", 0.05f, true);
        
        SetupSceneAsync();
    }

    private async UniTask SetupSceneAsync()
    {
        await Traveler.LoadScene(detailedViewSceneName);
        await LoaderScreen.HideAsync(true);

        if (Args == null || Args.launchStartAnimation)
        {
            await LoaderScreen.HideAsync();
            await loadingCanvas.FadeInAsync();
            await Fader.Out(0.5f);
            await UniTask.Delay(800);
            loadingAnimator.SetTrigger("Load");
            await UniTask.Delay(3000);
        }
        else
        {
            await LoaderScreen.ShowAsync(false);
        }

        float progress = container.GetLoadingProgress();

        container.Resolve();
        await container.Initialize(false);

        while(progress < 1)
        {
            progress = container.GetLoadingProgress();

            await UniTask.Yield();
        }
        
        await LoaderScreen.HideAsync(false);

        await UniTask.Delay(200);

        if(Args == null || Args.launchStartAnimation)
        {
            openingAnimationSkeleton.AnimationState.SetAnimation(0, openingAnimationName, false);
        }

        await loadingCanvas.FadeOutAsync();

        await Fader.Out(0.5f);

        if(!GameManager.Instance.Settings.passedOnBoarding)
        {
            await UniTask.Delay(1000);
            Messager.Instance.Send<CloseAppMenuMessage>();
            await Traveler.LoadScene("OnBoarding");
        }
    }

    private async UniTask OnMapPointClick(MapPoint point, bool preloaded)
    {
        GameManager.Instance.IsUserPointOpened = point.Data.userPoint;
        //LoaderScreen.ShowAsync();
        Messager.Instance.Send<CloseAppMenuMessage>();
        DetailedView.Instance.Open(point, preloaded);
        //await Traveler.LoadScene(pointDataViewSceneName, args);
        //loader screen hides in point view scene, after loading point additional data
    }

    public void AddPoint(Vector2 pos)
    {
        AddPointAsync(pos);
    }

    private async UniTask AddPointAsync(Vector2 pos)
    {
        LoaderScreen.ShowAsync(false);
        var args = ScriptableObject.CreateInstance<DataEditingSceneArgs>();
        args.pointData = new MapPointData(true) { anchoredPositionX = pos.x, anchoredPositionY = pos.y, userPoint = true, showByDefault = false };
        args.pointData.name = "";
        args.pointData.description = "";

        var group = Traveler.TryGetSceneHandler<MainMapSceneHandler>().Container.CurrentGroup;

        if(group == Group.None)
        {
            group = Group.Animals;
        }

        args.pointData.group = group;
        args.pointData.SetDirty();
        args.container = Traveler.TryGetSceneHandler<MainMapSceneHandler>().container;
        args.editType = PointEditType.Point;
        args.isNew = true;
        await Traveler.LoadScene(editSceneName, args);
    }

    public void OnPointViewSceneClose()
    {
        Messager.Instance.Send<CloseAppMenuMessage>();
        container.SetFilter(container.CurrentGroup);
    }

    public void HideMap()
    {
        canvasGroup.alpha = 0;
        canvasGroup.SetInteractions(false);
        //mainCanvas.gameObject.SetActive(false);
    }

    public void ShowMap()
    {
        canvasGroup.alpha = 1;
        canvasGroup.SetInteractions(true);
        //mainCanvas.gameObject.SetActive(true);
    }
}


