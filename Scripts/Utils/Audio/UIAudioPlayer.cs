using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class UIAudioPlayer : MonoCached
{
    [SerializeField] private string clipId = "map_click";

    public void Play()
    {
        AudioPlayer.Instance.PlayUI(clipId);
    }

    public void PlayOnState(bool value)
    {
        if(value)
        {
            Play();
        }
    }
}
