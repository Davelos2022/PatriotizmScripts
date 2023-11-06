using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class DetailedView : CachedSingleton<DetailedView>
{
    [SerializeField] private PointDetailedViewSceneHandler sceneHandler;

    public UnityEvent OnOpen;
    public UnityEvent OnClose;

    public void Open(MapPoint point, bool preloaded)
    {
        sceneHandler.Open(point, preloaded);
        OnOpen.Invoke();
    }

    public void Close()
    {
        sceneHandler.CloseView();
        OnClose.Invoke();
    }
}
