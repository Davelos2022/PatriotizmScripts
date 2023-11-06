using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using VolumeBox.Toolbox;
using VolumeBox.Toolbox.UIInformer;

[Serializable]
public class AlbumData
{
    [HideInInspector] public string UID;

    public string albumName;
    public string description;
    public string audioPath;
    public List<string> photoPaths;

    [JsonIgnore] public retere relatedCategory;
    [JsonIgnore] public List<Texture2D> photos;
    [JsonIgnore] public AudioClip audio;
    [JsonIgnore] public Texture2D cover;

    [JsonIgnore] public string RelatedPath => relatedCategory.RelatedPath + "/album_" + UID;

    [JsonIgnore] private bool _dirty;
    [JsonIgnore] public bool IsDirty => _dirty;

    public void SetDirty()
    {
        _dirty = true;
    }

    public void SetClear()
    {
        _dirty = false;
    }

    public AlbumData(retere category)
    {
        albumName = "";
        description = "";
        relatedCategory = category;

        UID = FileManager.GetUID();

        photoPaths = new();

        for (int i = 0; i < 5; i++)
        {
            photoPaths.Add(null);
        }

        photos = new();
    }

    public async UniTask LoadCover()
    {
        cover = await FileManager.LoadTextureAsync(photoPaths.FirstOrDefault(x => x.IsValuable()), true);
    }

    public async UniTask LoadData()
    {
        audio = null;

        if(audioPath.IsValuable())
        {
            audio = await FileManager.LoadAudioAsync(audioPath);
        }

        photos.Clear();

        var loadPhotoTasks = new UniTask<Texture2D>[photoPaths.Count];

        for (int i = 0; i < photoPaths.Count; i++)
        {
            if (photoPaths[i].IsValuable())
            {
                loadPhotoTasks[i] = UniTask.Create(async () => await FileManager.LoadTextureAsync(photoPaths[i]));
            }
        }

        var loadedTextures = await UniTask.WhenAll(loadPhotoTasks);

        for (int i = 0; i < loadedTextures.Length; i++)
        {
            if (loadedTextures[i] != null)
            {
                photos.Add(loadedTextures[i]);
            }
        }
    }

    public async UniTask ClearData()
    {
        var photosInFolder = Directory.GetFiles($"{FileManager.DataPath}/{RelatedPath}");

        var deleteTasks = new List<UniTask>();

        foreach(var photo in photosInFolder) 
        {
            var photoName = Path.GetFileName(photo);

            if(photoName.ToLower().Contains("audio_description"))
            {
                continue;
            }

            if(!photoPaths.Contains($"{RelatedPath}/{photoName}"))
            {
                deleteTasks.Add(UniTask.RunOnThreadPool(() => File.Delete(photo)));
            }
        }

        await UniTask.WhenAll(deleteTasks);
    }

    public void ClearAll()
    {
        DestroyData();

        albumName = "";
        description = "";

        audio = null;
        photoPaths = new();
        photos = new();

        audioPath = "";
    }

    internal async UniTask DestroyData()
    {
        if(audio != null)
        {
            ENTRY.Destroy(audio);
        }
        
        foreach(var photo in photos)
        {
            if(photo != null)
            {
                ENTRY.Destroy(photo);
            }
        }

        photos.Clear();

        await Resources.UnloadUnusedAssets();
    }

    public async UniTask Save()
    {
        if(_dirty)
        {
            string[] files = new string[0];

            if(Directory.Exists(FileManager.DataPath + "/" + RelatedPath))
            {
                files = Directory.GetFiles(FileManager.DataPath + "/" + RelatedPath);
            }

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);

                if (fileName.StartsWith("Audio_Description"))
                {
                    continue;
                }

                if (!photoPaths.Any(x => x.IsValuable() && x.Contains(fileName)))
                {
                    File.Delete(file);
                }
            }

            for (int i = 0; i < photoPaths.Count; i++)
            {
                if (photoPaths[i].IsValidPath())
                {
                    var inf = new FileInfo(photoPaths[i]);
                    var destPath = RelatedPath + "/" + FileManager.GetUID() + inf.Extension;
                    if(!await FileManager.CopyFileAsync(photoPaths[i], FileManager.DataPath + "/" + destPath))
                    {
                        Info.Instance.ShowHint("Ошибка при сохранении. Пожалуйста, выберите другое изображение.");
                    }
                    photoPaths[i] = destPath;
                }
            }

            if(audioPath.IsValidPath())
            {
                var info = new FileInfo(audioPath);
                var audioDestPath = RelatedPath + "/Audio_Description" + info.Extension;
                if(!await FileManager.CopyFileAsync(audioPath, FileManager.DataPath + "/" + audioDestPath))
                {
                    Info.Instance.ShowHint("Ошибка при сохранении. Пожалуйста, выберите другой аудиофайл.");
                }
                audioPath = audioDestPath;
            }

            await ClearData();
        }

        _dirty = false;
    }
    

    //public AlbumData Copy()
    //{
    //    AlbumData newAlbum = new();

    //    newAlbum.name = name;
    //    newAlbum.description = description;
    //    newAlbum.audioPath = audioPath;
        
    //    if(audio != null)
    //    {
    //        newAlbum.audio = ENTRY.Instantiate(audio);
    //    }

    //    newAlbum.photoPaths = photoPaths.ToList();

    //    newAlbum.photos = photos.ConvertAll(p => 
    //    {
    //        if (p != null)
    //        {
    //            return ENTRY.Instantiate(p);
    //        }

    //        return null;
    //    });


    //    return newAlbum;
    //}
}

[Serializable]
public class PhotoData
{
    public string path;
    public string miniaturePath;
    public bool isIcon;

    public static implicit operator PhotoData(string inPath)
    {
        return new PhotoData { path = inPath };
    }

    public static implicit operator string(PhotoData photoData)
    {
        return photoData.path;
    }
}