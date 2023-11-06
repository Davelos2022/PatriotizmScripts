using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using VolumeBox.Toolbox;

[Serializable]
public class retere
{
    [HideInInspector] public string UID;

    public string name;
    public string description;
    public string audioPath;
    public string coverPath;
    public List<AlbumData> albums;

    [JsonIgnore] public MapPointData relatedPoint;
    [JsonIgnore] public AudioClip audio;
    [JsonIgnore] public Texture2D cover;

    [JsonIgnore] public string RelatedPath => relatedPoint.RelatedPath + "/Category_" + UID;

    [JsonIgnore] private bool _dirty;
    [JsonIgnore] public bool IsDirty => _dirty;

    public retere(MapPointData point)
    {
        relatedPoint = point;
        coverPath = null;
        name = "";
        UID = FileManager.GetUID();
        albums = new();
    }

    public void UpdateRelations()
    {
        foreach(var album in albums) 
        {
            album.relatedCategory = this;
        }
    }

    public void SetDirty()
    {
        _dirty = true;

        relatedPoint.SetDirty();
    }

    public async UniTask LoadData()
    {
        audio = null;

        if(audioPath.IsValuable())
        {
            audio = await FileManager.LoadAudioAsync(audioPath);
        }

        cover = null;
        
        if(coverPath.IsValuable())
        {
            cover = await FileManager.LoadTextureAsync(coverPath);
        }
    }

    public async UniTask LoadAlbums()
    {
        var loadAlbumsTasks = new UniTask[albums.Count + 2];

        for (int i = 0; i < albums.Count; i++)
        {
            loadAlbumsTasks[i] = UniTask.Create(async () => await albums[i].LoadData());
        }

        await UniTask.WhenAll(loadAlbumsTasks);
    }

    public async UniTask DestroyData()
    {
        if(audio != null)
        {
            ENTRY.Destroy(audio);
        }

        if(cover != null)
        {
            ENTRY.Destroy(cover);
        }

        await Resources.UnloadUnusedAssets();
    }

    //public CategoryData Copy()
    //{
    //    CategoryData newCategory = new();

    //    newCategory.name = name;
    //    newCategory.description = description;
    //    newCategory.audioPath = audioPath;
    //    newCategory.coverPath = coverPath;
    //    newCategory.albums = albums.ConvertAll(a => a.Copy()).ToList();
        
    //    if(audio != null)
    //    {
    //        newCategory.audio = ENTRY.Instantiate(audio);
    //    }

    //    if(cover != null)
    //    {
    //        newCategory.cover = ENTRY.Instantiate(cover);
    //    }

    //    return newCategory;
    //}

    private async UniTask ClearData()
    {
        var albumPaths = Directory.GetDirectories($"{FileManager.DataPath}/{RelatedPath}");

        var deleteTasks = new List<UniTask>();

        if(albumPaths != null)
        {
            foreach (var album in albumPaths)
            {

                var uid = Path.GetFileName(album)[6..];

                //album_... starts at index 6
                if(!albums.Any(x => x.UID == uid))
                {
                    deleteTasks.Add(UniTask.RunOnThreadPool(() => Directory.Delete(album, true)));
                }
            }
        }

        await UniTask.WhenAll(deleteTasks);
    }

    public async UniTask Save()
    {
        if(_dirty)
        {
            if (!Directory.Exists(FileManager.DataPath + "/" + RelatedPath))
            {
                Directory.CreateDirectory(FileManager.DataPath + "/" + RelatedPath);
            }

            FileInfo info;

            if(audioPath.IsValidPath())
            {
                info = new FileInfo(audioPath);
                var audioDestPath =  RelatedPath + "/Audio_Description" + info.Extension;
                await FileManager.CopyFileAsync(audioPath, FileManager.DataPath + "/" + audioDestPath);
                audioPath = audioDestPath;
            }

            if(coverPath.IsValidPath())
            {
                info = new FileInfo(coverPath);
                var coverDestPath = RelatedPath + "/Cover_Image" + info.Extension;
                await FileManager.CopyFileAsync(coverPath, FileManager.DataPath + "/" + coverDestPath);
                coverPath = coverDestPath;
            }
        }

        UniTask[] saveTasks = new UniTask[albums.Count];

        for (int i = 0; i < albums.Count; i++)
        {
            saveTasks[i] = albums[i].Save();
        }

        await UniTask.WhenAll(saveTasks);

        if(_dirty)
        {
            await ClearData();
        }

        _dirty = false;
    }

    public void CopyTo(retere data)
    {
        data.name = name;
        data.description = description;
        data.audioPath = audioPath;
        data.coverPath = coverPath;
    }

    public void ClearAll()
    {
        DestroyData();

        name = string.Empty;
        description = string.Empty;
        audio = null;
        cover = null;
        audioPath = string.Empty;
        coverPath = string.Empty;

        foreach(var album in albums)
        {
            album.ClearAll();
        }

        albums = new();
    }

    public void SetClear()
    {
        _dirty = false;
    }
}
