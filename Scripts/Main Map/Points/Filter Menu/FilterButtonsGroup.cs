using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class FilterButtonsGroup: MonoCached
{
    [SerializeField] private Color defaultButtonColor;
    [SerializeField] private Image defaultMenuIcon;
    [SerializeField] private Image buttonImage;

    private MapPointsContainer _container;

    private FilterButton _currentPressed;

    public FilterButton CurrentPressed => _currentPressed;
    protected override void Rise()
    {
        _container = Traveler.TryGetSceneHandler<MainMapSceneHandler>().Container;
        Messager.Instance.Subscribe<CloseFilterVisualsMessage>(_ => CloseVisuals());
        Messager.Instance.Subscribe<OpenFilterVisualsMessage>(_ => OpenVisuals());
        Messager.Instance.Subscribe<ClearFilterMessage>(_ => ClearFilter());
        Messager.Instance.Subscribe<SetFilterMessage>(m => SetFilter(m.group));
    }

    private List<FilterButton> _filters = new List<FilterButton>();

    public void RegisterFilter(FilterButton filter)
    {
        if (!_filters.Contains(filter))
        {
            _filters.Add(filter);
        }
    }

    public void CloseVisuals()
    {
        _filters.ForEach(x => x.DisableVisuals());
    }

    public void OpenVisuals()
    {
        _filters.ForEach(x =>
        {
            x.EnableVisuals(x == _currentPressed);
        });
    }

    public void ClearFilter()
    {
        if (_currentPressed == null) return;

        OnFilterPressedCallback(_currentPressed);
    }

    public void UpdateVisual()
    {
        if(_currentPressed == null)
        {
            defaultMenuIcon.DOKill();
            defaultMenuIcon.DOFade(1, 0.2f);
            buttonImage.DOKill();
            buttonImage.DOColor(defaultButtonColor, 0.2f);
        }
        else
        {
            defaultMenuIcon.DOKill();
            defaultMenuIcon.DOFade(0, 0.2f);
            _currentPressed.MenuIcon.DOKill();
            _currentPressed.MenuIcon.DOFade(1, 0.2f);
            buttonImage.DOKill();
            buttonImage.DOColor(_currentPressed.ButtonColor, 0.2f);
        }
    }

    public void SetFilter(Group group)
    {
        if(group == Group.None)
        {
            ClearFilter();
        }
        else if(_currentPressed != null && _currentPressed.FilterType == group)
        {
            return;
        }
        else
        {
            var b = _filters.FirstOrDefault(x => x.FilterType == group);

            if(b != null)
            {
                b.OnClickCallback();
            }
        }
    }

    public void OnFilterPressedCallback(FilterButton pressedFilter)
    {
        if (_currentPressed != null && pressedFilter == _currentPressed)
        {
            _currentPressed.MenuIcon.DOKill();
            _currentPressed.MenuIcon.DOFade(0, 0.2f);
            buttonImage.DOKill();
            buttonImage.DOColor(defaultButtonColor, 0.2f);
            defaultMenuIcon.DOKill();
            defaultMenuIcon.DOFade(1, 0.2f);
            _filters.ForEach(f => f.Hide());
            _currentPressed = null;
            _container.SetFilter(Group.None);
            return;
        }

        foreach (FilterButton filter in _filters)
        {
            if (filter == pressedFilter)
            {
                defaultMenuIcon.DOKill();
                defaultMenuIcon.DOFade(0, 0.2f);

                filter.Show();

                if(_currentPressed != null)
                {
                    _currentPressed.MenuIcon.DOKill();
                    _currentPressed.MenuIcon.DOFade(0, 0.2f);
                }

                _currentPressed = pressedFilter;

                buttonImage.DOKill();
                buttonImage.DOColor(_currentPressed.ButtonColor, 0.2f);

                _currentPressed.MenuIcon.DOKill();
                _currentPressed.MenuIcon.DOFade(1, 0.2f);

            }
            else
            {
                filter.Hide();
            }
        }

        UpdateCurrent();
    }

    public void UpdateCurrent()
    {
        if (_currentPressed != null)
        {
            _container.SetFilter(_currentPressed.FilterType);
        }
    }
}

public class CloseFilterVisualsMessage: Message { }
public class OpenFilterVisualsMessage: Message { }
public class ClearFilterMessage: Message { }

[Serializable]
public class SetFilterMessage: Message 
{
    public Group group;
}