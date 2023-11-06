using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VolumeBox.Toolbox.UIInformer;

public abstract class UndoRecorder : MonoBehaviour
{
    private void Start()
    {
        UndoController.ClearEvent += ClearHandle;
        Initialize();
    }

    protected virtual void Initialize()
    {

    }

    protected abstract void RestoreStateHandle(object state);

    protected abstract void ClearHandle();

    public void RestoreState(object state)
    {
        RestoreStateHandle(state);
    }
    
    public void Record()
    {
        if (!RecordCheck()) return;

        UndoController.AddRecord(this, RecordHandle()); 
    }

    protected virtual bool RecordCheck()
    {
        return true;
    }

    protected abstract object RecordHandle();

    private void OnDestroy()
    {
        UndoController.ClearEvent -= ClearHandle;
    }
}
