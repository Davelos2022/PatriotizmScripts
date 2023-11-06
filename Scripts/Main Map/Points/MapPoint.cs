using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LeTai.TrueShadow;
using NaughtyAttributes;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class MapPoint: MonoCached, IPooled
{
    [SerializeField] private float initialWidth;
    [SerializeField] private float maxWidth;
    [SerializeField] private float additionalWidth;
    [SerializeField] private RectTransform namePanel;
    [Space]
    [SerializeField] private RectRebuilder rebuilder;
    [SerializeField] private RawImage icon;
    [SerializeField] private RectTransform centerPoint;
    [SerializeField] private RectTransform nameLabel;
    [SerializeField] private TextMeshProUGUI pointName;
    [SerializeField] private TMP_Text pointNameOverlay;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GradientComponent gradientComponent;
    [SerializeField] private RectTransform pointViewRoot;
    [SerializeField] private Image dragImage;
    [SerializeField] private float fadeDuration;
    [SerializeField] private float doubleClickDelay;
    [SerializeField] private float dragHoldDuration;
    [SerializeField] private TrueShadow shadow;
    [SerializeField] private MapPointData _currentData;

    public UnityEvent Clicked;
    public UnityEvent<bool> ShowText;
    public UnityEvent<Texture2D> IconSetEvent;

    public bool Visible { get; private set; } = true;
    public bool Loaded => _loaded;
    public MapPointData Data => _currentData;
    public RectTransform CenterPoint => centerPoint;
    public Gradient CurrentGradient => _currentGradient;
    public RawImage Icon => icon;
    public RectTransform LabelRect => nameLabel;

    private ExtendedMapPointClickedMessage _extendedPointClickedMessage;
    private MapPointClickedMessage _pointClickedMessage;
    private bool _loaded;
    private Gradient _currentGradient;
    private Canvas _canvas;
    private bool _clickedOnce;
    private bool _clickedTwice;
    private Tween _shakeTween;
    private bool _prevFramePointerDown;
    private bool _pointerDown;
    private float _timer;
    private bool _dragging;
    private bool _canClick = true;

    [Inject] private FileManager _file;

    protected override void Rise()
    {
        Messager.Instance.Subscribe<SetPointClickAllowStateMessage>(m => _canClick = m.state, gameObject.scene.name);
        _shakeTween = pointViewRoot.DOShakePosition(1, 1, 10, 90, false, false, ShakeRandomnessMode.Harmonic).SetLoops(-1).SetAutoKill(false);
        _shakeTween.Pause();
    }

    public void OnSpawn(object data)
    {
        _extendedPointClickedMessage = new ExtendedMapPointClickedMessage { point = this };
        _pointClickedMessage = new MapPointClickedMessage { point = this };
        //icon.alphaHitTestMinimumThreshold = 0.1f;
        _currentData = data as MapPointData;
        _canvas = Traveler.TryGetSceneHandler<MainMapSceneHandler>()?.MainCanvas;
        transform.localScale = Vector3.one;
    }

    public void OnPointerDown()
    {
        _pointerDown = true;
    }

    public void OnPointerUp()
    {
        _pointerDown = false;
    }

    public void DisableInteractions()
    {
        canvasGroup.SetInteractions(false);
        transform.localScale = Vector3.one;
    }

    public void EnableInteractions()
    {
        canvasGroup.SetInteractions(true);
        transform.localScale = Vector3.one;
    }

    public void Hide()
    {
        Visible = false;
        canvasGroup.SetInteractions(false);
        canvasGroup.DOKill();
        canvasGroup.DOFade(0, fadeDuration);
        transform.localScale = Vector3.one;
    }

    public async void Show()
    {
        Visible = true;
        canvasGroup.SetInteractions(true);
        canvasGroup.DOKill();
        canvasGroup.DOFade(1, fadeDuration);
        transform.localScale = Vector3.one;
    }

    public async UniTask LoadCurrentData()
    {
        _loaded = false;

        if(_currentData == null)
        {
            _loaded = true;
            return;
        }

        await _currentData.LoadIcon();
        
        var gr = GroupInfoContainer.GetGradient(_currentData.group);

        if(gr != null)
        {
            _currentGradient = gr;
        }

        gradientComponent.SetGradient(gr);

        if (_currentData.icon == null)
        {
            icon.gameObject.SetActive(false);
        }
        else
        {
            IconSetEvent.Invoke(_currentData.icon);
        }

        ShowText.Invoke(_currentData.name.IsValuable());
        pointName.text = _currentData.name;
        pointNameOverlay.text = _currentData.name;
        var rectPos = new Vector2(_currentData.anchoredPositionX, _currentData.anchoredPositionY);
        Rect.anchoredPosition = rectPos;
        transform.localScale = Vector3.one;

        shadow.Color = GroupInfoContainer.GetInfo(_currentData.group).shadowColor;

        LayoutRebuilder.ForceRebuildLayoutImmediate(pointName.transform.parent as RectTransform);
        rebuilder.Rebuild();
        transform.localScale = Vector3.one;
        //_message = new OpenMapInfoMessage { Data = data };
        _loaded = true;

        await UniTask.WaitForEndOfFrame(this);
        float width = 0;

        for (int i = 0; i < pointNameOverlay.textInfo.characterCount; i++)
        {
            var bl = pointNameOverlay.textInfo.characterInfo[i].vertex_BL;
            var br = pointNameOverlay.textInfo.characterInfo[i].vertex_BR;

            var charW = Mathf.Abs(bl.position.x - br.position.x);

            width += charW;
        }

        width += initialWidth;

        width += additionalWidth;

        width = Mathf.Clamp(width, 0, maxWidth);

        namePanel.sizeDelta = new Vector2(width, namePanel.sizeDelta.y);
        await UniTask.WaitForEndOfFrame(this);
        pointNameOverlay.mesh.RecalculateBounds();
        width = pointNameOverlay.mesh.bounds.size.x + additionalWidth + initialWidth;
        namePanel.sizeDelta = new Vector2(width, namePanel.sizeDelta.y);
    }

    public void DragHandler(BaseEventData e)
    {
        Rect.anchoredPosition += (e as PointerEventData).delta / _canvas.scaleFactor;
    }

    public void OnClick()
    {
        if (!_canClick) return;

        OnClickAsync();
    }

    private async UniTask OnClickAsync()
    {
        transform.localScale = Vector3.one;
        transform.DOKill();
        transform.DOPunchScale(Vector3.one * 0.5f, 0.1f);

        Clicked.Invoke();

        if(_clickedOnce)
        {
            _clickedTwice = true;
            _extendedPointClickedMessage.preloaded = false;
            Messager.Instance.Send(_extendedPointClickedMessage);
            return;
        }

        _clickedOnce = true;

        await UniTask.Delay(TimeSpan.FromSeconds(doubleClickDelay));

        if(!_clickedTwice)
        {   
            Messager.Instance.Send(_pointClickedMessage);
        }

        _clickedTwice = false;
        _clickedOnce = false;
    }

    public void EnableDrag()
    {
        if (!Data.userPoint || !GameManager.Instance.IsAdminMode) return;

        Messager.Instance.Send<HideMenuMessage>();
        Messager.Instance.Send<DisablePopupOpenMessage>();
        _prevFramePointerDown = false;
        _pointerDown = false;
        _dragging = true;
        dragImage.raycastTarget = true;
        _shakeTween.Restart();
    }

    public void DisableDrag()
    {
        if (!Data.userPoint) return;

        Messager.Instance.Send<ShowMenuMessage>();
        Messager.Instance.Send<EnablePopupOpenMessage>();
        _prevFramePointerDown = false;
        _pointerDown = false;
        _dragging = false;
        dragImage.raycastTarget = false;
        _shakeTween.Pause();
        Rect.anchoredPosition = new Vector2(_currentData.anchoredPositionX, _currentData.anchoredPositionY);
    }
    
    public void UpdatePosition()
    {
        _currentData.anchoredPositionX = Rect.anchoredPosition.x;
        _currentData.anchoredPositionY = Rect.anchoredPosition.y;
        transform.localScale = Vector3.one;
    }

    protected override void Tick()
    {
        HandlePointer();
    }

    private void HandlePointer()
    {
        if (_dragging) return;

        if (_pointerDown)
        {
            _timer += delta;

            if (_timer >= dragHoldDuration)
            {
                _timer = 0;
                _prevFramePointerDown = false;
                Messager.Instance.Send<EnablePointDragMessage>();
            }
        }
        else if (_prevFramePointerDown)
        {
            _timer = 0;
            OnClick();
        }

        _prevFramePointerDown = _pointerDown;
    }

    private async UniTask OnEnableAsync()
    {
        if(_currentData != null)
        {
            await _currentData.LoadIcon();
        }

        if (_currentData.icon == null)
        {
            icon.gameObject.SetActive(false);
        }
        else
        {
            IconSetEvent.Invoke(_currentData.icon);
        }
    }
}

[Serializable]
public class ExtendedMapPointClickedMessage: Message
{
    public MapPoint point;
    public bool preloaded;
}

[Serializable]
public class MapPointClickedMessage: Message
{
    public MapPoint point;
}

public class EnablePointDragMessage: Message
{

}

public class DisablePointDragMessage: Message
{

}

[Serializable]
public class SetPointClickAllowStateMessage: Message
{
    public bool state;
}

