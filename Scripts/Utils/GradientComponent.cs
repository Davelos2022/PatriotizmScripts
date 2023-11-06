using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class GradientComponent : MonoCached
{
    [SerializeField] private GradientDirection direction;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Material gradientMaterial;

    private Image _image;
    private Material _instMaterial;

    protected override void Rise()
    {
        //UpdateGradient();
    }

    public void SetGradient(Gradient gradient)
    {
        this.gradient = gradient;

        UpdateGradient();
    }

    [ContextMenu("Update Gradient")]
    private void UpdateGradient()
    {
        if(_image == null)
        {
            _image = GetComponent<Image>();
        }

        if (gradient == null)
        {
            gradient = new Gradient();
        }

        _instMaterial = Instantiate(gradientMaterial);

        if(direction == GradientDirection.Horizontal)
        {
            _instMaterial.SetColor("_TopLeftColor", gradient.Evaluate(0));
            _instMaterial.SetColor("_BottomLeftColor", gradient.Evaluate(0));
            _instMaterial.SetColor("_TopRightColor", gradient.Evaluate(1));
            _instMaterial.SetColor("_BottomRightColor", gradient.Evaluate(1));
        }
        
        if(direction == GradientDirection.Vertical)
        {
            _instMaterial.SetColor("_TopLeftColor", gradient.Evaluate(0));
            _instMaterial.SetColor("_BottomLeftColor", gradient.Evaluate(1));
            _instMaterial.SetColor("_TopRightColor", gradient.Evaluate(0));
            _instMaterial.SetColor("_BottomRightColor", gradient.Evaluate(1));
        }

        _image.material = _instMaterial;
    }
}

public enum GradientDirection
{
    Horizontal,
    Vertical,
}
