//using System;
//using System.Collections;
//using System.Collections.Generic;
//using Cysharp.Threading.Tasks;
//using DG.Tweening;
//using SFB;
//using SolidUtilities;
//using TMPro;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.EventSystems;
//using UnityEngine.Networking;
//using UnityEngine.UI;
//using VolumeBox.Toolbox;
//using VolumeBox.Toolbox.UIInformer;

//public class PointEditBehaviour: MonoCached
//{
//    [SerializeField] private CanvasGroup editPanelCanvasGroup;
//    [SerializeField] private TMP_InputField nameTextField;
//    [SerializeField] private TMP_InputField descriptionTextField;
//    [SerializeField] private TMP_Dropdown filterDropdown;
//    [SerializeField] private TMP_Dropdown areaDropdown;
//    [SerializeField] private Image mapImage;
//    [SerializeField] private Image photoPreview;
//    [SerializeField] private Image icon;
//    [SerializeField] private Sprite defaultIconButtonView;
//    [SerializeField] private RectTransform pin;
//    [SerializeField] private MapPointsContainer pointsContainer;
//    [SerializeField] private Toggle showbyDefaultToggle;
//    [SerializeField] private Toggle wavyToggle;
//    [Space]
//    [SerializeField] private CanvasGroup addPointButton;
//    [SerializeField] private CanvasGroup addPointHint;

//    [Inject] private FileManager _file;

//    private MapPointData _currentPoint;
//    private MapPointData _currentEditingPoint;

//    public UnityEvent<MapPointData> pointSaveEvent;
//    public UnityEvent pointChangeStart;
//    public UnityEvent pointChangeEnd;


//    protected override void Rise()
//    {
//        if (mapImage != null)
//        {
//            mapImage.raycastTarget = false;
//        }

//        _currentEditingPoint = new MapPointData();
//    }

//    public void ClickHandler(BaseEventData data)
//    {
//        PointerEventData p = data as PointerEventData;

//        Vector2 pos = ScreenUtils.PointerPositionToCanvas(p.position);
//        addPointHint.DOFade(0, 0.2f);

//        _currentEditingPoint.anchoredPositionX = pos.x;
//        _currentEditingPoint.anchoredPositionY = pos.y;

//        mapImage.raycastTarget = false;

//        pointChangeEnd.Invoke();

//        OnPosChanged(pos);
//    }

//    public void AddNewPoint()
//    {
//        //TODO: CheckPassword.Instance.PasswordPanel(AddNewPointCallback);
//        AddNewPointCallback();
//    }

//    private async void OnPosChanged(Vector2 newPos)
//    {
//        await SetPinTo(newPos);
//        SetFields(_currentEditingPoint);
//        FadeInEditPanel();
//    }

//    private async void AddNewPointCallback()
//    {
//        pointsContainer.DisablePointInteractions();
//        _currentPoint = new MapPointData();
//        _currentEditingPoint = new MapPointData();
//        _currentEditingPoint.CopyFrom(_currentPoint);

//        addPointButton.SetInteractions(false);
//        await addPointButton.DOFade(0, 0.2f);
//        addPointHint.DOFade(1, 0.2f);

//        //waiting user input
//        mapImage.raycastTarget = true;
//        pointChangeStart.Invoke();
//    }

//    private async UniTask SetPinTo(Vector2 position)
//    {
//        pin.gameObject.SetActive(true);
//        pin.anchoredPosition = position;
//        pin.localScale = Vector3.one * 5;
//        await pin.DOScale(Vector3.one, 0.3f).SetEase(Ease.InQuad);
//    }

//    public void Edit(MapPointData data)
//    {
//        Open(data);
//    }

//    private void Open(MapPointData data)
//    {
//        pointsContainer.DisablePointInteractions();
//        addPointHint.SetInteractions(false);
//        addPointHint.DOFade(0, 0.2f);
//        addPointButton.DOFade(0, 0.2f);
//        addPointButton.SetInteractions(false);

//        //Messager.Instance.Send<DisableScrollMessage>();

//        _currentPoint = data;

//        if (_currentEditingPoint == null)
//        {
//            _currentEditingPoint = new MapPointData();
//        }

//        _currentEditingPoint.CopyFrom(_currentPoint);

//        SetFields(_currentEditingPoint);

//        FadeInEditPanel();
//    }

//    private void FadeOutEditPanel()
//    {
//        editPanelCanvasGroup.DOFade(0, 0.3f);
//        editPanelCanvasGroup.SetInteractions(false);
//    }

//    private void FadeInEditPanel()
//    {
//        editPanelCanvasGroup.DOFade(1, 0.3f);
//        editPanelCanvasGroup.SetInteractions(true);
//    }

//    public async void SetFields(MapPointData data)
//    {
//        if (data != null)
//        {
//            await data.LoadDetailedData();
//        }

//        photoPreview.sprite = null;

//        if (data.photos != null && data.photos.Count > 0)
//        {
//            photoPreview.sprite = data.photos[0];
//        }

//        if (data.icon == null)
//        {
//            icon.sprite = defaultIconButtonView;
//        }
//        else
//        {
//            icon.sprite = data.icon;
//        }

//        filterDropdown.value = (int)data.group - 1;
//        showbyDefaultToggle.isOn = data.showByDefault;
//        wavyToggle.isOn = data.wavy;
//        nameTextField.text = data.name;
//        descriptionTextField.text = data.description;
//    }

//    public void Close()
//    {
//        Info.Instance.ShowBox("Вы действительно хотите закрыть окно редактирования? Несохраненные изменения будут утеряны.", OnClose);
//    }

//    private void OnClose()
//    {
//        photoPreview.sprite = null;
//        pointsContainer.EnablePointInteractions();
//        _currentPoint?.DestroyDetailedData();
//        _currentEditingPoint?.DestroyDetailedData();
//        _currentPoint = new MapPointData(); //clears data
//        addPointHint.DOFade(0, 0.2f);
//        addPointHint.SetInteractions(false);
//        addPointButton.DOFade(1, 0.2f);
//        addPointButton.SetInteractions(true);
//        //Messager.Instance.Send<EnableScrollMessage>();
//        pin.gameObject.SetActive(false);
//        FadeOutEditPanel();
//    }

//    public void DeletePoint()
//    {
//        Info.Instance.ShowBox("Вы действительно хотите удалить точку?", () =>
//        {
//            Messager.Instance.Send(new DeleteMapPointMessage { point = _currentPoint });
//            _currentEditingPoint = null;
//            OnClose();
//        });
//    }

//    public void Save()
//    {
//        if (!CheckFields()) return;

//        Info.Instance.ShowBox("Вы уверены что хотите сохранить точку?", OnSave);
//    }

//    private void OnSave()
//    {
//        ResolveSave();
//        OnClose();
//    }

//    private bool CheckFields()
//    {
//        if (!nameTextField.text.IsValuable() && _currentEditingPoint.icon == null)
//        {
//            Info.Instance.ShowHint("Назовите точку или выберите иконку");
//            return false;
//        }

//        if (!descriptionTextField.text.IsValuable())
//        {
//            Info.Instance.ShowHint("Опишите точку");
//            return false;
//        }

//        return true;
//    }

//    public async void ChangePoint()
//    {
//        pointsContainer.DisablePointInteractions();
//        pin.gameObject.SetActive(false);
//        FadeOutEditPanel();
//        addPointHint.DOFade(1, 0.2f);

//        //waiting user input
//        mapImage.raycastTarget = true;
//        pointChangeStart.Invoke();
//    }

//    private void ResolveSave()
//    {
//        _currentPoint.CopyFrom(_currentEditingPoint);

//        //TODO: TEMP
//        _currentPoint.showByDefault = showbyDefaultToggle.isOn;
//        _currentPoint.wavy = wavyToggle.isOn;

//        _currentPoint.name = nameTextField.text;
//        _currentPoint.description = descriptionTextField.text;
//        _currentPoint.group = (Group)filterDropdown.value + 1;

//        //saving audio
//        if (_currentPoint.audioDescriptionPath.IsValuable())
//        {
//            string audioPath = "User_Map_Points/point_" + _currentPoint.UID + "/audio.wav";
//            _file.CopyFileTo(_currentPoint.audioDescriptionPath, audioPath);
//            _currentPoint.audioDescriptionPath = audioPath;
//        }

//        pointSaveEvent.Invoke(_currentPoint);

//        Info.Instance.ShowHint("Сохранено!");
//    }

//    public async void OpenPhotoBrowser()
//    {
//        if (FileManager.SelectImageInBrowser(out string path))
//        {
//            //TODO: change sprite loading
//            Sprite s = FileManager.CreateSprite(await FileManager.LoadTextureAsync(path));
//            _currentEditingPoint.photos.Add(s);
//        }
//    }

//    public async UniTask OpenIconBrowser()
//    {
//        if (FileManager.SelectImageInBrowser(out string path))
//        {
//            Sprite s = FileManager.CreateSprite(await FileManager.LoadTextureAsync(path));
//            _currentEditingPoint.icon = s;
//            icon.sprite = s;
//        }
//    }

//    public void OpenAudioBrowser()
//    {
//        LoadAudioCoroutine();
//    }

//    public void DeleteCurrentPhoto()
//    {
//        if (_currentEditingPoint.photos.Count <= 0)
//        {
//            return;
//        }

//        _currentEditingPoint.photos.Remove(photoPreview.sprite);

//        if (_currentEditingPoint.photos.Count > 0)
//        {
//            photoPreview.sprite = _currentEditingPoint.photos[0];
//        }
//        else
//        {
//            photoPreview.sprite = null;
//        }
//    }

//    public void OpenNextPreview()
//    {
//        if (_currentEditingPoint.photos.Count < 2) return;

//        int nextIndex = _currentEditingPoint.photos.IndexOf(photoPreview.sprite) + 1;

//        nextIndex = Mathf.Clamp(nextIndex, 0, _currentEditingPoint.photos.Count - 1);

//        photoPreview.sprite = _currentEditingPoint.photos[nextIndex];
//    }

//    public void OpenPrevPreview()
//    {
//        if (_currentEditingPoint.photos.Count < 2) return;

//        int nextIndex = _currentEditingPoint.photos.IndexOf(photoPreview.sprite) - 1;

//        nextIndex = Mathf.Clamp(nextIndex, 0, _currentEditingPoint.photos.Count - 1);

//        photoPreview.sprite = _currentEditingPoint.photos[nextIndex];
//    }

//    private async UniTask LoadAudioCoroutine()
//    {
//        if (FileManager.SelectAudioInBrowser(out string path))
//        {
//            _currentEditingPoint.clip = await _file.LoadAudio(path, false);
//        }
//    }
//}
