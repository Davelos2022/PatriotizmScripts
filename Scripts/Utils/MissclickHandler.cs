using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VolumeBox.Toolbox;
using static Tayx.Graphy.GraphyManager;

public class MissclickHandler : MonoCached
{
    [SerializeField] private bool useGraphicState;
    [SerializeField] private float clickOffsetThreshold = 1;
    [SerializeField] private bool enabledOnStart;
    [SerializeField] private bool missclickOnOtherScene;
    [SerializeField, Tooltip("Camera used by RectangleContainsScreenPoint method. May be null")] private Camera testCamera;

    public UnityEvent Missclicked;
    public UnityEvent Clicked;

    private bool clickedOtherScene = false;
    private bool pointerDownInRectangle = false;
    private Vector2 pointerDownPosition = Vector2.zero;

    private bool _enabled = false;

    private ExtendedInputModule inp;

    protected override void Rise()
    {
        inp = EventSystem.current.currentInputModule as ExtendedInputModule;
        _enabled = enabledOnStart;
    }

    public void EnableCheckDelayed(float delay = 0.1f)
    {
        Invoke(nameof(EnableCheck), delay);
    }

    public void EnableCheck()
    {
        _enabled = true;
    }

    public void DisableCheck()
    {
        _enabled = false;
    }

    protected override void Tick()
    {
        if(this == null)
        {
            return;
        }
        
        if (!_enabled) return;

        if (Input.GetMouseButtonDown(0))
        {
            var clickedObject = inp.GameObjectUnderPointer();

            if (useGraphicState && (clickedObject != gameObject))
            {
                return;
            }

            if (clickedObject != null && clickedObject.scene.name != gameObject.scene.name)
            {
                clickedOtherScene = true;
            }

            var vector = Input.mousePosition;// / mainCanvas.scaleFactor;

            if (ContainsPoint(vector))
            {
                pointerDownPosition = vector;
                pointerDownInRectangle = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && gameObject.activeInHierarchy)
        {
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(mainCanvas.transform as RectTransform, Input.mousePosition, Camera.main, out Vector2 vector);

            var mousePos = Input.mousePosition;// / mainCanvas.scaleFactor;

            StartCoroutine(OnMouseUpCheck(mousePos));
        }
    }

    private IEnumerator OnMouseUpCheck(Vector3 mousePosition)
    {
        if (!gameObject.activeInHierarchy || !_enabled) yield break;

        yield return new WaitForEndOfFrame();

        if (pointerDownInRectangle)
        {
            if(missclickOnOtherScene && clickedOtherScene)
            {
                clickedOtherScene = false;
                pointerDownInRectangle = false;
                yield break;
            }

            if(ContainsPoint(mousePosition) && Vector2.Distance(mousePosition, pointerDownPosition) <= clickOffsetThreshold)
            {
                Clicked?.Invoke();
            }

            pointerDownInRectangle = false;
            yield break;
        }
        else
        {
            if(clickedOtherScene && !missclickOnOtherScene)
            {
                clickedOtherScene = false;
                yield break;
            }

            pointerDownInRectangle = false;
            Missclicked.Invoke();
        }
    }

    private bool ContainsPoint(Vector2 pos)
    {
        if (testCamera == null)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(Rect, pos);
        }
        else
        {
            return RectTransformUtility.RectangleContainsScreenPoint(Rect, pos, testCamera);
        }
    }

    protected override void Destroyed()
    {
        _enabled = false;
    }
}
