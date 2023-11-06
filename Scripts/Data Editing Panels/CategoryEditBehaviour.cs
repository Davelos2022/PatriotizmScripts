using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class CategoryEditBehaviour: PointEditBehaviourBase
{
    [SerializeField] private TMP_Text headerLabel;
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private AddPhotoButton addPhotoButton;
    [Space]
    [SerializeField] private ErrorHighlighter nameHighlighter;
    [SerializeField] private ErrorHighlighter photoHighlighter;

    protected override async UniTask DeleteHandle()
    {
        CurrentPoint.categories.Remove(CurrentCategory);
        await CurrentPoint.Save();
        //Traveler.TryGetSceneHandler<MainMapSceneHandler>()?.Container.RefreshSave();
    }

    protected override async UniTask EditHandle(bool isNew)
    {
        if(CurrentCategory == null)
        {
            CurrentCategory = new(CurrentPoint);
        }
        else
        {
            await CurrentCategory.LoadData();
        }

        nameField.text = CurrentCategory.name;
        
        if(CurrentCategory.name.IsValuable())
        {
            headerLabel.text = "Редактирование категории";
        }
        else
        {
            headerLabel.text = "Создание категории";
        }

        await addPhotoButton.LoadPhoto(CurrentCategory.coverPath, true);


        await LoaderScreen.HideAsync(false);
    }

    protected override async UniTask PropagateEditedFields()
    {
        CurrentCategory.name = nameField.text;
        CurrentCategory.coverPath = addPhotoButton.Path;

        if(!CurrentPoint.categories.Any(x => x.UID == CurrentCategory.UID))
        {
            CurrentPoint.categories.Add(CurrentCategory);
        }

        CurrentCategory.SetDirty();
    }

    protected override string CheckFields()
    {
        if(!nameField.text.IsValuable())
        {
            nameHighlighter.Highlight();
            return "Введите название категории";
        }

        if(!addPhotoButton.Path.IsValuable())
        {
            photoHighlighter.Highlight();
            return "Добавьте обложку к категории";
        }

        return null;
    }

    protected override void SaveMessage()
    {
        Notifier.Instance.Notify(NotifyType.Success, "Категория сохранена");
    }

    protected override string GetDeleteMessage()
    {
        return "Вы уверены что хотите удалить категорию?";
    }

    protected override bool CheckEdit()
    {
        var edited = nameField.text != CurrentCategory.name;

        edited = edited || addPhotoButton.Path != CurrentCategory.coverPath;

        return edited;
    }
}
