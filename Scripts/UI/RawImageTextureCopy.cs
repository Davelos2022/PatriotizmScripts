using UnityEngine;
using UnityEngine.UI;

public class RawImageTextureCopy : MonoBehaviour
{
    [SerializeField] private RawImage from;
    [SerializeField] private RawImage to;
    
    public void Copy()
    {
        to.texture = from.texture;
    }
}
