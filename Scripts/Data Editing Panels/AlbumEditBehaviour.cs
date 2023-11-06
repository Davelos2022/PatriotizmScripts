using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class AlbumEditBehaviour: PointEditBehaviourBase
{
    [SerializeField] private TMP_Text headerLabel;
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_InputField descriptionField;
    [SerializeField] private List<AddPhotoButton> photoButtons;
    [SerializeField] private AudioLoader audioLoader;
    [Space]
    [SerializeField] private ErrorHighlighter nameHighlight;
    [SerializeField] private ErrorHighlighter descriptionHighlight;
    [SerializeField] private ErrorHighlighter photoHighlighter;

    public UnityEvent<Texture2D> FirstImageLoaded;
    public UnityEvent AllImagesDeleted;

    protected override void Rise()
    {
        base.Rise();

        foreach (var photoButton in photoButtons)
        {
            photoButton.OnImageLoaded.AddListener(OnImageLoadedCallback);
            photoButton.OnImageDeleted.AddListener(OnImageDeletedCallback);
        }
    }

    protected override void OnActivate()
    {
        photoButtons.ForEach(b => b.Missclick.EnableCheck());
    }

    protected override void OnDeactivate()
    {
        photoButtons.ForEach(b => b.Missclick.DisableCheck());
    }

    private void OnImageLoadedCallback(Texture2D image)
    {
        CheckImages();
    }

    private void OnImageDeletedCallback()
    {
        CheckImages();
    }

    private void CheckImages()
    {
        var photo = photoButtons.FirstOrDefault(x => x.Path.IsValuable());

        if(photo is null)
        {
            AllImagesDeleted.Invoke();
        }
        else
        {
            FirstImageLoaded.Invoke(photo.LoadedImage);
        }
    }

    protected override string CheckFields()
    {
        if(!nameField.text.IsValuable())
        {
            nameHighlight.Highlight();
            return "Введите название альбома";
        }

        if(!descriptionField.text.IsValuable())
        {
            descriptionHighlight.Highlight();
            return "Введите описание альбома";
        }

        if(!photoButtons.Any(b => b.Path.IsValuable()))
        {
            photoHighlighter.Highlight();
            return "Загрузите хотя бы одну фотографию";
        }

        return null;
    }

    protected override async UniTask DeleteHandle()
    {
        await CurrentAlbum.DestroyData();
        CurrentCategory.albums.Remove(CurrentCategory.albums.FirstOrDefault(a => a.UID == CurrentAlbum.UID));
        await CurrentCategory.Save();
        Traveler.TryGetSceneHandler<MainMapSceneHandler>()?.Container.RefreshSave();
    }

    protected override async UniTask EditHandle(bool isNew)
    {
        if (CurrentAlbum == null)
        {
            headerLabel.text = "Создание альбома";
            CurrentAlbum = new(CurrentCategory);
            _handler.SetAlbumToOpen(CurrentAlbum);
        }
        else
        {
            headerLabel.text = "Редактирование альбома";

            var loadTasks = new List<UniTask>();

            await CurrentAlbum.LoadData();

            for (int i = 0; i < CurrentAlbum.photoPaths.Count; i++)
            {
                loadTasks.Add(photoButtons[i].LoadPhoto(CurrentAlbum.photoPaths[i], false));
            }

            await UniTask.WhenAll(loadTasks);
        }

        audioLoader.SetCurrent(CurrentAlbum.audioPath);

        nameField.text = CurrentAlbum.albumName;
        descriptionField.text = CurrentAlbum.description;

        photoButtons.ForEach(b => b.Missclick.EnableCheck());

        LoaderScreen.HideAsync(false);
    }

    protected override async UniTask PropagateEditedFields()
    {
        CurrentAlbum.albumName = nameField.text;
        CurrentAlbum.description = descriptionField.text;
        CurrentAlbum.photoPaths = photoButtons.ConvertAll(b => b.Path);
        CurrentAlbum.audioPath = audioLoader.LoadedPath;

        var existingAlbum = CurrentCategory.albums.FirstOrDefault(x => x.UID == CurrentAlbum.UID);

        if (existingAlbum == null)
        {
            CurrentCategory.albums.Add(CurrentAlbum);
        }

        CurrentAlbum.SetDirty();
    }

    protected override void SaveMessage()
    {
        Notifier.Instance.Notify(NotifyType.Success, "Альбом сохранен");
    }

    protected override string GetDeleteMessage()
    {
        return "Вы уверены что хотите удалить альбом?";
    }

    protected override bool CheckEdit()
    {
        var edited = nameField.text != CurrentAlbum.albumName ||
            descriptionField.text != CurrentAlbum.description;

        for (int i = 0; i < photoButtons.Count; i++)
        {
            edited = edited || CurrentAlbum.photoPaths[i] != photoButtons[i].Path;
        }

        edited = edited || audioLoader.LoadedPath != CurrentAlbum.audioPath;

        return edited;
    }
}
