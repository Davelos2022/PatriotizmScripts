using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppButton : MonoBehaviour
{
    [SerializeField, Scene] private string appEntryScene;
    [SerializeField] private AppState relatedState;
    [SerializeField] private Image panelImage;
    [Space]
    [SerializeField] private Sprite firstPanelSprite;
    [SerializeField] private Sprite middlePanelSprite;
    [SerializeField] private Sprite lastPanelSprite;

    public void UpdateVisuals()
    {
        var ind = transform.GetSiblingIndex();

        var firstActiveIndex = -1;
        var lastActiveIndex = -1;

        for (int i = 0; i < transform.parent.childCount; i++)
        {
            if(transform.parent.GetChild(i).gameObject.activeSelf)
            {
                if(firstActiveIndex == -1)
                {
                    firstActiveIndex = i;
                }

                lastActiveIndex = i;
            }
        }

        if(ind == firstActiveIndex)
        {
            panelImage.sprite = firstPanelSprite;
        }
        else if(ind == lastActiveIndex)
        {
            panelImage.sprite = lastPanelSprite;
        }
        else
        {
            panelImage.sprite = middlePanelSprite;
        }

        gameObject.SetActive(relatedState != GameManager.Instance.CurrentAppState);
    }

    public void OpenApp()
    {
        AppHandler.Instance.Open(appEntryScene);
    }

    public void CloseApp()
    {
        AppHandler.Instance.CloseCurrent();
    }
}
