using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class PointNavigationVisual : MonoCached
{
    [SerializeField] private RectTransform currentPosition;
    [SerializeField] private Transform pinsRoot;
    [SerializeField] private string pinsTag;

    private List<GameObject> _pins = new List<GameObject>();

    public void SetCount(int count)
    {
        Clear();

        for (int i = 0; i < count; i++)
        {
            var pin = Pooler.Instance.Spawn(pinsTag, Vector3.zero, Quaternion.identity, pinsRoot);

            var p = pin.transform.localPosition;
            p.z = 0;
            pin.transform.localPosition = p;
            pin.transform.localScale = Vector3.one;

            _pins.Add(pin);
        }
    }

    public void ResetCurrent()
    {
        currentPosition.gameObject.SetActive(true);

        currentPosition.anchoredPosition = new Vector2(-4.5f, 0);
    }

    public void SetCurrent(int index)
    {
        if(index < 0 || index >= _pins.Count)
        {
            return;
        }

        currentPosition.gameObject.SetActive(true);

        currentPosition.localPosition = _pins[index].transform.localPosition;
        currentPosition.anchoredPosition -= Vector2.left * -7.5f;
    }

    public void Clear()
    {
        _pins.ForEach(p => Pooler.Instance.DespawnOrDestroy(p));

        _pins.Clear();

        currentPosition.gameObject.SetActive(false);
    }
}
