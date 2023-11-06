using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VolumeBox.Toolbox;

public class EditableDropdownItem: MonoBehaviour
{
    [SerializeField] private int editableIndex;
    [SerializeField] private GameObject editButton;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text groupLabel;

    private Subscriber updateSub;

    private void Awake()
    {
        updateSub = Messager.Instance.Subscribe<UpdateGroupDropdownMessage>(_ => UpdateVisualState());
        inputField.gameObject.SetActive(false);
        editButton.SetActive(false);
        UpdateVisualState();
    }

    public void UpdateVisualState()
    {
        var index = transform.GetSiblingIndex();
        bool showEdit = (index == editableIndex) && (GameManager.Instance.IsAdminMode && GameManager.Instance.IsUserPointOpened);

        if(index == editableIndex)
        {
            groupLabel.text = GameManager.Instance.Settings.otherGroupName;
            inputField.text = GameManager.Instance.Settings.otherGroupName;
        }

        if(showEdit)
        {
            editButton.SetActive(showEdit);
        }
    }

    public void OnEditClick()
    {
        groupLabel.gameObject.SetActive(false);
        inputField.gameObject.SetActive(true);
        inputField.text = groupLabel.text;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        inputField.Select();
    }

    public void OnEditEnd()
    {
        OnEditEndAsync();
    }

    private async UniTask OnEditEndAsync()
    {
        if (!inputField.text.IsValuable())
        {
            inputField.text = "Прочее";
        }

        GameManager.Instance.Settings.otherGroupName = inputField.text;

        groupLabel.gameObject.SetActive(true);
        inputField.gameObject.SetActive(false);
        groupLabel.text = inputField.text;

        await GameManager.Instance.SaveSettings();
        Messager.Instance.Send<UpdateOtherGroupLabelMessage>();
    }

    private void OnDestroy()
    {
        Messager.Instance.RemoveSubscriber(updateSub);
    }
}

public class UpdateGroupDropdownMessage: Message { }
