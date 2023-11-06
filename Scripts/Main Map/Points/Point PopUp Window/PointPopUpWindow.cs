using AirFishLab.ScrollingList;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LeTai.TrueShadow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class PointPopUpWindow : MonoCached
{
    [SerializeField] private RectTransform center;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private RectTransform raycastRect;
    [SerializeField] private Transform pointer;
    [SerializeField] private TrueShadow pointerShadow;
    [SerializeField] private RectTransform pointerGraphics;
    [SerializeField] private Vector2 openedPointerSize;
    [SerializeField] private RectTransform window;
    [SerializeField] private Image background;
    [SerializeField] private float animDuration;
    [SerializeField] private Vector2 closedSize;
    [SerializeField] private Vector2 openedSize;
    [SerializeField] private Vector2 offset;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private CircularScrollingList scrollingList;
    [SerializeField] private PreviewListBank listBank;
    [SerializeField] private AudioSource descriptionAudioSource;
    [SerializeField] private GameObject playButton;
    [SerializeField] private CircularBoxSlider boxSlider;
    [SerializeField] private GameObject controlButtons;
    [SerializeField] private bool missclickOnOtherScene;
    [SerializeField] private Camera missclickTestCamera;

    public UnityEvent OpeningEvent;
    public UnityEvent ClosingEvent;
    public UnityEvent StartFetching;
    public UnityEvent StopFetching;
    public UnityEvent<Texture2D> IconFetchedEvent;

    private CancellationTokenSource _tokenSource;
    private MapPoint _currentPoint;
    private Subscriber _openSub;

    private bool _tryingToOpen;
    private bool _opened;
    private Vector3 _initialPos;
    private bool pointerDownInRectangle = false;
    private bool clickedOtherScene = false;

    private bool _canBeOpened = true;

    private Scene _preloadScene;

    private List<Texture2D> _miniatures = new List<Texture2D>();
    
    protected override void Rise()
    {
        _tokenSource = new CancellationTokenSource();
        _openSub = Messager.Instance.Subscribe<MapPointClickedMessage>(x => OpenAsync(x.point));
        background.material = Instantiate(background.material);
        _initialPos = transform.position;
        Messager.Instance.Subscribe<OpenPopUpExtendedMessage>(_ => OpenExtended(), gameObject.scene.name);
        Messager.Instance.Subscribe<EnablePopupOpenMessage>(_ => _canBeOpened = true, gameObject.scene.name);
        Messager.Instance.Subscribe<DisablePopupOpenMessage>(_ => _canBeOpened = false, gameObject.scene.name);
        Messager.Instance.Subscribe<ClosePopupWindowMessage>(_ => Close(), gameObject.scene.name);
        Messager.Instance.Subscribe<PopupEnableSceneMissclickMessage>(_ => missclickOnOtherScene = true, gameObject.scene.name);
        Messager.Instance.Subscribe<PopupDisableSceneMissclickMessage>(_ => missclickOnOtherScene = false, gameObject.scene.name);
    }

    public void Open()
    {
        OpenAsync(_currentPoint);
    }

    public async UniTask OpenAsync(MapPoint point)
    {
        if (point == _currentPoint || !_canBeOpened) return;

        playButton.SetActive(false);

        nameText.text = point.Data.name;
        descriptionText.text = point.Data.description;
        IconFetchedEvent.Invoke(null);

        _tryingToOpen = true;

        _tokenSource.Cancel();
        _tokenSource.Dispose();
        _tokenSource = new CancellationTokenSource();

        await CloseAsync();

        _currentPoint = point;

        OpeningEvent?.Invoke();

        ClampToWindow(point.transform.position, point.CenterPoint.position);
        Rect.sizeDelta = openedSize;
        ClampPointer();
        Rect.sizeDelta = closedSize;

        await Rect.DOSizeDelta(openedSize, animDuration).WithCancellation(_tokenSource.Token);
        await pointerGraphics.DOScale(Vector3.one, animDuration * 0.25f).WithCancellation(_tokenSource.Token);
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f)).AttachExternalCancellation(_tokenSource.Token);

        _opened = true;
        StartFetching.Invoke();

        _miniatures.Clear();

        foreach(var photo in point.Data.pointPhotosPath)
        {
            if(photo.miniaturePath.IsValuable())
            {
                _miniatures.Add(await FileManager.LoadTextureAsync(photo.miniaturePath, true, _tokenSource.Token));
            }
        }
        
        await point.Data.LoadClip();

        if(_tokenSource.Token.IsCancellationRequested)
        {
            return;
        }

        descriptionAudioSource.clip = point.Data.clip;

        playButton.SetActive(point.Data.clip != null);

        listBank.SetListContent(_miniatures.ConvertAll(x => new PreviewData { previewTexture = x }).ToArray());
        scrollingList.Initialize();
        scrollingList.Refresh();
        var count = scrollingList.ListBank.GetContentCount();
        scrollingList.SelectContentID(Mathf.CeilToInt((count * 0.5f) - 1));

        controlButtons.SetActive(point.Data.photos.Count > 1);

        StopFetching.Invoke();

        IconFetchedEvent.Invoke(point.Data.icon);

        boxSlider.UpdateContent();

        _tryingToOpen = false;
        ClampPointer();
        var p = pointer.transform.localPosition;
        p.z = 0;
        pointer.transform.localPosition = p;
    }

    public void PlayDescription()
    {
        descriptionAudioSource.Play();
    }

    public void PauseDescription()
    {
        descriptionAudioSource.Pause();
    }

    public void StopDescription()
    {
        descriptionAudioSource.Stop();
    }

    public void NextPreview()
    {
        var index = scrollingList.GetFocusingContentID();
        var count = scrollingList.ListBank.GetContentCount();
        scrollingList.SelectContentID(Mathf.Clamp(index + 1, 0, count - 1));
    }

    public void PreviousPreview()
    {
        var index = scrollingList.GetFocusingContentID();
        var count = scrollingList.ListBank.GetContentCount();
        scrollingList.SelectContentID(Mathf.Clamp(index - 1, 0, count - 1));
    }

    public void OpenExtended()
    {
        OpenExtendedAsync();
    }

    public async UniTask OpenExtendedAsync()
    {
        var pointToOpen = _currentPoint;
        await CloseAsync();
        Messager.Instance.Send(new ExtendedMapPointClickedMessage { point = pointToOpen, preloaded = true });
    }

    public void Close()
    {
        CloseAsync();
    }

    public async UniTask CloseAsync()
    {
        if (!_opened) return;

        descriptionAudioSource.Stop();

        if(_currentPoint != null)
        {
            //await _currentPoint.Data.DestroyPhotoResources();
            
            //Just in case window closed before data destroyed
            if (_currentPoint != null)
            {
                //await _currentPoint.Data.DestroyClipResource();
            }

            //await Resources.UnloadUnusedAssets();
        }

        _miniatures.ForEach(m => Destroy(m));
        
        _tokenSource.Cancel();
        _tokenSource.Dispose();
        _tokenSource = new CancellationTokenSource();

        ClosingEvent?.Invoke();
        pointerGraphics.localScale = Vector3.zero;
        await Rect.DOSizeDelta(closedSize, animDuration).WithCancellation(_tokenSource.Token);
        _opened = false;
        _currentPoint = null;
    }

    private void ClampPointer()
    {
        if (_currentPoint == null) return;

        var corners = new Vector3[4];
        var points = new Vector3[4];

        raycastRect.GetWorldCorners(corners);

        for (int i = 0; i < corners.Length; i++)
        {
            var nextPointIndex = i + 1 < corners.Length ? i + 1 : 0;

            var dir = corners[nextPointIndex] - corners[i];

            points[i] = MathUtils.GetClosestPointOnLineSegment(corners[i], corners[nextPointIndex], _currentPoint.CenterPoint.position);
        }

        int closestPoint = 0;

        for (int i = 0; i < points.Length; i++)
        {
            if(Vector3.Distance(_currentPoint.CenterPoint.position, points[i]) < Vector3.Distance(_currentPoint.CenterPoint.position, points[closestPoint]))
            {
                closestPoint = i;
            }
        }

        var pointerDir = _currentPoint.CenterPoint.position - points[closestPoint];

        var angle = Vector3.Angle(Vector3.up, pointerDir);

        if(angle > 45 && angle < 135)
        {
            if(_currentPoint.CenterPoint.position.x > points[closestPoint].x)
            {
                angle = -90;
            }
            else
            {
                angle = 90;
            }
        }
        else if(angle > 135)
        {
            angle = 180;
        }
        else
        {
            angle = 0;
        }

        var closest = points[closestPoint];
        closest.z = 0;

        pointer.rotation = Quaternion.Euler(0f, 0f, angle);
        pointerShadow.OffsetAngle = angle - 270;
        pointer.position = closest;
    }

    private void ClampToWindow(Vector2 pointAnchoredPosition, Vector3 pointPosition)
    {
        bool lower = pointAnchoredPosition.y < center.transform.position.y;
        bool left = pointAnchoredPosition.x < center.transform.position.x;

        var xPivot = left ? 0 : 1;
        var yPivot = lower ? 0 : 1;

        var pivotXDir = Mathf.Sign(xPivot - 0.5f);
        var pivotYDir = Mathf.Sign(yPivot - 0.5f);

        var offsetX = offset.x * pivotXDir;
        var offsetY = offset.y * pivotYDir;

        Rect.pivot = new Vector2(xPivot, yPivot);

        Vector3 finalPos = pointPosition;

        Rect.position = finalPos;
        Rect.anchoredPosition += new Vector2(offsetX, offsetY);
    }

    protected override void Destroyed()
    {
        Messager.Instance.RemoveSubscriber(_openSub);
    }

    //protected override void Tick()
    //{
    //    if(Input.GetMouseButtonDown(0))
    //    {
    //        var clickedObject = (EventSystem.current.currentInputModule as ExtendedInputModule).GameObjectUnderPointer();

    //        if (clickedObject != null && clickedObject.scene.name != gameObject.scene.name)
    //        {
    //            clickedOtherScene = true;
    //        }

    //        var vector = Input.mousePosition / mainCanvas.scaleFactor;

    //        if (RectTransformUtility.RectangleContainsScreenPoint(Rect, vector, missclickTestCamera))
    //        {
    //            pointerDownInRectangle = true;
    //        }
    //    }

    //    if(Input.GetMouseButtonUp(0))
    //    {
    //        var vector = Input.mousePosition / mainCanvas.scaleFactor;

    //        Debug.Log(vector);

    //        OnMouseUpCheck();
    //    }
    //}

    //private async UniTask OnMouseUpCheck()
    //{
    //    await UniTask.WaitForEndOfFrame();

    //    if (_tryingToOpen || pointerDownInRectangle || (clickedOtherScene && !missclickOnOtherScene))
    //    {
    //        clickedOtherScene = false;
    //        pointerDownInRectangle = false;
    //        return;
    //    }
    //    else
    //    {
    //        CloseAsync();
    //    }
    //}
}

public class OpenPopUpExtendedMessage: Message
{

}

public class EnablePopupOpenMessage: Message { }
public class DisablePopupOpenMessage: Message { }
public class ClosePopupWindowMessage: Message { }
public class PopupDisableSceneMissclickMessage: Message { }
public class PopupEnableSceneMissclickMessage: Message { }