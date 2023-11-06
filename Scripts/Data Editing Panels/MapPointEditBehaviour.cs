using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class MapPointEditBehaviour: PointEditBehaviourBase
{
    [SerializeField] private TMP_Text headerLabel;
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_InputField descriptionField;
    [SerializeField] private Toggle showByDefaultToggle;
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private List<AddPhotoButton> photoButtons;
    [SerializeField] private AudioLoader audioLoader;
    [Space]
    [SerializeField] private ErrorHighlighter nameHighlighter;
    [SerializeField] private ErrorHighlighter descriptionHighlighter;
    [SerializeField] private ErrorHighlighter photoHighlighter;

    public UnityEvent<Texture2D> IconFetchedEvent;

    protected override void Rise()
    {
        base.Rise();

        foreach(var button in photoButtons) 
        {
            button.SelectedAsIcon.AddListener(IconSelected);
            button.OnImageLoaded.AddListener(OnImageLoadedCallback);
            button.OnImageDeleted.AddListener(OnImageDeletedCallback);
        }
    }

    protected override void OnDeactivate() 
    {
        photoButtons.ForEach(b => b.Missclick.DisableCheck());
    }

    private void OnImageLoadedCallback(Texture2D texture)
    {
        CheckImages();
    }

    private void OnImageDeletedCallback()
    {
        CheckImages();
    }

    private void CheckImages()
    {
        if (!photoButtons.Any(b => b.IsIcon))
        {
            photoButtons[0].SelectAsIcon();
        }

        if (!photoButtons.Any(b => b.Path.IsValuable()))
        {
            IconFetchedEvent.Invoke(null);
        }

        var emptyButton = photoButtons.FirstOrDefault(b => b.IsIcon && !b.Path.IsValuable());

        if (emptyButton != null)
        {
            emptyButton.ClearIconSelection();

            var button = photoButtons.FirstOrDefault(b => b.Path.IsValuable());

            if(button == null)
            {
                IconFetchedEvent.Invoke(null);
            }
            else
            {
                button.SelectAsIcon();
            }
        }
    }

    protected override async UniTask PropagateEditedFields()
    {
        if (!photoButtons.Any(b => b.IsIcon))
        {
            photoButtons[0].SelectAsIcon();
            return;
        }

        CurrentPoint.audioDescriptionPath = audioLoader.LoadedPath;

        CurrentPoint.pointPhotosPath.Clear();

        for (int i = 0; i < photoButtons.Count; i++)
        {
            CurrentPoint.pointPhotosPath.Add(photoButtons[i].Path);    
        }

        CurrentPoint.pointPhotosPath = photoButtons.ConvertAll(b => new PhotoData { path = b.Path, isIcon = b.IsIcon });

        var iconPath = photoButtons.FirstOrDefault(b => b.IsIcon).Path;

        CurrentPoint.name = nameField.text;
        CurrentPoint.description = descriptionField.text;

        CurrentPoint.iconPath = iconPath;

        CurrentPoint.group = (Group)(dropdown.value + 1);
        CurrentPoint.showByDefault = showByDefaultToggle.isOn;

        CurrentPoint.SetDirty();
    }

    private void IconSelected()
    {
        foreach(var photoButton in photoButtons)
        {
            photoButton.ClearIconSelection();

            if(photoButton.IsIcon)
            {
                IconFetchedEvent.Invoke(photoButton.LoadedImage);
            }
        }
    }

    protected override async UniTask EditHandle(bool isNew)
    {
        if(isNew)
        {
            GameManager.Instance.IsUserPointOpened = true;
        }

        await LoaderScreen.ShowAsync(false);

        audioLoader.SetCurrent(CurrentPoint.audioDescriptionPath);
        nameField.text = CurrentPoint.name;
        descriptionField.text = CurrentPoint.description;

        if(isNew)
        {
            dropdown.value = (int)(Traveler.TryGetSceneHandler<MainMapSceneHandler>().Container.CurrentGroup) - 1;
        }
        else
        {
            dropdown.value = (int)(CurrentPoint.group) - 1;
        }


        if(CurrentPoint.name.IsValuable())
        {
            headerLabel.text = "Редактирование объекта";

            List<UniTask> loadTasks = new List<UniTask>();

            for (int i = 0; i < photoButtons.Count; i++)
            {
                if (i >= 0 && i < CurrentPoint.pointPhotosPath.Count)
                {
                    loadTasks.Add(photoButtons[i].LoadPhoto(CurrentPoint.pointPhotosPath[i], CurrentPoint.pointPhotosPath[i].isIcon));
                }
            }

            await UniTask.WhenAll(loadTasks);
        }
        else
        {
            headerLabel.text = "Создание объекта";
        }

        IconFetchedEvent.Invoke(photoButtons[0].LoadedImage);

        showByDefaultToggle.isOn = CurrentPoint.showByDefault;

        await LoaderScreen.HideAsync(false);
        
        photoButtons.ForEach(b => b.Missclick.EnableCheck());

        Messager.Instance.Send<UpdateGroupDropdownMessage>();
    }

    protected override async UniTask DeleteHandle()
    {
        await CurrentPoint.DestroyAllResources();
        await _handler.Container.DeletePoint(CurrentPoint);
        DetailedView.Instance.Close();
    }

    protected override string CheckFields()
    {
        if(!nameField.text.IsValuable())
        {
            nameHighlighter.Highlight();
            return "Введите название точки";
        }

        if(!descriptionField.text.IsValuable())
        {
            descriptionHighlighter.Highlight();
            return "Введите описание точки";
        }

        if(photoButtons.All(x => !x.Path.IsValuable()))
        {
            photoHighlighter.Highlight();
            return "Добавьте хотя бы одно фото";
        }

        return null;
    }

    protected override void SaveMessage()
    {
        if(_isNew)
        {
            Notifier.Instance.Notify(NotifyType.Success, "Новая точка сохранена");
        }
        else
        {
            Notifier.Instance.Notify(NotifyType.Success, "Точка сохранена");
        }
    }

    protected override string GetDeleteMessage()
    {
        return "Вы уверены что хотите удалить точку?";
    }

    protected override bool CheckEdit()
    {
        var edited = nameField.text != CurrentPoint.name ||
            descriptionField.text != CurrentPoint.description ||
            CurrentPoint.group != (Group)(dropdown.value + 1);

        for (int i = 0; i < photoButtons.Count; i++)
        {
            edited = edited || CurrentPoint.pointPhotosPath[i] != photoButtons[i].Path;
        }

        edited = edited || audioLoader.LoadedPath != CurrentPoint.audioDescriptionPath;

        return edited;
    }
}
