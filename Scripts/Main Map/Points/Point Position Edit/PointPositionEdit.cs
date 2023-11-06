using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VolumeBox.Toolbox;

public class PointPositionEdit : MonoCached
{
    [SerializeField] private GameObject editButton;
    [SerializeField] private GameObject saveButton;
    [SerializeField] private GameObject closeButton;
    [SerializeField] private MapPointsContainer container;

    protected override void Rise()
    {
        CloseEditMode();
    }

    private void OpenEditMode()
    {
        editButton.SetActive(false);
        closeButton.SetActive(true);
        saveButton.SetActive(true);
    }

    private void CloseEditMode()
    {
        editButton.SetActive(true);
        closeButton.SetActive(false);
        saveButton.SetActive(false);
    }

    public void Edit()
    {
        OpenEditMode();
        container.DrawedPoints.ForEach(p => p.EnableDrag());
    }

    public void Save() 
    {
        SaveAsync();
    }

    public async UniTask SaveAsync()
    {
        //Jandro was here
        await LoaderScreen.ShowAsync(false);

        container.DrawedPoints.ForEach(p => 
        {
            p.UpdatePosition();
            p.DisableDrag();
        });

        await container.RefreshSave();
        await container.RedrawPoints(true);
        CloseEditMode();
        await LoaderScreen.HideAsync(false);
    }

    public void Close() 
    {
        CloseAsync();
    }

    public async UniTask CloseAsync()
    {
        //Jandro was here
        await LoaderScreen.ShowAsync(false);

        container.DrawedPoints.ForEach(p => p.DisableDrag());

        await container.RedrawPoints(true);
        CloseEditMode();
        await LoaderScreen.HideAsync(false);
    }
}
