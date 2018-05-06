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

public class CameraPathPoint : ScriptableObject
{
    public enum PositionModes
    {
        Free,
        FixedToPoint,
        FixedToPercent
    }

    public PositionModes positionModes = PositionModes.Free;
    public string customName = "";

    [SerializeField]
    protected float _percent = 0;

    [SerializeField]
    protected float _animationPercentage = 0;

    public CameraPathControlPoint point = null;

    public int index = 0;

    //free point values - calculated by the CameraPathPointList
    public CameraPathControlPoint cpointA;
    public CameraPathControlPoint cpointB;
    public float curvePercentage = 0;

    public Vector3 worldPosition;

    public bool lockPoint = false;

    public float percent
    {
        get
        {
            switch (positionModes)
            {
                case PositionModes.Free:
                    return _percent;

                case PositionModes.FixedToPercent:
                    return _percent;

                case PositionModes.FixedToPoint:
                    return point.percentage;
            }
            return _percent;
        }
        set { _percent = value; }
    }

    public float animationPercentage
    {
        get
        {
            switch (positionModes)
            {
                case PositionModes.Free:
                    return _animationPercentage;

                case PositionModes.FixedToPercent:
                    return _animationPercentage;

                case PositionModes.FixedToPoint:
                    return point.normalisedPercentage;
            }
            return _percent;
        }
        set { _animationPercentage = value; }
    }

    public string displayName
    {
        get
        {
            if(customName != "")
                return customName;
            else
                return name;
        }
    }
}