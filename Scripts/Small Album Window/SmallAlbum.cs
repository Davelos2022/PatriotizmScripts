using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VolumeBox.Toolbox;
using VolumeBox.Toolbox.UIInformer;

public class SmallAlbum : MonoCached
{
    [SerializeField] private string photoViewTag;
    [SerializeField] private string photoDefaultViewTag;
    [SerializeField] private Transform viewRoot;
    [SerializeField] private GameObject addPhotoButton;
    [SerializeField] private int maxPhotoCount = 50;
    [SerializeField] private bool editable = true;

    private List<LoadedTextureData> _loadedTextures;

    public int LoadedTexturesCount => _loadedTextures.Count;

    private ToggleGroup _providedToggleGroup;

    public UnityEvent<Texture2D> TextureAdded;

    public void SetSource(List<Texture2D> list)
    {
        _loadedTextures = list.ConvertAll(x => new LoadedTextureData { texture = x });

        RefreshView();
    }

    public void ProvideToggleGroup(ToggleGroup group)
    {
        _providedToggleGroup = group;
    }

    public void RemoveByIndex(int index)
    {
        if(_loadedTextures == null || _loadedTextures.Count <= 0 || index < 0 || index >= _loadedTextures.Count)
        {
            return;
        }

        try
        {
            Destroy(_loadedTextures[index].texture);
        }
        catch
        {
            Debug.LogWarning("Skipped destroying asset");
        }

        _loadedTextures.RemoveAt(index);
        RefreshView();
    }

    public async void SelectImageInBrowser()
    {
        FileManager.SelectImageInBrowser(out string path);

        if(path.IsValuable())
        {
            LoaderScreen.ShowAsync();
            var tex = await FileManager.LoadTextureAsync(path, false);

            if(tex == null)
            {
                Info.Instance.ShowHint("Ошибка при загрузке изображения. Попробуйте загрузить другое.");
            }
            else
            {
                AddTexture(new LoadedTextureData { texture = tex, path = path });
            }

            LoaderScreen.HideAsync();
        }
    }

    public void AddTexture(LoadedTextureData texture)
    {
        if (_loadedTextures == null)
        {
            _loadedTextures = new List<LoadedTextureData>();
        }

        _loadedTextures.Add(texture);

        _loadedTextures = _loadedTextures.OrderBy(x => 
        {
            if(x == null || x.texture == null)
            {
                return 0;
            }
            else
            {
                return x.texture.GetPixel(0, 0).r;
            }
        }).ToList();

        TextureAdded.Invoke(texture.texture);
        RefreshView();
    }

    [Button("Refresh View")]
    public void RefreshView()
    {
        Clear();

        foreach (var data in _loadedTextures)
        {
            string tag;

            if(editable)
            {
                tag = photoViewTag;
            }
            else
            {
                tag = photoDefaultViewTag;
            }

            Pooler.Instance.Spawn(tag, Vector3.zero, Quaternion.identity, viewRoot, new AlbumPhotoViewData { album = this, texture = data.texture, toggleGroup = _providedToggleGroup }, x => x.transform.localScale = Vector3.one);
        }

        addPhotoButton.SetActive(_loadedTextures.Count < maxPhotoCount);
    }

    [Button("Clear")]
    public void Clear()
    {
        while (viewRoot.childCount > 0)
        {
            Pooler.Instance.DespawnOrDestroy(viewRoot.GetChild(0).gameObject);
        }
    }

    public List<LoadedTextureData> GetEditedList()
    {
        return _loadedTextures.ToList();
    }
}

[Serializable]
public class AlbumPhotoViewData
{
    public ToggleGroup toggleGroup = null;
    public SmallAlbum album;
    public Texture2D texture;
}
