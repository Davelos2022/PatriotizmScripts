using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Serialization;
using VolumeBox.Toolbox;
using VolumeBox.Toolbox.UIInformer;

public class SettingsView : MonoBehaviour
{
    [SerializeField] private Vector2 pinPosition;
    [SerializeField] private ToggleButton musicByDefaultEnabled;
    [SerializeField] private ToggleButton uiSoundToggle;
    [SerializeField] private ToggleButton drawToggle;
    [SerializeField] private ToggleButton volumeSliderToggle;
    [SerializeField] private ToggleButton screenshotToggle;
    [Space]
    [SerializeField] private TextMeshProUGUI screenshotPathText;
    [SerializeField] private TextMeshProUGUI drawingPathText;

    private string drawingsSavePath;
    private string screenshotsSavePath;

    public UnityEvent OnOpen;
    public UnityEvent OnClose;

    private string _changedPin;

    private AppSettings _editedSettings;

    public void OpenPanel()
    {
        _editedSettings = ScriptableObject.CreateInstance<AppSettings>();

        GameManager.Instance.Settings.CopyTo(_editedSettings);

        drawingsSavePath = _editedSettings.drawingsPath;
        screenshotsSavePath = _editedSettings.screenshotsPath;

        screenshotPathText.text = screenshotsSavePath;
        drawingPathText.text = drawingsSavePath;

        uiSoundToggle.SetState(_editedSettings.uiSoundEnabled);
        musicByDefaultEnabled.SetState(_editedSettings.musicByDefaultEnabled);
        drawToggle.SetState(_editedSettings.drawingEnabled);
        volumeSliderToggle.SetState(_editedSettings.volumeSliderEnabled);
        screenshotToggle.SetState(_editedSettings.screenshotEnabled);

        OnOpen.Invoke();
    }

    public void ChangePin()
    {
        PinPad.Instance.OpenAt(pinPosition, EnterNewPin, "Введите текущий пароль");
    }

    private void EnterNewPin()
    {
        PinPad.Instance.GetInput(pinPosition, AcceptNewPin, "Введите новый пароль", 1);
    }

    private void AcceptNewPin(string newPin)
    {
        _changedPin = newPin;
        PinPad.Instance.GetInput(pinPosition, CheckNewPin, "Подтвердите новый пароль", 1);
    }

    private void CheckNewPin(string newPin)
    {
        if(newPin == _changedPin)
        {
            _editedSettings.pinCode = newPin;
            OnCorrectChange();
        }
        else
        {
            Info.Instance.ShowHint("Пароли не совпадают");
            AcceptNewPin(_changedPin);
        }
    }

    private async UniTask OnCorrectChange()
    {
        await LoaderScreen.ShowAsync();
        await GameManager.Instance.SaveSettings();
        await LoaderScreen.HideAsync();
    }

    public void SelectDrawingsFolder()
    {
        var newDrawingsPath = FileManager.SelectFolderInBrowser();

        if(!newDrawingsPath.IsValuable())
        {
            return;
        }

        drawingsSavePath = newDrawingsPath;

        drawingPathText.text = drawingsSavePath;
        TruncateText(drawingPathText);
    }

    public void SelectScreenshotsFolder()
    {
        var newScreenshotsPath = FileManager.SelectFolderInBrowser();

        if(!newScreenshotsPath.IsValuable())
        {
            return;
        }

        screenshotsSavePath = newScreenshotsPath;

        screenshotPathText.text = screenshotsSavePath;
        TruncateText(screenshotPathText);
    }

    public void ClosePanel()
    {
        ClosePanelAsync();
    }

    public static void TruncateText(TMP_Text textField)
    {
        string text = textField.text;
        int lastIndex = text.Length - 1;
        if (lastIndex < 10) //isNullOrEmpty as well as isNullOrWhitespace do not return true reliably
        {
            return; // 8 chars always fit
        }
        // force it to calculate the character layout.
        textField.ForceMeshUpdate();
        TMP_TextInfo textInfo = textField.textInfo;

        // Get the range between the first and last character in the line.        
        float start = textInfo.characterInfo[0].origin;
        float end = textInfo.characterInfo[lastIndex].xAdvance;
        float budget = textField.rectTransform.rect.width;

        int i;
        for (i = lastIndex; i > -1; i--)
        {
            TMP_CharacterInfo info = textInfo.characterInfo[i];
            start = info.origin;
            if (end - start > budget) break;
        }

        // Take only characters to the right of our first overflow.
        textField.SetText(text.Substring(i + 1));
    }

    private async UniTask ClosePanelAsync()
    {
        _editedSettings.uiSoundEnabled = uiSoundToggle.IsEnabled;
        _editedSettings.drawingEnabled = drawToggle.IsEnabled;
        _editedSettings.musicByDefaultEnabled = musicByDefaultEnabled.IsEnabled;
        _editedSettings.screenshotEnabled = screenshotToggle.IsEnabled;
        _editedSettings.volumeSliderEnabled = volumeSliderToggle.IsEnabled;
        _editedSettings.drawingsPath = drawingsSavePath;
        _editedSettings.screenshotsPath = screenshotsSavePath;

        if (!_editedSettings.Equals(GameManager.Instance.Settings))
        {
            await LoaderScreen.ShowAsync();

            _editedSettings.CopyTo(GameManager.Instance.Settings);

            await GameManager.Instance.SaveSettings();

            await LoaderScreen.HideAsync();

            Notifier.Instance.Notify(NotifyType.Default, "Параметры сохранены");

            Messager.Instance.Send<SettingsChangedMessage>();
        }

        OnClose.Invoke();
    }
}
