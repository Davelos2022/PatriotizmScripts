using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;
using VolumeBox.Toolbox;

[Serializable]
public partial class MapPointData
{
    private const int ICON_SIZE = 128;

    [HideInInspector] public string UID;
    public string name;

    [JsonIgnore] public Texture2D icon;
    public string iconPath;

    [JsonIgnore]public List<Texture2D> photos = new List<Texture2D>();
    public List<PhotoData> pointPhotosPath = new List<PhotoData>(5);

    [TextArea(10, 25)] public string description;
    public float anchoredPositionX;
    public float anchoredPositionY;
    public bool userPoint;
    public bool showByDefault = false;
    public List<retere> categories = new List<retere>();
    public bool extended;

    public Group group = Group.Animals;

    [JsonIgnore] public bool IsDirty => _dirty; 

    [JsonIgnore] private bool _dirty;

    [JsonIgnore] public string RelatedPath => (userPoint ? GameManager.Instance.UserPointsPath : GameManager.Instance.BasePointsPath) + "/p_" + UID;

    [HideInInspector] public string audioDescriptionPath;
    [JsonIgnore] public AudioClip clip;

    public MapPointData(bool isNew = false)
    {
        pointPhotosPath = new(5);

        if(isNew)
        {
            for (int i = 0; i < 5; i++)
            {
                pointPhotosPath.Add(new PhotoData());
            }
        }

        photos = new();
        UID = FileManager.GetUID();
        categories = new();
    }

    public void UpdateRelations()
    {
        foreach (var category in categories)
        {
            category.relatedPoint = this;
            category.UpdateRelations();
        }
    }

    public async UniTask LoadIcon()
    {
        if(icon == null)
        {
            await DestroyIconResource();

            icon = await FileManager.LoadTextureAsync(iconPath);
        }
    }

    public async UniTask LoadClip()
    {
        await DestroyClipResource();

        if (audioDescriptionPath.IsValuable())
        {
            clip = await FileManager.LoadAudioAsync(audioDescriptionPath);
        }
    }

    public async UniTask LoadPhotos(CancellationToken token = default)
    {
        await DestroyPhotoResources();

        var loadPhotosTasks = new UniTask<Texture2D>[pointPhotosPath.Count];

        for (int i = 0; i < pointPhotosPath.Count; i++)
        {
            if(token.IsCancellationRequested)
            {
                loadPhotosTasks = null;
                return;
            }

            if (pointPhotosPath[i].path.IsValuable())
            {
                loadPhotosTasks[i] = UniTask.Create<Texture2D>(async () => await FileManager.LoadTextureAsync(pointPhotosPath[i]));
            }
        }

        var resultTextures = await UniTask.WhenAll(loadPhotosTasks).AttachExternalCancellation(token);

        foreach (var task in resultTextures)
        {
            photos.Add(task);
        }

        photos = photos.OrderBy(x =>
        {
            if (x == null)
            {
                return 0;
            }
            else
            {
                return x.GetPixel(0, 0).r;
            }
        }).ToList();
    }

    public async UniTask LoadDetailedData()
    {
        await DestroyAllResources();
        await LoadIcon();
        await LoadClip();
        await LoadPhotos();
    }

    public async UniTask Save()
    {
        if(_dirty)
        {
            FileInfo info;

            string iconResolvePath = string.Empty;

            //copying icon
            if (iconPath.IsValidPath())
            {
                info = new FileInfo(iconPath);
                iconResolvePath = iconPath;
                iconPath = RelatedPath + "/icon" + info.Extension;

                var tex = await FileManager.LoadTextureAsync(iconResolvePath, false);

                var destIconPath = FileManager.DataPath + "/" + RelatedPath;

                if (!Directory.Exists(destIconPath))
                {
                    Directory.CreateDirectory(destIconPath);
                }

                var finalTex = FileManager.LowerTexture(tex, ICON_SIZE);

                await FileManager.SaveTexture(finalTex, destIconPath + "/icon.jpg");

                iconPath = RelatedPath + "/icon.jpg";

                await DestroyIconResource();

                icon = finalTex;
            }

            icon = await FileManager.LoadTextureAsync(iconPath);

            //copying audio
            if (audioDescriptionPath.IsValidPath())
            {
                info = new FileInfo(audioDescriptionPath);
                var newAudioPath = RelatedPath + "/Audio_Description" + info.Extension;
                await FileManager.CopyFileAsync(audioDescriptionPath, FileManager.DataPath + "/" + newAudioPath);
                audioDescriptionPath = newAudioPath;
            }

            if(Directory.Exists(FileManager.DataPath + "/" + RelatedPath))
            {
                var files = Directory.GetFiles(FileManager.DataPath + "/" + RelatedPath);

                foreach(var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);

                    if(fileName.StartsWith("icon") || fileName.StartsWith("Audio_Description"))
                    {
                        continue;
                    }

                    if (!pointPhotosPath.Any(x => x.path.IsValuable() && x.path.Contains(fileName)))
                    {
                        File.Delete(file);
                    }
                }
            }

            //copying photos
            for (int i = 0; i < pointPhotosPath.Count; i++)
            {
                if (pointPhotosPath[i].path.IsValidPath())
                {
                    info = new FileInfo(pointPhotosPath[i]);
                    var newPhotoPath = RelatedPath + "/" + FileManager.GetUID() + info.Extension;
                    await FileManager.CopyFileAsync(pointPhotosPath[i], FileManager.DataPath + "/" + newPhotoPath);
                    pointPhotosPath[i] = newPhotoPath;
                }
            }

            await RegenerateMiniatures();
        }

        //saving categories
        List<UniTask> categorySaveTasks = new();

        for (int i = 0; i < categories.Count; i++)
        {
            categorySaveTasks.Add(categories[i].Save());
        }

        await UniTask.WhenAll(categorySaveTasks);
    }

    public async UniTask DestroyPhotoResources()
    {
        foreach (var photo in photos)
        {
            if (photo != null)
            {
                ENTRY.Destroy(photo);
            }
        }

        photos.Clear();

        await Resources.UnloadUnusedAssets();
    }

    public async UniTask DestroyIconResource()
    {
        if (icon != null)
        {
            ENTRY.Destroy(icon);
        }

        await Resources.UnloadUnusedAssets();
    }

    public async UniTask DestroyClipResource()
    {
        if (clip != null)
        {
            ENTRY.Destroy(clip);
        }

        await Resources.UnloadUnusedAssets();
    }

    public async UniTask DestroyAllResources()
    {
        await DestroyPhotoResources();
        await DestroyClipResource();
        await DestroyIconResource();
    }

    public void ClearAll()
    {
        iconPath = string.Empty;
        pointPhotosPath.Clear();
        UID = string.Empty;
        description = string.Empty;
        audioDescriptionPath = string.Empty;
        anchoredPositionX = 0;
        anchoredPositionY = 0;
        userPoint = false;
        showByDefault = false;
        group = Group.Animals;
        icon = null;
        clip = null;
        photos.Clear();

        foreach(var category in categories)
        {
            category.DestroyData();
        }

        categories = new();
    }

    public void SetDirty()
    {
        _dirty = true;
    }

    public void SetClear()
    {
        _dirty = false;
    }

    public async UniTask RegenerateMiniatures()
    {
        foreach (var photo in pointPhotosPath)
        {
            if (photo is null || !photo.path.IsValuable())
            {
                photo.miniaturePath = string.Empty;
                continue;
            }

            var fullPath = FileManager.DataPath + "/" + photo;
            var minPath = RelatedPath + "/" + Path.GetFileNameWithoutExtension(fullPath) + "_min.jpg";
            var fullMinPath = FileManager.DataPath + "/" + minPath;
            photo.miniaturePath = minPath;

            if(File.Exists(fullMinPath))
            {
                await Task.Run(() => File.Delete(fullMinPath));
            }

            var tex = await FileManager.LoadTextureAsync(photo);

            var min = FileManager.LowerTexture(tex, 256);

            byte[] bytes = min.EncodeToJPG(60);


            await File.WriteAllBytesAsync(fullMinPath, bytes);
        }
    }

    //public void CopyFrom(MapPointData data)
    //{
    //    name = data.name;
    //    iconPath = data.iconPath;
    //    pointPhotosPath = data.pointPhotosPath.ToList();
    //    description = data.description;
    //    audioDescriptionPath = data.audioDescriptionPath;
    //    anchoredPositionX = data.anchoredPositionX;
    //    anchoredPositionY = data.anchoredPositionY;
    //    group = data.group;
    //    showByDefault = data.showByDefault;
    //    userPoint = data.userPoint;
    //    UID = data.UID;

    //    icon = icon == null ? null : ENTRY.Instantiate(data.icon);
    //    clip = clip == null ? null : ENTRY.Instantiate(data.clip);

    //    photos = data.photos.ConvertAll(p => 
    //    {
    //        if(p == null)
    //        {
    //            return null;
    //        }
    //        else
    //        {
    //            return ENTRY.Instantiate(p);
    //        }
    //    });

    //    wavy = data.wavy;
    //    categories = data.categories.ConvertAll(c => c.Copy()).ToList();

    //    Debug.Log(categories[0].albums.Count);
    //}
}

public enum RegionArea
{
    None,
    DalVost,
    Privolzh,
    SevKav,
    SevZap,
    Sibir,
    Ural,
    Central,
    Uzhn
}

public enum Group
{
    None,
    Animals,
    Nature_Objects,
    Culture,
    Minerals,
    Industry,
    Other,
    City,
}
