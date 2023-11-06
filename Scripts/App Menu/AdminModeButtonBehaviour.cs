using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VolumeBox.Toolbox;

public class AdminModeButtonBehaviour : MonoCached
{
    [SerializeField] private Image icon;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;
    [Foldout("Visual"), SerializeField] private Gradient onGradient;
    [Foldout("Visual"), SerializeField] private Gradient offGradient;
    [Foldout("Visual"), SerializeField] private string onText;
    [Foldout("Visual"), SerializeField] private string offText;

    [SerializeField] private TMP_Text captionText;
    [SerializeField] private GradientComponent gradientComponent;
    protected override void Rise()
    {
        Messager.Instance.Subscribe<AdminModeChangedMessage>(m => ResolveMode(m.enabled));
        ResolveMode(GameManager.Instance.IsAdminMode);
    }

    private void ResolveMode(bool enabledAdmin)
    {
        if (enabledAdmin)
        {
            OnEnabledAdmin();
        }
        else
        {
            OnDisabledAdmin();
        }
    }

    public void OnEnabledAdmin()
    {
        icon.sprite = unlockedSprite;
        captionText.text = onText;
        gradientComponent.SetGradient(onGradient);
    }

    public void OnDisabledAdmin()
    {
        icon.sprite = lockedSprite;
        captionText.text = offText;
        gradientComponent.SetGradient(offGradient);
    }
}
