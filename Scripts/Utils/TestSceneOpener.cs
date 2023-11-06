using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class TestSceneOpener : MonoBehaviour
{
    [SerializeField] private string sceneName;

    [Button("Open")]
    public void Open()
    {
        Traveler.LoadScene(sceneName);
    }
}
