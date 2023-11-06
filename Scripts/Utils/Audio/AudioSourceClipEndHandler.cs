using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;

public class AudioSourceClipEndHandler : MonoCached
{
    [SerializeField] private AudioSource source;

    public UnityEvent ClipEnded;

    protected override void Tick()
    {
        if (source.clip != null && source.GetClipRemainingTime() <= 0)
        {
            ClipEnded.Invoke();
        }
    }
}
