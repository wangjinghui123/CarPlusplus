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

public class CameraPathDelay : CameraPathPoint
{
    public float time = 0.0f;

    //intro ease curve
    public float introStartEasePercentage = 0.1f;
    public AnimationCurve introCurve = AnimationCurve.Linear(0, 1, 1, 1);

    //exit ease curve
    public float outroEndEasePercentage = 0.1f;
    public AnimationCurve outroCurve = AnimationCurve.Linear(0, 1, 1, 1);
}