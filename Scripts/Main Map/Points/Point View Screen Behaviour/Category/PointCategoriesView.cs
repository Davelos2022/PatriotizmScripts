using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class PointCategoriesView : MonoCached
{
    [SerializeField] private string addCategoryButtonTag;
    [SerializeField] private string categoryButtonTag;
    [SerializeField] private Transform buttonsRoot;
    [SerializeField] private ImagePreviewer previewer;

    private List<retere> _currentCategories;

    public List<retere> CurrentCategories => _currentCategories;

    private Subscriber _categoryClickSub;

    protected override void Rise()
    {
        _categoryClickSub = Messager.Instance.Subscribe<CategoryOpenMessage>(c => OnCategoryOpen(c.category));
    }

    public void RefreshPointCategories(MapPointData point)
    {
        while(buttonsRoot.childCount > 0)
        {
            if (Pooler.HasInstance)
            {
                Pooler.Instance.TryDespawn(buttonsRoot.GetChild(0).gameObject);
            }
        }

        _currentCategories = point.categories;

        foreach(var category in _currentCategories)
        {
            Pooler.Instance.Spawn(categoryButtonTag, Vector3.zero, Quaternion.identity, buttonsRoot, category, x => x.transform.localScale = Vector3.one);
        }

        Pooler.Instance.Spawn(addCategoryButtonTag, Vector3.zero, Quaternion.identity, buttonsRoot, null, x => x.transform.localScale = Vector3.one);
    }

    protected override void Destroyed()
    {
        Messager.Instance.RemoveSubscriber(_categoryClickSub);
    }

    private void OnCategoryOpen(retere data)
    {
        previewer.SetList(new List<Texture2D> { data.cover });
    }
}
