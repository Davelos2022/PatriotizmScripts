
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoControllerComponent : MonoBehaviour
{
    [ContextMenu("Undo")]
    public void Undo()
    {
        UndoController.Undo();
    }

    [ContextMenu("Redo")]
    public void Redo()
    {
        UndoController.Redo();
    }

    public void ClearHistory()
    {
        UndoController.ClearHistory();
    }
}
