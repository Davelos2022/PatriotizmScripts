using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerController : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AnimationCurve volumeCurve;
    [SerializeField] private string valueName = "masterVolume";

    public void SetVolumeNormalized(float value)
    {
        value = Mathf.Clamp01(value);

        mixer.SetFloat(valueName, volumeCurve.Evaluate(value));
    }
}
