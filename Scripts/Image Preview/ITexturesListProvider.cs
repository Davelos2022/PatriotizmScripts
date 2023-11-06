using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITexturesListProvider
{
    public List<Texture2D> images { get; }
}
