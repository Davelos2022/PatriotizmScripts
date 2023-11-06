using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using VolumeBox.Toolbox;

public class CategoryPreview: MonoCached, IPooled
{
    [SerializeField] private string addAlbumButtonTag;
    [SerializeField] private string albumButtonTag;
    [SerializeField] private ScrollableLayoutHandler layout;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Toggle toggle;
    [SerializeField] private AccordionElement accordion;

    public UnityEvent<Texture2D> OnTextureLoaded;

    private retere _currentCategory;

    public ScrollableLayoutHandler AlbumLayout => layout;
    public retere CurrentCategory => _currentCategory;

    private bool skipPush = false;

    public void OnSpawn(object data)
    {
        layout?.Clear();

        if (toggle != null)
        {
            toggle.isOn = false;
        }

        accordion.isOn = false;
        _currentCategory = (retere)data;
        OnTextureLoaded.Invoke(null); //clearing view on spawn
        accordion.enabled = _currentCategory.albums.Count > 0 || GameManager.Instance.IsAdminMode;

        SpawnAsync();
    }

    public void Click(bool skipPush = false)
    {
        toggle.isOn = true;
        accordion.isOn = true;
        this.skipPush = skipPush;
    }

    public void OnToggleValueChanged(bool value)
    {
        if (_currentCategory.albums.Count <= 0) return;

        if(value)
        {
            OpenCategoryView();
        }
    }

    public void OpenCategoryView()
    {
        Messager.Instance.Send(new CategoryOpenMessage { category = _currentCategory });
    }

    public async UniTask SpawnAsync()
    {
        nameText.text = _currentCategory.name;
        //descriptionText.text = _currentCategory.description;

        var tex = await FileManager.LoadTextureAsync(_currentCategory.coverPath);

        layout?.Clear();

        if (toggle != null)
        {
            toggle.isOn = false;
        }

        toggle.group = toggle.GetComponentInParent<ToggleGroup>();

        if (_currentCategory.albums.Count > 0)
        {
            layout.SetData(_currentCategory.albums.ConvertAll(a => new ScrollableLayoutObjectData { data = new AlbumRelatedData { albumData = a, categoryData = _currentCategory }, objectTag = albumButtonTag }));
        }

        layout.AddObject(new ScrollableLayoutObjectData { data = new AlbumRelatedData { categoryData = _currentCategory, mapPointData = _currentCategory.relatedPoint }, objectTag = addAlbumButtonTag }).transform.SetAsFirstSibling();

        OnTextureLoaded.Invoke(tex);
    }

    public void OnClick(bool value)
    {
        if(value)
        {
            if(skipPush)
            {
                skipPush = false;
            }
            else
            {
                Messager.Instance.Send(new PushScrollMessage { obj = transform });
            }
        }
    }

    public void EditCategory()
    {
        Messager.Instance.Send(new CategoryEditMessage { category = _currentCategory });
    }
}

public class AlbumRelatedData
{
    public AlbumData albumData;
    public retere categoryData;
    public MapPointData mapPointData;
}
