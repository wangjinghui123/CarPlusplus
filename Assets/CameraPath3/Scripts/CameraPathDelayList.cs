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

public class CameraPathDelayList : CameraPathPointList
{
    public delegate void CameraPathDelayEventHandler(float time);
    public event CameraPathDelayEventHandler CameraPathDelayEvent;
    private float _lastPercentage = 0;

    [SerializeField]
    private CameraPathDelay _introPoint;
    [SerializeField]
    private CameraPathDelay _outroPoint;

    [SerializeField]
    private bool delayInitialised;

    public CameraPathDelay this[int index]
    {
        get { return ((CameraPathDelay)(base[index])); }
    }

    public CameraPathDelay introPoint {get {return _introPoint;}}

    public CameraPathDelay outroPoint {get {return _outroPoint;}}

    public override void Init(CameraPath _cameraPath)
    {
        base.Init(_cameraPath);

        if(!delayInitialised)
        {
            _introPoint = CreateInstance<CameraPathDelay>();
            _introPoint.customName = "Start Point";
            AddPoint(introPoint, 0);
            _outroPoint = CreateInstance<CameraPathDelay>();
            _outroPoint.customName = "End Point";
            AddPoint(outroPoint, 1);
            RecalculatePoints();
            delayInitialised = true;
        }

        pointTypeName = "Delay";
    }

    public void AddDelayPoint(CameraPathControlPoint atPoint)
    {
        CameraPathDelay point = CreateInstance<CameraPathDelay>();
        AddPoint(point, atPoint);
        RecalculatePoints();
    }

    public CameraPathDelay AddDelayPoint(CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage)
    {
        CameraPathDelay point = CreateInstance<CameraPathDelay>();
        AddPoint(point, curvePointA, curvePointB, curvePercetage);
        RecalculatePoints();
        return point;
    }

    public void OnAnimationStart(float startPercentage)
    {
        _lastPercentage = startPercentage;
    }

    public void CheckEvents(float percentage)
    {
        if (Mathf.Abs(percentage - _lastPercentage) > 0.1f)
        {
            _lastPercentage = percentage;//probable loop/seek
            return;
        }

        for (int i = 0; i < realNumberOfPoints; i++)
        {
            CameraPathDelay eventPoint = this[i];

            if(eventPoint == introPoint)
                continue;

            if(eventPoint == outroPoint)
                continue;

            if (eventPoint.percent > _lastPercentage && eventPoint.percent < percentage)
            {
                FireDelay(eventPoint);
            }
        }

        _lastPercentage = percentage;
    }

    public float CheckEase(float percent)
    {
        float output = 1.0f;

        for (int i = 0; i < realNumberOfPoints; i++)
        {
            CameraPathDelay eventPoint = this[i];

            if(eventPoint != introPoint)
            {
                CameraPathDelay earlierPoint = (CameraPathDelay)GetPoint(i - 1);
                float pathIntroPercent = cameraPath.GetPathPercentage(earlierPoint.percent, eventPoint.percent, 1-eventPoint.introStartEasePercentage);
                if (pathIntroPercent < percent && eventPoint.percent > percent)
                {
                    float animCurvePercent = (percent - pathIntroPercent) / (eventPoint.percent - pathIntroPercent);
                    output = eventPoint.introCurve.Evaluate(animCurvePercent);
                }
            }

            if(eventPoint != outroPoint)
            {
                CameraPathDelay laterPoint = (CameraPathDelay)GetPoint(i + 1);
                float pathOutroPercent = cameraPath.GetPathPercentage(eventPoint.percent, laterPoint.percent, eventPoint.outroEndEasePercentage);
                if (eventPoint.percent < percent && pathOutroPercent > percent)
                {
                    float animCurvePercent = (percent - eventPoint.percent) / (pathOutroPercent - eventPoint.percent);
                    output = eventPoint.outroCurve.Evaluate(animCurvePercent);
                }
            }
        }
        return output;
    }

    public void FireDelay(CameraPathDelay eventPoint)
    {
        if (CameraPathDelayEvent != null)
            CameraPathDelayEvent(eventPoint.time);
    }
}