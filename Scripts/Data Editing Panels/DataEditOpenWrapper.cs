using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class DataEditOpenWrapper : MonoBehaviour
{
    [SerializeField] private string editSceneName = "Data Editing Scene";
    [SerializeField] private PointEditType editType;

    [Button("Open")]
    public async void Open()
    {
        var args = ScriptableObject.CreateInstance<DataEditingSceneArgs>();
        //args.container = null;// Traveler.TryGetSceneHandler<MainMapSceneHandler>().Container;
        //args.editType = editType;

        try
        {
            Traveler.LoadScene(editSceneName, args);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
