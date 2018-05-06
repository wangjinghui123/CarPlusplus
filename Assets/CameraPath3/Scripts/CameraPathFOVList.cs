// Camera Path 3
// Available on the Unity Asset Store
// Copyright (c) 2013 Jasper Stocker http://support.jasperstocker.com/camera-path/
// For support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

public class CameraPathFOVList : CameraPathPointList
{
        public enum Interpolation
        {
            None,
            Linear,
            SmoothStep
        }
    
        public Interpolation interpolation = Interpolation.SmoothStep;

    private const float DEFAULT_FOV = 60;

    public bool enabled = true;

    public override void Init(CameraPath _cameraPath)
    {
        if(initialised)
            return;
        base.Init(_cameraPath);
        cameraPath.PathPointAddedEvent += AddFOV;
        pointTypeName = "FOV";
        initialised = true;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        cameraPath.PathPointAddedEvent -= AddFOV;
        initialised = false;
    }

    public CameraPathFOV this[int index] 
    {
        get { return ((CameraPathFOV)(base[index])); }
    }

    public void AddFOV(CameraPathControlPoint atPoint)
    {
        CameraPathFOV point = CreateInstance<CameraPathFOV>();
        point.FOV = defaultFOV;
        AddPoint(point,atPoint);
        RecalculatePoints();
    }

    public CameraPathFOV AddFOV(CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage, float fov)
    {
        CameraPathFOV fovpoint = CreateInstance<CameraPathFOV>();
        fovpoint.FOV = fov;
        AddPoint(fovpoint, curvePointA, curvePointB, curvePercetage);
        RecalculatePoints();
        return fovpoint;
    }

    public void SetDummyLastPoint(bool toggle)
    {
        if (toggle)
        {
            CameraPathFOV pathFov = CreateInstance<CameraPathFOV>();
            pathFov.FOV = cameraPath.nextPath.fovList[0].FOV;
            AddPoint(pathFov, cameraPath.dummyLastPoint);
            dummyLastPoint = pathFov;
        }
        else
        {
            DestroyImmediate(dummyLastPoint);
            dummyLastPoint = null;
        }
        RecalculatePoints();
    }

    public float GetFOV(float percentage)
    {
        if (realNumberOfPoints < 2)
        {
            if (realNumberOfPoints == 1)
                return (this[0]).FOV;
            return 60;
        }

        if (percentage >= 1)
            return ((CameraPathFOV)GetPoint(realNumberOfPoints - 1)).FOV;

        percentage = Mathf.Clamp(percentage, 0.0f, 0.999f);

        switch(interpolation)
        {
            case Interpolation.SmoothStep:
            return SmoothStepInterpolation(percentage);

            case Interpolation.Linear:
            return LinearInterpolation(percentage);

            default:
            return LinearInterpolation(percentage);
        }
    }

    private float LinearInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathFOV pointP = (CameraPathFOV)GetPoint(index);
        CameraPathFOV pointQ = (CameraPathFOV)GetPoint(index + 1);

        if (percentage < pointP.percent)
            return pointP.FOV;
        if (percentage > pointQ.percent)
            return pointQ.FOV;

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;
        return Mathf.Lerp(pointP.FOV, pointQ.FOV, ct);
    }

    private float SmoothStepInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathFOV pointP = (CameraPathFOV)GetPoint(index);
        CameraPathFOV pointQ = (CameraPathFOV)GetPoint(index + 1);

        if (percentage < pointP.percent)
            return pointP.FOV;
        if (percentage > pointQ.percent)
            return pointQ.FOV;

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;
        return Mathf.Lerp(pointP.FOV, pointQ.FOV, CPMath.SmoothStep(ct));
    }

    /// <summary>
    /// Attempt to find the camera in use for this scene and apply the field of view as default
    /// </summary>
    private float defaultFOV
    {
        get
        {
            if(Camera.current)
                return Camera.current.fieldOfView;

            Camera[] cams = Camera.allCameras;
            bool sceneHasCamera = cams.Length > 0;
            if(sceneHasCamera)
                return cams[0].fieldOfView;
            return DEFAULT_FOV;
        }
    }

    protected override void RecalculatePoints()
    {
        base.RecalculatePoints();
        if (dummyLastPoint != null)
        {
            ((CameraPathFOV)dummyLastPoint).FOV = cameraPath.nextPath.fovList[0].FOV;
        }
    }
}
