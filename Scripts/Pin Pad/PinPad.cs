using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using VolumeBox.Toolbox;
using VolumeBox.Toolbox.UIInformer;

public class PinPad : CachedSingleton<PinPad>
{
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private MissclickHandler missclickHandler;
    [SerializeField] private List<PinInput> inputs;

    private Action _currentAction;
    private Action<string> _currentInputAction;

    public UnityEvent OpenedEvent;
    public UnityEvent ClosedEvent;

    private bool _opened;

    public void OpenAt(Vector2 pos, Action next, string text = null, float delay = 0)
    {
        OpenAtAsync(pos, next, text, delay);
    }

    private async UniTask OpenAtAsync(Vector2 pos, Action next, string text = null, float delay = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        if (_opened) return;

        _opened = true;

        _currentInputAction = null;
        _currentAction = next;

        headerText.text = text;

        Rect.anchoredPosition = pos;

        ClearAllInputs();

        OpenedEvent.Invoke();

        Invoke(nameof(OnOpenMissclick), 0.1f);
    }

    public void GetInput(Vector2 pos, Action<string> next, string text = null, float delay = 0)
    {
        GetInputAsync(pos, next, text, delay);
    }

    public async UniTask GetInputAsync(Vector2 pos, Action<string> next, string text = null, float delay = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        if (_opened) return;

        _opened = true;

        _currentAction = null;
        _currentInputAction = next;

        headerText.text = text;

        Rect.anchoredPosition = pos;

        ClearAllInputs();

        OpenedEvent.Invoke();

        Invoke(nameof(OnOpenMissclick), 0.1f);
    }

    private void OnOpenMissclick()
    {
        missclickHandler.EnableCheck();
    }

    public void ClearLast()
    {
        var input = inputs.FindLast(i => i.CurrentValue.HasValue);

        if(input != null)
        {
            input.ClearValue();
        }
    }

    public void ClearAllInputs()
    {
        inputs.ForEach(i => i.ClearValue());
    }

    public void Enter(string val)
    {
        var emptyInput = inputs.FirstOrDefault(x => !x.CurrentValue.HasValue);

        if(emptyInput == null)
        {
            return;
        }
        else
        {
            emptyInput.SetValue(val[0]);
        }

        CheckInput();
    }

    private void CheckInput()
    {
        if (inputs.All(i => i.CurrentValue.HasValue))
        {
            string pinInput = string.Empty;

            foreach (var input in inputs)
            {
                pinInput += Char.ToString(input.CurrentValue.Value);
            }

            if(_currentAction != null)
            {
                if(pinInput == GameManager.Instance.Settings.pinCode)
                {
                    _currentAction?.Invoke();
                    Close();
                }
                else
                {
                    Info.Instance.ShowHint("Неверный пин-код");
                    ClearAllInputs();
                }
            }

            if(_currentInputAction != null)
            {
                _currentInputAction.Invoke(pinInput);
                Close();
            }
        }
    }

    public void Close()
    {
        if (!_opened) return;

        _opened = false;
        missclickHandler.DisableCheck();
        ClearAllInputs();
        ClosedEvent.Invoke();
    }
}
