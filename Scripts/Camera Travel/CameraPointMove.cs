using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumeBox.Toolbox;

public class CameraPointMove : MonoCached
{
    [SerializeField] private float moveDuration;
    [SerializeField] private Ease moveEase;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CameraPoint[] points;

    public void MoveTo(int pointIndex)
    {
        if(points == null || points.Length <= 0)
        {
            return;
        }

        CameraPoint p = points[pointIndex];

        cameraTransform.DOMove(p.Position, moveDuration).SetEase(moveEase);
        cameraTransform.DORotate(p.Rotation, moveDuration).SetEase(moveEase);
        cameraTransform.DORotate(p.Rotation, moveDuration).SetEase(moveEase);
    }
}

[Serializable]
public class CameraPoint
{
    public Vector3 Position;
    public Vector3 Rotation;
}