using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class AddPhotoButton : MonoCached
{
    [SerializeField] private CanvasGroupFader controlButtonsFader;
    [SerializeField] private MissclickHandler missclick;

    private Texture2D loadedImage;
    private string path;
    private bool isIcon;
    private bool isSelectedAsIcon;

    public bool IsIcon => isIcon;
    public string Path => path;
    public Texture2D LoadedImage => loadedImage;

    public UnityEvent<Texture2D> OnImageLoaded;
    public UnityEvent OnImageDeleted;
    public UnityEvent SelectedAsIcon;
    public UnityEvent LoadImageEvent;

    public MissclickHandler Missclick => missclick;

    public void OnMissclick()
    {
        controlButtonsFader.FadeOut();
    }

    public void OnClick()
    {
        if(path.IsValuable())
        {
            controlButtonsFader.FadeIn();
        }
        else
        {
            LoadImageEvent.Invoke();
            LoadPhoto(false);
        }
    }

    public void SelectAsIcon()
    {
        isIcon = true;
        isSelectedAsIcon = true;

        SelectedAsIcon.Invoke();
    }

    public void DeletePhoto()
    {
        loadedImage = null;
        path = string.Empty;
        //missclick.DisableCheck();
        controlButtonsFader.FadeOut();
        OnImageDeleted.Invoke();
    }

    public void LoadPhotoCallback()
    {
        LoadPhoto(false);
    }

    public async UniTask LoadPhoto(bool useSavePath = true)
    {
        var newPath = FileManager.SelectImageInBrowser();

        await LoadPhoto(newPath, false, useSavePath);
    }

    public void ClearIconSelection()
    {
        if(isSelectedAsIcon)
        {
            isSelectedAsIcon = false;
            return;
        }

        isIcon = false;
    }

    public async UniTask LoadPhoto(string pathToLoad, bool isIcon, bool useSavePath = true)
    {
        if (!pathToLoad.IsValuable())
        {
            path = pathToLoad;
            return;
        }


        var newImage = await FileManager.LoadTextureAsync(pathToLoad, useSavePath);

        if(newImage != null)
        {
            if(loadedImage != null)
            {
                Destroy(loadedImage);
            }

            //missclick.EnableCheck();
            loadedImage = newImage;

            path = pathToLoad;
            OnImageLoaded.Invoke(loadedImage);
        }
    }
}
