using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public abstract class Loader : MonoCached
{
    public abstract void StartLoading();
    public abstract void StopLoading();
}
