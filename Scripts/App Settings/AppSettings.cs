using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

[Serializable]
public class AppSettings : ScriptableObject, IEquatable<AppSettings>
{
    public string pinCode = "1234";
    public bool screenshotEnabled = true;
    public bool volumeSliderEnabled = true;
    public bool drawingEnabled = true;
    public string screenshotsPath;
    public string drawingsPath;
    public bool musicByDefaultEnabled = true;
    public bool uiSoundEnabled = true;
    public string otherGroupName = "Моя Категория";
    public bool passedOnBoarding = false;

    public void CopyTo(AppSettings other)
    {
        other.pinCode = pinCode;
        other.screenshotEnabled = screenshotEnabled;
        other.volumeSliderEnabled = volumeSliderEnabled;
        other.drawingEnabled = drawingEnabled;
        other.screenshotEnabled = screenshotEnabled;
        other.drawingsPath = drawingsPath;
        other.screenshotsPath = screenshotsPath;
        other.musicByDefaultEnabled = musicByDefaultEnabled;
        other.uiSoundEnabled = uiSoundEnabled;
        other.otherGroupName = otherGroupName;
        other.passedOnBoarding = passedOnBoarding;
    }

    public bool Equals(AppSettings other)
    {
        bool equals = true;

        equals = equals && (pinCode == other.pinCode);
        equals = equals && (screenshotEnabled == other.screenshotEnabled);
        equals = equals && (volumeSliderEnabled == other.volumeSliderEnabled);
        equals = equals && (drawingEnabled == other.drawingEnabled) ;
        equals = equals && (screenshotsPath == other.screenshotsPath);
        equals = equals && (drawingsPath == other.drawingsPath);
        equals = equals && (musicByDefaultEnabled == other.musicByDefaultEnabled);
        equals = equals && (uiSoundEnabled == other.uiSoundEnabled);
        equals = equals && (otherGroupName == other.otherGroupName);
        equals = equals && (passedOnBoarding == other.passedOnBoarding);

        return equals;
    }
}

public class SettingsChangedMessage: Message { }
