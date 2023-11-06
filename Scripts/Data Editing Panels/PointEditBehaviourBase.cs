using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;
using VolumeBox.Toolbox.UIInformer;

public abstract class PointEditBehaviourBase : MonoCached
{
    [SerializeField] private TMP_Text saveButtonText;
    [SerializeField] private string pointsFolder;

    protected MapPointData CurrentPoint;
    protected retere CurrentCategory;
    protected AlbumData CurrentAlbum;

    protected DataEditingSceneHandler _handler;

    public UnityEvent OpenedExisting;

    protected bool _isNew;

    protected override void Rise()
    {
        _handler = Traveler.TryGetSceneHandler<DataEditingSceneHandler>();
    }

    public async UniTask Edit(MapPointData data, retere category = null, AlbumData album = null, bool isNew = false)
    {
        await LoaderScreen.ShowAsync(false);

        CurrentPoint = data;
        CurrentCategory = category;
        CurrentAlbum = album;

        _isNew = isNew;

        if(isNew)
        {
            saveButtonText.text = "Создать";
        }
        else
        {
            saveButtonText.text = "Сохранить";
            OpenedExisting?.Invoke();
        }

        await EditHandle(isNew);
    }

    public void Delete()
    {
        Info.Instance.ShowBox(GetDeleteMessage(), () => DeleteAsync(), null, "Подтвердите действие", "Удалить", "Отмена", Color.red);
    }

    public async UniTask DeleteAsync()
    {
        await LoaderScreen.ShowAsync(false);
        
        await DeleteHandle();

        await _handler.Container.RefreshSave();

        _handler.Close();

        await LoaderScreen.HideAsync(false);
    }

    public void Close()
    {
        if(CheckEdit())
        {
            Info.Instance.ShowBox("Сохранить изменения перед закрытием?", () => Save(), () => CloseHandle(), "Подтвердите действие", "Сохранить", "Не сохранять", GameManager.Instance.NormalColor);
        }
        else
        {
            CloseHandle();
        }

    }

    private void CloseHandle()
    {
        CurrentPoint?.SetClear();
        CurrentCategory?.SetClear();
        CurrentAlbum?.SetClear();

        _handler.Close();
    }

    protected abstract bool CheckEdit();

    public void Save()
    {
        var checkMessage = CheckFields();

        if (checkMessage != null)
        {
            Notifier.Instance.Notify(NotifyType.Error, checkMessage);
            return;
        }

        SaveAsync();

        if (_isNew)
        {
            var detailedView = Traveler.TryGetSceneHandler<PointDetailedViewSceneHandler>();

            if(detailedView is not null)
            {
                detailedView.ScrollToEnd = true;
            }
        }
    }

    public async UniTask SaveAsync()
    {
        await LoaderScreen.ShowAsync(false);

        await PropagateEditedFields();

        await CurrentPoint.Save();

        await _handler.Container.SavePoint(CurrentPoint);

        Messager.Instance.Send<PointSavedMessage>();
        Messager.Instance.Send(new SetFilterMessage { group = CurrentPoint.group });
        _handler.Container.TryClickPin(CurrentPoint);

        SaveMessage();

        _handler.Close();
    }

    protected abstract string CheckFields();

    protected abstract string GetDeleteMessage();
    protected abstract void SaveMessage();
    protected abstract UniTask PropagateEditedFields();
    protected abstract UniTask EditHandle(bool isNew);
    protected abstract UniTask DeleteHandle();
}

[Serializable]
public class PointSavedMessage: Message
{

}
