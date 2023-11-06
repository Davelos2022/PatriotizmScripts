using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class ScrollableLayoutHandler : MonoCached
{
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private float scrollPushMultiplier = 1;
    [SerializeField] private float delay;

    private List<GameObject> _content = new List<GameObject>();

    public List<GameObject> Content => _content;

    private CancellationTokenSource _pushToken;

    private float _currentPushedPos;

    private float CurrentPushedScrollPos 
    {
        get
        {
            return _currentPushedPos;
        }
        set
        {
            _currentPushedPos = value;
            scroll.horizontalNormalizedPosition = _currentPushedPos;
        }
    }

    protected override void Rise()
    {
        Messager.Instance.Subscribe<PushScrollMessage>(m => PushTo(m.obj), gameObject.scene.name);
    }

    public void SetData(List<ScrollableLayoutObjectData> data)
    {
        Clear();

        foreach(ScrollableLayoutObjectData item in data) 
        {
            AddObject(item);
        }
    }

    public void ScrollTo(Vector2 value)
    {
        scroll.horizontalNormalizedPosition = value.x;
        scroll.verticalNormalizedPosition = value.y;
    }

    public GameObject AddObject(ScrollableLayoutObjectData objectData)
    {
        var obj = Pooler.Instance.Spawn(objectData.objectTag, Vector3.zero, Quaternion.identity, contentRoot, objectData.data, x => x.transform.localScale = Vector3.one);
        var p = obj.transform.localPosition;
        p.z = 0;
        obj.transform.localPosition = p;
        obj.transform.localScale = Vector3.one;
        _content.Add(obj);
        return obj;
    }

    /// <summary>
    /// Clears all content in layout
    /// </summary>
    [Button("Clear")]
    public void Clear()
    {
        //foreach(Transform child in contentRoot)
        //{
        //    Pooler.Instance.TryDespawn(child.gameObject);
        //}
        while (contentRoot.childCount > 0) 
        {
            var child = contentRoot.GetChild(0);

            if(child != null && child.gameObject != null)
            {
                if(Pooler.HasInstance)
                {
                    Pooler.Instance.TryDespawn(child.gameObject);
                }
            }
        }

        _content.Clear();
    }

    public void StopPush()
    {
        _pushToken?.Cancel();
        _pushToken?.Dispose();
        _pushToken = new CancellationTokenSource();
    }

    public async UniTask PushTo(Transform obj)
    {
        if (centerPoint == null) return;

        StopPush();

        await UniTask.Delay(TimeSpan.FromSeconds(delay)).AttachExternalCancellation(_pushToken.Token);

        _pushToken?.Cancel();
        _pushToken?.Dispose();
        _pushToken = new CancellationTokenSource();

        var diff = obj.position - centerPoint.position;

        diff /= 1920;

        diff *= scrollPushMultiplier;

        diff.x = scroll.horizontalNormalizedPosition + diff.x;

        CurrentPushedScrollPos = scroll.horizontalNormalizedPosition;

#pragma warning disable
        DOTween.To(
            () => CurrentPushedScrollPos,
            x => 
            {
                var pos = x;

                if (x > 1)
                {
                    pos = 1;
                }

                if (x < 0)
                {
                    pos = 0;
                }

                CurrentPushedScrollPos = pos;
            },
            diff.x,
            0.6f
            ).SetEase(Ease.OutCubic).WithCancellation(_pushToken.Token);
    }
#pragma warning enable
}

public class PushScrollMessage: Message
{
    public Transform obj;
}

public class ScrollableLayoutObjectData
{
    public string objectTag;
    public object data;
}
