// Camera Path 3
// Available on the Unity Asset Store
// Copyright (c) 2013 Jasper Stocker http://support.jasperstocker.com/camera-path/
// For support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;

public class CameraPathTiltList : CameraPathPointList
{
    public enum Interpolation
    {
        None,
        Linear,
        SmoothStep
    }

    public Interpolation interpolation = Interpolation.SmoothStep;

    public bool enabled = true;
    public float autoSensitivity = 1.0f;

    public override void Init(CameraPath _cameraPath)
    {
        if (initialised)
            return;
        base.Init(_cameraPath);
        cameraPath.PathPointAddedEvent += AddTilt;
        pointTypeName = "Tilt";
        initialised = true;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        cameraPath.PathPointAddedEvent -= AddTilt;
        initialised = false;
    }

    public CameraPathTilt this[int index] 
    {
        get { return ((CameraPathTilt)(base[index])); }
    }

    public void AddTilt(CameraPathControlPoint atPoint)
    {
        CameraPathTilt point = CreateInstance<CameraPathTilt>();
        point.tilt = 0;
        AddPoint(point,atPoint);
        RecalculatePoints();
    }

    public CameraPathTilt AddTilt(CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage, float tilt)
    {
        CameraPathTilt tiltPoint = CreateInstance<CameraPathTilt>();
        tiltPoint.tilt = tilt;
        AddPoint(tiltPoint, curvePointA, curvePointB, curvePercetage);
        RecalculatePoints();
        return tiltPoint;
    }

    public void SetDummyLastPoint(bool toggle)
    {
        if (toggle)
        {
            CameraPathTilt point = CreateInstance<CameraPathTilt>();
            point.tilt = cameraPath.nextPath.tiltList[0].tilt;
            AddPoint(point, cameraPath.dummyLastPoint);
            dummyLastPoint = point;
        }
        else
        {
            DestroyImmediate(dummyLastPoint);
            dummyLastPoint = null;
        }
        RecalculatePoints();
    }

    public float GetTilt(float percentage)
    {
        if (realNumberOfPoints < 2)
        {
            if (realNumberOfPoints == 1)
                return (this[0]).tilt;
            return 0;
        }

        if (percentage >= 1)
            return ((CameraPathTilt)GetPoint(realNumberOfPoints - 1)).tilt;

        percentage = Mathf.Clamp(percentage, 0.0f, 0.999f);

        switch(interpolation)
        {
            case Interpolation.SmoothStep:
                return SmoothStepInterpolation(percentage);

            case Interpolation.Linear:
                return LinearInterpolation(percentage);

            case Interpolation.None:
                CameraPathTilt point = (CameraPathTilt)GetPoint(GetNextPointIndex(percentage));
                return point.tilt;

            default:
                return LinearInterpolation(percentage);
        }
    }

    public void AutoSetTilts()
    {
        for(int i = 0; i < realNumberOfPoints; i++)
        {
            AutoSetTilt(this[i]);
        }
    }

    public void AutoSetTilt(CameraPathTilt point)
    {
        float tiltPercentage = point.percent;
        Vector3 pointA = cameraPath.GetPathPosition(tiltPercentage - 0.1f);
        Vector3 pointB = cameraPath.GetPathPosition(tiltPercentage);
        Vector3 pointC = cameraPath.GetPathPosition(tiltPercentage + 0.1f);

        Vector3 directionAB = pointB - pointA;
        Vector3 directionBC = pointC - pointB;
        Quaternion angle = Quaternion.LookRotation(-cameraPath.GetPathDirection(point.percent));
        Vector3 pathCurveDirection = angle * (directionBC - directionAB).normalized;
        float curveAngle = Vector2.Angle(Vector2.up, new Vector2(pathCurveDirection.x, pathCurveDirection.y));
        float ratio = Mathf.Min(Mathf.Abs(pathCurveDirection.x) + Mathf.Abs(pathCurveDirection.y) / Mathf.Abs(pathCurveDirection.z),1.0f);

        point.tilt = -curveAngle * autoSensitivity * ratio;
    }

    private float LinearInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathTilt pointP = (CameraPathTilt)GetPoint(index);
        CameraPathTilt pointQ = (CameraPathTilt)GetPoint(index + 1);

        if (percentage < pointP.percent)
            return pointP.tilt;
        if (percentage > pointQ.percent)
            return pointQ.tilt;

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;
        return Mathf.Lerp(pointP.tilt, pointQ.tilt, ct);
    }

    private float SmoothStepInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathTilt pointP = (CameraPathTilt)GetPoint(index);
        CameraPathTilt pointQ = (CameraPathTilt)GetPoint(index + 1);

        if (percentage < pointP.percent)
            return pointP.tilt;
        if (percentage > pointQ.percent)
            return pointQ.tilt;

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;
        return Mathf.Lerp(pointP.tilt, pointQ.tilt, CPMath.SmoothStep(ct));
    }

    protected override void RecalculatePoints()
    {
        base.RecalculatePoints();
        if (dummyLastPoint != null)
        {
            ((CameraPathTilt)dummyLastPoint).tilt = cameraPath.nextPath.tiltList[0].tilt;
        }
    }
}
