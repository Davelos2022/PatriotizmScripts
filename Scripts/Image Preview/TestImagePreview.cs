using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class TestImagePreview: MonoCached, ITexturesListProvider
{
    [SerializeField] private List<Texture2D> texs;

    public List<Texture2D> images => texs;

    public UnityEvent<List<Texture2D>> ReadyEvent;

    [Button("Invoke Test")]
    public void InvokeTest()
    {
        ReadyEvent.Invoke(texs);
    }
}
