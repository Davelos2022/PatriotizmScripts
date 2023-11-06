using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using VolumeBox.Toolbox;
using VolumeBox.Toolbox.UIInformer;

public class AudioLoader: MonoCached
{
    [SerializeField] private TMP_Text loadText;
    [SerializeField] private GameObject deleteButton;
    public string LoadedPath { get; private set; }

    public void LoadAudio()
    {
        LoadAudioAsync();
    }

    public void SetCurrent(string path)
    {
        LoadedPath = path;

        UpdateText();
    }

    private void UpdateText()
    {
        if (LoadedPath.IsValuable())
        {
            loadText.text = "Изменить файл...";
            deleteButton.SetActive(true);
        }
        else
        {
            loadText.text = "Добавить файл...";
            deleteButton.SetActive(false);
        }
    }

    public void DeleteAudio()
    {
        LoadedPath = string.Empty;
        UpdateText();
    }

    public async UniTask LoadAudioAsync()
    {
        var path = FileManager.SelectAudioInBrowser();

        if(!path.IsValuable())
        {
            return;
        }

        try
        {
            var ext = Path.GetExtension(path).ToLower();

            if(ext != ".wav" && ext != ".mp3")
            {
                Notifier.Instance.Notify(NotifyType.Error, "Ошибка загрузки");
                return;
            }

            await FileManager.LoadAudioAsync(path, false);
        }
        catch
        {
            Notifier.Instance.Notify(NotifyType.Error, "Ошибка загрузки");
            Debug.LogError($"Failed to load audio at path {path}");
            return;
        }

        LoadedPath = path;

        UpdateText();
    }
}
