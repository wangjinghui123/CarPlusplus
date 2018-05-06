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

public class CameraPathEventList : CameraPathPointList
{
    public delegate void CameraPathEventPointHandler(string name);
    public event CameraPathEventPointHandler CameraPathEventPoint;
    private float _lastPercentage;

    public CameraPathEvent this[int index]
    {
        get { return ((CameraPathEvent)(base[index])); }
    }

    public override void Init(CameraPath _cameraPath)
    {
        pointTypeName = "Event";
        base.Init(_cameraPath);
    }

    public void AddEvent(CameraPathControlPoint atPoint)
    {
        CameraPathEvent point = CreateInstance<CameraPathEvent>();
        AddPoint(point, atPoint);
        RecalculatePoints();
    }

    public CameraPathEvent AddEvent(CameraPathControlPoint curvePointA, CameraPathControlPoint curvePointB, float curvePercetage)
    {
        CameraPathEvent eventPoint = CreateInstance<CameraPathEvent>();
        AddPoint(eventPoint, curvePointA, curvePointB, curvePercetage);
        RecalculatePoints();
        return eventPoint;
    }

    public void OnAnimationStart(float startPercentage)
    {
        _lastPercentage = startPercentage;
    }

    public void CheckEvents(float percentage)
    {
        if(Mathf.Abs(percentage - _lastPercentage) > 0.999f)
        {
            _lastPercentage = percentage;//probable loop
            return;
        }

        for(int i = 0; i < realNumberOfPoints; i++)
        {
            CameraPathEvent eventPoint = this[i];
            if (eventPoint.percent > _lastPercentage && eventPoint.percent < percentage)
            {
                switch(eventPoint.type)
                {
                    case CameraPathEvent.Types.Broadcast:
                        BroadCast(eventPoint);
                        break;

                    case CameraPathEvent.Types.Call:
                        Call(eventPoint);
                        break;
                }
            }
        }

        _lastPercentage = percentage;
    }

    public void BroadCast(CameraPathEvent eventPoint)
    {
        if(CameraPathEventPoint != null)
        {
            CameraPathEventPoint(eventPoint.eventName);
        }
    }

    public void Call(CameraPathEvent eventPoint)
    {
        eventPoint.target.SendMessage(eventPoint.methodName, SendMessageOptions.DontRequireReceiver);
    }
}
