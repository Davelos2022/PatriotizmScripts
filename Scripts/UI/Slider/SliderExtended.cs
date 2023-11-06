using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.UI;
using UnityEditor;
#endif

public class SliderExtended : Slider
{
    public UnityEvent OnMaxReached;
    public UnityEvent OnMinReached;
    public UnityEvent OnMovedFromMax;
    public UnityEvent OnMovedFromMin;

    private float m_prevValue;

    protected override void Awake()
    {
        base.Awake();

        m_prevValue = m_Value;
    }

    protected override void Set(float input, bool sendCallback = true)
    {
        float b_val = m_Value;

        base.Set(input, sendCallback);

        if (b_val == ClampValue(input))
        {
            return;
        }

        if(m_Value <= 0 && m_Value != m_prevValue)
        {
            OnMinReached?.Invoke();
        }
        else if(m_Value >= 1 && m_Value != m_prevValue)
        {
            OnMaxReached?.Invoke();
        }

        if(m_Value < 1 && m_prevValue >= 1)
        {
            OnMovedFromMax?.Invoke();
        }

        if(m_Value > 0 && m_prevValue <= 0)
        {
            OnMovedFromMin?.Invoke();
        }

        m_prevValue = m_Value;
    }

    protected float ClampValue(float input)
    {
        float newValue = Mathf.Clamp(input, minValue, maxValue);
        if (wholeNumbers)
            newValue = Mathf.Round(newValue);
        return newValue;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SliderExtended), true)]
public class SliderExtendedEditor: SliderEditor
{
    private Editor defaultEditor;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnMaxReached"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnMinReached"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnMovedFromMax"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnMovedFromMin"));

        serializedObject.ApplyModifiedProperties();
        EditorGUI.EndChangeCheck();

    }
}

#endif