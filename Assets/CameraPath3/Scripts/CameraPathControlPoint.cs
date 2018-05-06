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

public class CameraPathControlPoint : ScriptableObject
{
    public string customName = "";

    public Transform baseTransform;
    [SerializeField]
    private Vector3 _position;

    //Bezier Control Points
    [SerializeField]
    private bool _splitControlPoints = false;
    [SerializeField]
    private Vector3 _forwardControlPoint;
    [SerializeField]
    private Vector3 _backwardControlPoint;

    //Internal stored calculations
    [SerializeField]
    private Vector3 _pathDirection = Vector3.forward;

    public int index = 0;
    public float percentage = 0;
    public float normalisedPercentage = 0;

    public Vector3 localPosition
    {
        get
        {
            return baseTransform.rotation * _position;
        }
        set
        {
            Vector3 newValue = value;
            newValue = Quaternion.Inverse(baseTransform.rotation) * newValue;
            _position = newValue;
        }
    }
    public Vector3 worldPosition
    {
        get
        {
            return baseTransform.rotation * _position + baseTransform.position;
        }
        set
        {
            Vector3 newValue = value - baseTransform.position;
            newValue = Quaternion.Inverse(baseTransform.rotation) * newValue;
            _position = newValue;
        }
    }

    public Vector3 forwardControlPoint
    {
        get
        {
            return baseTransform.rotation * (_forwardControlPoint + _position);
        }
        set
        {
            Vector3 newValue = value;
            newValue = Quaternion.Inverse(baseTransform.rotation) * newValue;
            newValue += -_position;
            _forwardControlPoint = newValue;
        }
    }

    public Vector3 forwardControlPointLocal
    {
        get
        {
            return baseTransform.rotation * _forwardControlPoint;
        }
        set
        {
            Vector3 newValue = value;
            newValue = Quaternion.Inverse(baseTransform.rotation) * newValue;
            _forwardControlPoint = newValue;
        }
    }

    public Vector3 backwardControlPoint
    {
        get
        {
            Vector3 controlPoint = (_splitControlPoints) ? _backwardControlPoint : -_forwardControlPoint;
            return baseTransform.rotation * (controlPoint + _position);
        }
        set
        {
            Vector3 newValue = value;
            newValue = Quaternion.Inverse(baseTransform.rotation) * newValue;
            newValue += -_position;
            if (_splitControlPoints)
                _backwardControlPoint = newValue;
            else
                _forwardControlPoint = -newValue;
        }
    }

    public bool splitControlPoints
    {
        get { return _splitControlPoints; }
        set
        {
            if (value != _splitControlPoints)
                _backwardControlPoint = -_forwardControlPoint;
            _splitControlPoints = value;
        }
    }

    public Vector3 trackDirection
    {
        get
        {
            return _pathDirection;
        }
        set
        {
            if (value == Vector3.zero)
                return;
            _pathDirection = value.normalized;
        }
    }

    public string displayName
    {
        get
        {
            if (customName != "")
                return customName;
            return name;
        }
    }
}
