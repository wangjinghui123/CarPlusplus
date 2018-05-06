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

public class CameraPathOrientationList : CameraPathPointList
{
        public enum Interpolation
        {
            None,
            Linear,
            SmoothStep,
            Hermite,
            Cubic
        }
    
        public Interpolation interpolation = Interpolation.Cubic;

    [SerializeField]
    private Quaternion[] _storedRotations = new Quaternion[0];
    [SerializeField]
    private float[] _orientationRatios = new float[0];

    public override void Init(CameraPath _cameraPath)
    {
        if (initialised)
            return;

        pointTypeName = "Orientation";
        base.Init(_cameraPath);
        cameraPath.PathPointAddedEvent += AddOrientation;
        initialised = true;
    }

    public override void CleanUp()
    {
        base.CleanUp();
        cameraPath.PathPointAddedEvent -= AddOrientation;
        initialised = false;
    }

    public CameraPathOrientation this[int index] 
    {
        get {return ((CameraPathOrientation)(base[index]));}
    }

    public void AddOrientation(CameraPathControlPoint atPoint)
    {
        CameraPathOrientation orientation = CreateInstance<CameraPathOrientation>();
        if (atPoint.forwardControlPoint != Vector3.zero)
            orientation.rotation = Quaternion.LookRotation(atPoint.forwardControlPoint);
        else
            orientation.rotation = Quaternion.LookRotation(cameraPath.GetPathDirection(atPoint.percentage));
        AddPoint(orientation, atPoint);
        RecalculatePoints();
        //return orientation;
    }

    public CameraPathOrientation AddOrientation(CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage, Quaternion rotation)
    {
        CameraPathOrientation orientation = CreateInstance<CameraPathOrientation>();
        orientation.rotation = rotation;
        AddPoint(orientation, curvePointA, curvePointB, curvePercetage);
        RecalculatePoints();
        return orientation;
    }

    public void SetDummyLastPoint(bool toggle)
    {
        if(toggle)
        {
            if(dummyLastPoint!=null)
                DestroyImmediate(dummyLastPoint);
            CameraPathOrientation orientation = CreateInstance<CameraPathOrientation>();
            orientation.rotation = cameraPath.nextPath.orientationList[0].rotation;
            AddPoint(orientation, cameraPath.dummyLastPoint);
            dummyLastPoint = orientation;
        }
        else
        {
            DestroyImmediate(dummyLastPoint);
            dummyLastPoint = null;
        }
        RecalculatePoints();
    }

    public void RemovePoint(CameraPathOrientation orientation)
    {
        base.RemovePoint(orientation);
        RecalculatePoints();
    }

    public Quaternion GetOrientation(float percentage)
    {
        if (realNumberOfPoints < 2)
        {
            if (realNumberOfPoints == 1)
                return (this[0]).rotation;
            return Quaternion.identity;
        }

        if (percentage >= 1)
            return ((CameraPathOrientation)GetPoint(realNumberOfPoints - 1)).rotation;

        percentage = Mathf.Clamp(percentage, 0.0f, 0.999f);

        Quaternion returnQ = Quaternion.identity;
        switch (interpolation)
        {
            case Interpolation.Cubic:
                returnQ = CubicInterpolation(percentage);
                break;

            case Interpolation.Hermite:
                returnQ = CubicInterpolation(percentage);
                break;

            case Interpolation.SmoothStep:
                returnQ = SmootStepInterpolation(percentage);
                break;

            case Interpolation.Linear:
                returnQ = LinearInterpolation(percentage);
                break;

            case Interpolation.None:
                CameraPathOrientation point = (CameraPathOrientation)GetPoint(GetNextPointIndex(percentage));
                returnQ = point.rotation;
                break;

            default:
                returnQ = Quaternion.LookRotation(Vector3.forward);
                break;
        }
        return returnQ;
    }

    private Quaternion LinearInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathOrientation pointP = (CameraPathOrientation)GetPoint(index);
        CameraPathOrientation pointQ = (CameraPathOrientation)GetPoint(index + 1);

        if (percentage < pointP.percent)
            return pointP.rotation;
        if (percentage > pointQ.percent)
            return pointQ.rotation;

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;
        return Quaternion.Lerp(pointP.rotation, pointQ.rotation, ct);
    }

    private Quaternion SmootStepInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathOrientation pointP = (CameraPathOrientation)GetPoint(index);
        CameraPathOrientation pointQ = (CameraPathOrientation)GetPoint(index + 1);

        if (percentage < pointP.percent)
            return pointP.rotation;
        if (percentage > pointQ.percent)
            return pointQ.rotation;

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;

        Quaternion returnQ = Quaternion.Lerp(pointP.rotation, pointQ.rotation, CPMath.SmoothStep(ct));
        return returnQ;
    }

    private Quaternion CubicInterpolation(float percentage)
    {
        int index = GetLastPointIndex(percentage);
        CameraPathOrientation pointP = (CameraPathOrientation)GetPoint(index);
        CameraPathOrientation pointQ = (CameraPathOrientation)GetPoint(index + 1);


        if (percentage < pointP.percent)
            return pointP.rotation;
        if (percentage > pointQ.percent)
            return pointQ.rotation;

        CameraPathOrientation pointA = (CameraPathOrientation)GetPoint(index - 1);
        CameraPathOrientation pointB = (CameraPathOrientation)GetPoint(index + 2);

        float startPercentage = pointP.percent;
        float endPercentage = pointQ.percent;

        if (startPercentage > endPercentage)
            endPercentage += 1;

        float curveLength = endPercentage - startPercentage;
        float curvePercentage = percentage - startPercentage;
        float ct = curvePercentage / curveLength;

        Quaternion returnQ = CPMath.CalculateCubic(pointP.rotation, pointA.rotation, pointB.rotation, pointQ.rotation, ct);
        return returnQ;
    }

    protected override void RecalculatePoints()
    {
        base.RecalculatePoints();

        for(int i = 0; i < realNumberOfPoints; i++)
        {
            CameraPathOrientation point = this[i];
            if(point.lookAt != null)
                point.rotation = Quaternion.LookRotation(point.lookAt.transform.position - point.worldPosition);
        }

        if(dummyLastPoint != null)
        {
            ((CameraPathOrientation)dummyLastPoint).rotation = cameraPath.nextPath.orientationList[0].rotation;
        }
    }
}
