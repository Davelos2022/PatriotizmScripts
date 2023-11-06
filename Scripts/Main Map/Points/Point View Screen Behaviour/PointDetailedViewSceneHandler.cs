using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class PointDetailedViewSceneHandler: SceneHandler<PointDetailedViewSceneArgs>
{
    [SerializeField] private string editSceneName;
    [SerializeField] private float fullscreenTweenDuration;
    [Space]
    [SerializeField] private float openDuration;
    [SerializeField] private Vector2 pointOffset;
    [SerializeField] private RectTransform pointViewScreenRoot;
    [SerializeField] private PointViewScreen viewScreen;
    [SerializeField] private CanvasGroup mainGroup;
    [Space]
    [SerializeField] private string categoryPreviewObjectTag;
    [SerializeField] private string categoryAddObjectTag;
    [SerializeField] private string usualPhotoPreviewTag;
    [Space]
    [SerializeField] private ScrollableLayoutHandler scrollableLayout;
    [SerializeField] private PointMiniMap miniMap;
    [Space]
    [SerializeField] private Vector2 minimizedImageSize;
    [SerializeField] private Vector2 maximizedImageSize;
    [Space]
    [SerializeField] private RectTransform imageViewRect;
    [SerializeField] private RectTransform imageContainerRect;
    [Space]
    [SerializeField] private ImagePreviewer imagePreviewer;
    [SerializeField] private RectTransform bottomPanel;
    [SerializeField] private RectTransform bottomBackground;
    [SerializeField] private RectTransform sidePanel;
    [SerializeField] private AudioSource audioDescriptionSource;
    [SerializeField] private ToggleButton audioToggleButton;
    [SerializeField] private GameObject playButtonPanel;
    [SerializeField] private GradientComponent gradientComponent;
    [Space]
    [SerializeField] private Image groupDotBackground;
    [SerializeField] private TMP_Text groupText;

    private MapPoint _currentPoint;
    private retere _currentCategoryView;
    private AlbumData _currentAlbumView;

    public UnityEvent<Gradient> GroupGradientFetched;
    public UnityEvent<List<Texture2D>> PointPhotosOpenEvent;
    public UnityEvent<MapPointData> PointDataUpdate;
    [Space]
    public UnityEvent OnFullscreenOpen;
    public UnityEvent OnFullscreenClose;
    [Space]
    public UnityEvent<string> GroupNameFetched;
    public UnityEvent<string> PointNameFetched;
    public UnityEvent<string> PointDescriptionFetched;
    [Space]
    public UnityEvent<Texture2D> PointIconFetched;
    public UnityEvent PointLoaded;
    public UnityEvent Opened;

    private Subscriber _refreshViewSub;
    private MapPointsContainer _container;

    private string _currentPointUID;

    private bool _isFullscreen;

    public string CachedAlbumToOpenUID;
    private bool openingAlbum = false;

    public bool ScrollToEnd { get; set; }

    protected override async void SetupScene(PointDetailedViewSceneArgs args)
    {
        _container = Traveler.TryGetSceneHandler<MainMapSceneHandler>().Container;

        Messager.Instance.Subscribe<CategoryOpenMessage>(m => OpenCategory(m.category), gameObject.scene.name);
        Messager.Instance.Subscribe<AlbumOpenMessage>(m => OpenAlbum(m.album), gameObject.scene.name);
        Messager.Instance.Subscribe<AlbumCloseMessage>(m => OnAlbumClose(m.category), gameObject.scene.name);
        Messager.Instance.Subscribe<CreateCategoryMessage>(m => CreateCategory(m.relatedPoint));
        Messager.Instance.Subscribe<CreateAlbumMessage>(m => CreateAlbum(m.RelatedPoint, m.RelatedCategory), gameObject.scene.name);
        Messager.Instance.Subscribe<CategoryEditMessage>(m => OpenCategoryEdit(m.category), gameObject.scene.name);
        Messager.Instance.Subscribe<AlbumEditMessage>(m => OpenAlbumEdit(m.album, m.relatedCategory), gameObject.scene.name);
        Messager.Instance.Subscribe<DataEditSceneCloseMessage>(m => RefreshView(m.albumData), gameObject.scene.name);
        Messager.Instance.Subscribe<CloseDetailedViewMessage>(_ => CloseView(), gameObject.scene.name);

        pointViewScreenRoot.localScale = Vector3.zero;
    }

    public async UniTask Open(MapPoint point, bool preloaded)
    {
        _currentPoint = point;
        _currentPointUID = point.Data.UID;
        _currentCategoryView = null;
        _currentAlbumView = null;


        UpdateGroupInfo(_currentPoint.Data);

        //await LoaderScreen.ShowAsync(false);

        pointViewScreenRoot.localScale = Vector3.one * 0.001f;
        await OpenPointAsync(preloaded);
        var pointPos = new Vector2(_currentPoint.Data.anchoredPositionX, _currentPoint.Data.anchoredPositionY) + pointOffset;
        Vector2 normalizedPointPos = new Vector2(pointPos.x / maximizedImageSize.x, pointPos.y / maximizedImageSize.y);
        pointViewScreenRoot.SetPivot(normalizedPointPos);
        pointViewScreenRoot.anchoredPosition = pointPos;
        pointViewScreenRoot.localScale = Vector3.zero;
        CloseFullScreenInstant();
        await pointViewScreenRoot.DOScale(Vector3.one, openDuration).SetEase(Ease.OutQuad);

        Messager.Instance.Send(new OpenImagePreviewByIndexMessage { index = 0 });

        Messager.Instance.Send<UpdateEditableSwitchersMessage>();
        Messager.Instance.Send(new SetPointClickAllowStateMessage { state = false });
        
        Opened.Invoke();

        Canvas.ForceUpdateCanvases();
    }

    public void PlayDescription()
    {
        audioDescriptionSource.Play();
    }

    public void PauseDescription()
    {
        audioDescriptionSource.Pause();
    }

    public void StopDescription()
    {
        audioDescriptionSource.Stop();
    }

    public async UniTask CreateCategory(MapPointData relatedPoint)
    {
        LoaderScreen.ShowAsync(false);

        var editArgs = ScriptableObject.CreateInstance<DataEditingSceneArgs>();
        editArgs.pointData = relatedPoint;
        editArgs.container = Traveler.TryGetSceneHandler<MainMapSceneHandler>().Container;
        editArgs.editType = PointEditType.Category;
        editArgs.isNew = true;
        Traveler.LoadScene(editSceneName, editArgs);

        LoaderScreen.HideAsync(false);
    }

    public async UniTask CreateAlbum(MapPointData relatedPoint, retere relatedCategory)
    {
        LoaderScreen.ShowAsync(false);

        var editArgs = ScriptableObject.CreateInstance<DataEditingSceneArgs>();
        editArgs.container = Traveler.TryGetSceneHandler<MainMapSceneHandler>().Container;
        editArgs.pointData = relatedPoint;
        editArgs.category = relatedCategory;
        editArgs.editType = PointEditType.Album;
        editArgs.isNew = true;
        Traveler.LoadScene(editSceneName, editArgs);

        LoaderScreen.HideAsync(false);
    }

    public async UniTask OpenCategory(retere category)
    {
        if (category == _currentCategoryView) return;

        audioToggleButton.SetState(false);

        await LoaderScreen.ShowAsync(false);

        if(_currentCategoryView != null)
        {
            await _currentCategoryView.DestroyData();
        }

        if(_currentAlbumView != null)
        {
            await _currentAlbumView.DestroyData();
        }

        if(_currentPoint != null)
        {
            await _currentPoint.Data.DestroyClipResource();
            await _currentPoint.Data.DestroyPhotoResources();
        }

        await category.LoadData();
        RefreshCategoryView(category);
        _currentCategoryView = category;
        _currentAlbumView = null;

        await LoaderScreen.HideAsync(false);
    }

    private void RefreshCategoryView(retere category)
    {
        _currentCategoryView = category;
        PointDescriptionFetched.Invoke(category.description);
        PointNameFetched.Invoke(category.name);
        PointPhotosOpenEvent.Invoke(new List<Texture2D>() { category.cover });
    }

    public async UniTask OpenAlbum(AlbumData album) 
    {
        openingAlbum = true;

        audioToggleButton.SetState(false);

        await LoaderScreen.ShowAsync(false);
        
        if(_currentAlbumView != null)
        {
            await _currentAlbumView.DestroyData();
        }

        if(_currentCategoryView != null)
        {
            await _currentCategoryView.DestroyData();
        }

        if(_currentPoint != null)
        {
            await _currentPoint.Data.DestroyPhotoResources();
            await _currentPoint.Data.DestroyClipResource();
        }

        await album.LoadData();

        _currentAlbumView = album;
        playButtonPanel.SetActive(_currentAlbumView.audio != null);
        audioToggleButton.SetState(false);
        audioDescriptionSource.clip = _currentAlbumView.audio;
        _currentCategoryView = null;
        PointPhotosOpenEvent.Invoke(album.photos.Where(x => x != null).ToList());
        PointDescriptionFetched.Invoke(album.description);
        //PointNameFetched.Invoke(album.name);
        await UniTask.Delay(50);
        //scrollableLayout.ScrollTo(Vector2.left);

        await LoaderScreen.HideAsync(false);
    }

    private async UniTask OnAlbumClose(retere category)
    {
        await RefreshView();

        scrollableLayout.Content.ForEach(c =>
        {
            if(c.TryGetComponent(out CategoryPreview ctg))
            {
                if(ctg.CurrentCategory.UID == category.UID)
                {
                    ctg.Click();
                }
            }
        });
    }

    public void ToggleFullscreen()
    {
        _isFullscreen = !_isFullscreen;

        if(_isFullscreen)
        {
            OpenFullScreenAsync();
        }
        else
        {
            CloseFullScreenAsync();
        }
    }

    public async UniTask OpenFullScreenAsync()
    {
        OnFullscreenOpen.Invoke();
        mainGroup.interactable = false;
        imageContainerRect.DOSizeDelta(maximizedImageSize, fullscreenTweenDuration);
        bottomPanel.DOAnchorPosY(-bottomPanel.rect.height, fullscreenTweenDuration);
        bottomBackground.DOSizeDelta(new Vector2(bottomBackground.sizeDelta.x, 280), fullscreenTweenDuration);
        sidePanel.DOAnchorPosX(sidePanel.rect.width, fullscreenTweenDuration);
        await UniTask.Delay(TimeSpan.FromSeconds(fullscreenTweenDuration));
        mainGroup.interactable = true;
        //Messager.Instance.Send<ReleaseResizeRawImageMessage>();
    }

    public async UniTask CloseFullScreenAsync()
    {
        OnFullscreenClose.Invoke();
        mainGroup.interactable = false;
        //imageViewRect.SetAnchors(Vector2.zero, Vector2.one);
        imageContainerRect.DOSizeDelta(minimizedImageSize, fullscreenTweenDuration);
        bottomPanel.DOAnchorPosY(0, fullscreenTweenDuration);
        sidePanel.DOAnchorPosX(0, fullscreenTweenDuration);
        bottomBackground.DOSizeDelta(new Vector2(bottomBackground.sizeDelta.x, 330), fullscreenTweenDuration);
        await UniTask.Delay(TimeSpan.FromSeconds(fullscreenTweenDuration));
        mainGroup.interactable = true;
        //Messager.Instance.Send<ReleaseResizeRawImageMessage>();
    }

    public void CloseFullScreenInstant()
    {
        OnFullscreenClose.Invoke();
        imageContainerRect.sizeDelta = minimizedImageSize;
        bottomPanel.anchoredPosition = new Vector2(bottomPanel.anchoredPosition.x, 0);
        sidePanel.anchoredPosition = new Vector2(0, sidePanel.anchoredPosition.y);
        bottomBackground.sizeDelta = new Vector2(bottomBackground.sizeDelta.x, 330);
        _isFullscreen = false;
    }

    public void OpenPointWrapper()
    {
        OpenPointAsync(false);
    }

    private async UniTask OpenPointAsync(bool preloaded)
    {
        if(!preloaded)
        {
            await LoaderScreen.ShowAsync(false);

            if (_currentPoint == null)
            {
                await LoaderScreen.HideAsync(false);
                return;
            }

            await _currentPoint.Data.LoadClip();
        }

        await _currentPoint.Data.LoadPhotos();

        playButtonPanel.SetActive(_currentPoint.Data.clip != null);
        
        audioToggleButton.SetState(false);
        audioDescriptionSource.clip = _currentPoint.Data.clip;

        viewScreen.OpenPoint(_currentPoint.Data);
        _currentAlbumView = null;
        _currentCategoryView = null;

        UpdateGroupInfo(_currentPoint.Data);
        
        PointLoaded.Invoke();

        await RefreshPoint(_currentPoint.Data);

        await LoaderScreen.HideAsync(false);
    }

    private void UpdateGroupInfo(MapPointData point)
    {
        if(point.group == Group.Other)
        {
            GroupNameFetched.Invoke(GameManager.Instance.Settings.otherGroupName);
        }
        else
        {
            GroupNameFetched.Invoke(GroupInfoContainer.GetInfo(point.group).name);
        }

        groupDotBackground.sprite = GroupInfoContainer.GetInfo(point.group).backgroundDot;
        groupText.color = GroupInfoContainer.GetInfo(point.group).fontColor;
        var gr = GroupInfoContainer.GetGradient(point.group);
        gradientComponent.SetGradient(gr);
    }

    private async UniTask RefreshPoint(MapPointData point)
    {
        //await point.LoadDetailedData();
        
        if(point.photos != null && point.photos.Count > 0)
        {
            PointPhotosOpenEvent.Invoke(point.photos.Where(x => x != null).ToList());
        }

        scrollableLayout.Clear();

        PointIconFetched.Invoke(point.icon);
        miniMap.UpdatePoint(point);

        if(GameManager.Instance.IsAdminMode && point.userPoint)
        {
            if (point.categories.Count > 0)
            {
                scrollableLayout.SetData(point.categories.ConvertAll(x => new ScrollableLayoutObjectData { data = x, objectTag = categoryPreviewObjectTag }));
            }

            scrollableLayout.AddObject(new ScrollableLayoutObjectData { objectTag = categoryAddObjectTag, data = _currentPoint.Data }).transform.SetAsFirstSibling();
        }
        else
        {
            if (point.categories.Count > 0)
            {
                scrollableLayout.SetData(point.categories.ConvertAll(x => new ScrollableLayoutObjectData { data = x, objectTag = categoryPreviewObjectTag }));
            }
            else
            {
                if (point.photos.Count > 0)
                {
                    scrollableLayout.SetData(point.photos.Where(x => x != null).ToList().ConvertAll(x => new ScrollableLayoutObjectData { data = x, objectTag = usualPhotoPreviewTag }));
                }
            }
        }


        PointDataUpdate.Invoke(point);
        PointNameFetched.Invoke(point.name);
        PointDescriptionFetched.Invoke(point.description);

        await UniTask.Delay(200);

        if(ScrollToEnd)
        {
            scrollableLayout.ScrollTo(Vector2.right);

            ScrollToEnd = false;
        }
        else
        {
            scrollableLayout.ScrollTo(Vector2.left);
        }
    }

    public void OpenPointEditWrapper()
    {
        OpenPointEdit();
    }

    public async UniTask OpenPointEdit()
    { 
        var editArgs = ScriptableObject.CreateInstance<DataEditingSceneArgs>();
        editArgs.container = Traveler.TryGetSceneHandler<MainMapSceneHandler>().Container;
        editArgs.editType = PointEditType.Point;
        editArgs.pointData = _currentPoint.Data;

        LoaderScreen.ShowAsync(false);
        await Traveler.LoadScene(editSceneName, editArgs);
    }

    public async UniTask OpenCategoryEdit(retere category)
    {
        var editArgs = ScriptableObject.CreateInstance<DataEditingSceneArgs>();
        editArgs.container = Traveler.TryGetSceneHandler<MainMapSceneHandler>().Container;
        editArgs.category = category;
        editArgs.editType = PointEditType.Category;
        editArgs.pointData = _currentPoint.Data;

        LoaderScreen.ShowAsync(false);
        await Traveler.LoadScene(editSceneName, editArgs);
    }

    public async UniTask OpenAlbumEdit(AlbumData album, retere relatedCategory)
    {
        var editArgs = ScriptableObject.CreateInstance<DataEditingSceneArgs>();
        editArgs.container = Traveler.TryGetSceneHandler<MainMapSceneHandler>().Container;
        editArgs.album = album;
        editArgs.category = relatedCategory;
        editArgs.editType = PointEditType.Album;
        editArgs.pointData = _currentPoint.Data;

        LoaderScreen.ShowAsync(false);
        await Traveler.LoadScene(editSceneName, editArgs);
    }

    public void CloseView()
    {
        CloseViewOrderAsync();
    }

    async UniTask CloseViewOrderAsync()
    {
        await CloseViewAsync();
        GameManager.Instance.IsUserPointOpened = false;
        Messager.Instance.Send<UpdateEditableSwitchersMessage>();
    }

    private async UniTask CloseViewAsync()
    {
        audioDescriptionSource.Stop();
        audioDescriptionSource.clip = null;

        if(_currentPoint != null)
        {
            await _currentPoint.Data.DestroyPhotoResources();
            await _currentPoint.Data.DestroyClipResource();

            foreach (var category in _currentPoint.Data.categories)
            {
                await category.DestroyData();

                foreach (var album in category.albums)
                {
                    await album.DestroyData();
                }
            }
        }

        //if(_currentCategoryView != null)
        //{
        //    await _currentCategoryView.DestroyData();
        //}

        //if(_currentAlbumView != null)
        //{
        //    await _currentAlbumView.DestroyData();
        //}

        await pointViewScreenRoot.DOScale(Vector3.zero, openDuration).SetEase(Ease.OutQuad);

        PointIconFetched.Invoke(null);
        PointNameFetched.Invoke(null);
        PointDescriptionFetched.Invoke(null);
        scrollableLayout.Clear();
        
        _currentCategoryView = null;
        _currentAlbumView = null;
        Traveler.TryGetSceneHandler<MainMapSceneHandler>().OnPointViewSceneClose();
        Messager.Instance.Send(new SetPointClickAllowStateMessage { state = true });
    }

    public async UniTask RefreshView(AlbumData albumToOpen = null)
    {
        Messager.Instance.Send<CloseAlbumsMessage>();

        _currentPoint = _container.DrawedPoints.FirstOrDefault(x => x.Data.UID == _currentPointUID);

        await LoaderScreen.ShowAsync(false);

        //if(_currentCategoryView != null)
        //{
        //    await OpenCategory(_currentCategoryView);
        //}
        //else if(_currentAlbumView != null)
        //{
        //    await OpenAlbum(_currentAlbumView);
        //}
        //else
        //{
        //    await OpenPointAsync();
        //}
        await OpenPointAsync(false);

        if(albumToOpen != null)
        {
            await UniTask.Delay(150);

            foreach(var category in scrollableLayout.Content)
            {
                var view = category.GetComponent<CategoryPreview>();

                if(view != null && view.CurrentCategory.UID == albumToOpen.relatedCategory.UID)
                {
                    view.Click(true);

                    foreach(var album in view.AlbumLayout.Content)
                    {
                        var albumView = album.GetComponent<AlbumButton>();

                        if(albumView != null && albumView.CurrentAlbum.UID == albumToOpen.UID)
                        {
                            albumView.OpenAlbumView();
                            break;
                        }
                    }
                }
            }
        }

        await LoaderScreen.HideAsync(false);
    }
}

[Serializable]
public class CategoryOpenMessage: Message
{
    public retere category;
}

[Serializable]
public class AlbumOpenMessage: Message
{
    public AlbumData album;
}

[Serializable]
public class CategoryEditMessage: Message
{
    public retere category;
}

[Serializable]
public class AlbumEditMessage: Message
{
    public AlbumData album;
    public retere relatedCategory;
}

public class CreateAlbumMessage: Message 
{
    public MapPointData RelatedPoint;
    public retere RelatedCategory;
}

public class CreateCategoryMessage: Message
{
    public MapPointData relatedPoint;
}

public class OpenDataVisualMessage: Message
{
    public string categoryUID;
    public string albumUID;
}


public class OpenAlbumViewMessage: Message { }
public class OpenCategoryViewMessage: Message { }
public class OpenPointViewMessage: Message { }
public class CloseDetailedViewMessage: Message { }