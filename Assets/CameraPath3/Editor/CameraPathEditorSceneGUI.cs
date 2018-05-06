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
using UnityEditor;

public class CameraPathEditorSceneGUI
{
    private const float LINE_RESOLUTION = 0.005f;
    private const float HANDLE_SCALE = 0.1f;

    public static CameraPathEditor.PointModes pointMode = CameraPathEditor.PointModes.Transform;

    public static CameraPath _cameraPath;
    public static CameraPathAnimator _animator;
    public static int selectedPointIndex = 0;//selected track point
    private static Vector3 cpPosition;

    public delegate void NewPointSelectedHandler(int selectedPointIndex);
    public static event NewPointSelectedHandler NewPointSelected;

    public static void OnSceneGUI()
    {
        if(!_cameraPath.showGizmos)
            return;
        if(_cameraPath.transform.rotation != Quaternion.identity)
            return;

        cpPosition = _cameraPath.transform.position;

        if (SceneView.focusedWindow != null)
            SceneView.focusedWindow.wantsMouseMove = false;

        //draw small point indicators
        Handles.color = CameraPathColours.GREY;
        int numberOfCPoints = _cameraPath.fovList.realNumberOfPoints;
        for (int i = 0; i < numberOfCPoints; i++)
        {
            CameraPathPoint point = _cameraPath.fovList[i];
            if (point.positionModes == CameraPathPoint.PositionModes.Free)
                Handles.DotCap(0, point.worldPosition, Quaternion.identity, 0.2f);
        }
        numberOfCPoints = _cameraPath.delayList.realNumberOfPoints;
        for (int i = 0; i < numberOfCPoints; i++)
        {
            CameraPathPoint point = _cameraPath.delayList[i];
            if (point.positionModes == CameraPathPoint.PositionModes.Free)
                Handles.DotCap(0, point.worldPosition, Quaternion.identity, 0.2f);
        }
        numberOfCPoints = _cameraPath.orientationList.realNumberOfPoints;
        for (int i = 0; i < numberOfCPoints; i++)
        {
            CameraPathPoint point = _cameraPath.orientationList[i];
            if (point.positionModes == CameraPathPoint.PositionModes.Free)
                Handles.DotCap(0, point.worldPosition, Quaternion.identity, 0.2f);
        }
        numberOfCPoints = _cameraPath.speedList.realNumberOfPoints;
        for (int i = 0; i < numberOfCPoints; i++)
        {
            CameraPathPoint point = _cameraPath.speedList[i];
            if (point.positionModes == CameraPathPoint.PositionModes.Free)
                Handles.DotCap(0, point.worldPosition, Quaternion.identity, 0.2f);
        }
        numberOfCPoints = _cameraPath.tiltList.realNumberOfPoints;
        for (int i = 0; i < numberOfCPoints; i++)
        {
            CameraPathPoint point = _cameraPath.tiltList[i];
            if (point.positionModes == CameraPathPoint.PositionModes.Free)
                Handles.DotCap(0, point.worldPosition, Quaternion.identity, 0.2f);
        }

        //draw track outline
        Camera sceneCamera = Camera.current;
        int numberOfPoints = _cameraPath.numberOfPoints;
        Handles.color = _cameraPath.selectedPathColour;
        for(int i = 0; i < numberOfPoints-1; i++)
        {
            CameraPathControlPoint pointA = _cameraPath.GetPoint(i);
            CameraPathControlPoint pointB = _cameraPath.GetPoint(i+1);

            float dotPA = Vector3.Dot(sceneCamera.transform.forward, pointA.worldPosition - sceneCamera.transform.position);
            float dotPB = Vector3.Dot(sceneCamera.transform.forward, pointB.worldPosition - sceneCamera.transform.position);

            if (dotPA < 0 && dotPB < 0)//points are both behind camera - don't render
                continue;

            float pointAPercentage = pointA.percentage;
            float pointBPercentage = pointB.percentage;
            float arcPercentage = pointBPercentage - pointAPercentage;
            Vector3 arcCentre = (pointA.worldPosition + pointB.worldPosition) * 0.5f;
            float arcLength = _cameraPath.StoredArcLength(_cameraPath.GetCurveIndex(pointA.index));
            float arcDistance = Vector3.Distance(sceneCamera.transform.position, arcCentre);
            int arcPoints = Mathf.RoundToInt(arcLength * (40 / Mathf.Max(arcDistance,20)));
            float arcTime = 1.0f / arcPoints;

            float endLoop = 1.0f - arcTime;
            Vector3 lastPoint = Vector3.zero;
            for (float p = 0; p < endLoop; p += arcTime)
            {
                float p2 = p + arcTime;
                float pathPercentageA = pointAPercentage + arcPercentage * p;
                float pathPercentageB = pointAPercentage + arcPercentage * p2;
                Vector3 lineStart = _cameraPath.GetPathPosition(pathPercentageA, true);
                Vector3 lineEnd = _cameraPath.GetPathPosition(pathPercentageB, true);

                Handles.DrawLine(lineStart + cpPosition, lineEnd + cpPosition);

                lastPoint = lineEnd;
            }
            Handles.DrawLine(lastPoint + cpPosition, _cameraPath.GetPathPosition(pointB.percentage, true) + cpPosition);
        }

        switch(pointMode)
        {
            case CameraPathEditor.PointModes.Transform:
                SceneGUIPointBased();
                break;

            case CameraPathEditor.PointModes.ControlPoints:
                    SceneGUIPointBased();
                break;

            case CameraPathEditor.PointModes.Orientations:
                SceneGUIOrientationBased();
                break;

            case CameraPathEditor.PointModes.FOV:
                SceneGUIFOVBased();
                break;

            case CameraPathEditor.PointModes.Events:
                SceneGUIEventBased();
                break;

            case CameraPathEditor.PointModes.Speed:
                SceneGUISpeedBased();
                break;

            case CameraPathEditor.PointModes.Tilt:
                SceneGUITiltBased();
                break;

            case CameraPathEditor.PointModes.Delay:
                SceneGUIDelayBased();
                break;

            case CameraPathEditor.PointModes.Ease:
                SceneGUIEaseBased();
                break;

            case CameraPathEditor.PointModes.AddPathPoints:
                AddPathPoints();
                break;

                case CameraPathEditor.PointModes.RemovePathPoints:
                RemovePathPoints();
                break;

                case CameraPathEditor.PointModes.AddOrientations:
                AddCPathPoints();
                break;

                case CameraPathEditor.PointModes.AddFovs:
                AddCPathPoints();
                break;

                case CameraPathEditor.PointModes.AddTilts:
                AddCPathPoints();
                break;

                case CameraPathEditor.PointModes.AddEvents:
                AddCPathPoints();
                break;

                case CameraPathEditor.PointModes.AddSpeeds:
                AddCPathPoints();
                break;

                case CameraPathEditor.PointModes.AddDelays:
                AddCPathPoints();
                break;

                case CameraPathEditor.PointModes.RemoveOrientations:
                RemoveCPathPoints();
                break;

                case CameraPathEditor.PointModes.RemoveTilts:
                RemoveCPathPoints();
                break;

                case CameraPathEditor.PointModes.RemoveFovs:
                RemoveCPathPoints();
                break;

                case CameraPathEditor.PointModes.RemoveEvents:
                RemoveCPathPoints();
                break;

                case CameraPathEditor.PointModes.RemoveSpeeds:
                RemoveCPathPoints();
                break;

                case CameraPathEditor.PointModes.RemoveDelays:
                RemoveCPathPoints();
                break;

        }

        if (Event.current.type == EventType.ValidateCommand)
        {
            switch (Event.current.commandName)
            {
                case "UndoRedoPerformed":
                    GUI.changed = true;
                    break;
            }
        }
    }

    private static void SceneGUIPointBased()
    {
        Camera sceneCamera = Camera.current;
        int realNumberOfPoints = _cameraPath.realNumberOfPoints;
        for (int i = 0; i < realNumberOfPoints; i++)
        {
            CameraPathControlPoint point = _cameraPath[i];
            if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            Undo.RecordObject(point,"Modifying Path Point");
            Handles.Label(point.worldPosition, point.displayName);
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;
            if (Handles.Button(point.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize, Handles.DotCap))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
                //point.isDirty = true;
            }

            if(i == selectedPointIndex)
            {
                if(pointMode == CameraPathEditor.PointModes.Transform || _cameraPath.interpolation != CameraPath.Interpolation.Bezier)
                {
                    Vector3 currentPosition = point.worldPosition;
                    currentPosition = Handles.DoPositionHandle(currentPosition, Quaternion.identity);
                    point.worldPosition = currentPosition;

                    if(_cameraPath.interpolation == CameraPath.Interpolation.Bezier)
                    {
                        Handles.color = CameraPathColours.DARKGREY;
                        Handles.DrawLine(point.worldPosition, point.forwardControlPoint + cpPosition);
                        Handles.DotCap(0, point.forwardControlPoint + cpPosition, Quaternion.identity, pointHandleSize * 0.5f);
                        Handles.DrawLine(point.worldPosition, point.backwardControlPoint + cpPosition);
                        Handles.DotCap(0, point.backwardControlPoint + cpPosition, Quaternion.identity, pointHandleSize * 0.5f);
                    }
                }
                else
                {
                    //Backward ControlPoints point - render first so it's behind the forward
                    Handles.DrawLine(point.worldPosition, point.backwardControlPoint + cpPosition);
                    point.backwardControlPoint = Handles.DoPositionHandle(point.backwardControlPoint + cpPosition, Quaternion.identity) - cpPosition;
                    if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) > 0)
                        Handles.Label(point.backwardControlPoint, "point " + i + " reverse ControlPoints point");

                    //Forward ControlPoints point
                    if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) > 0)
                        Handles.Label(point.forwardControlPoint, "point " + i + " ControlPoints point");
                    Handles.color = _cameraPath.selectedPointColour;
                    Handles.DrawLine(point.worldPosition, point.forwardControlPoint + cpPosition);
                    point.forwardControlPoint = Handles.DoPositionHandle(point.forwardControlPoint + cpPosition, Quaternion.identity) - cpPosition;
                    
                }
            }
        }
    }

    private static void SceneGUIOrientationBased()
    {
        CameraPathOrientationList orientationList = _cameraPath.orientationList;
        Camera sceneCamera = Camera.current;
        int orientationCount = orientationList.realNumberOfPoints;
        for (int i = 0; i < orientationCount; i++)
        {
            CameraPathOrientation orientation = orientationList[i];
            Undo.RecordObject(orientation, "Modifying Orientation Point");
            if (Vector3.Dot(sceneCamera.transform.forward, orientation.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string orientationLabel = orientation.displayName;
            orientationLabel += "\nat percentage: " + orientation.percent.ToString("F3");
            switch(orientation.positionModes)
            {
                case CameraPathPoint.PositionModes.FixedToPoint:
                    orientationLabel += "\nat point: " + orientation.point.name;
                    break;
            }

            Handles.Label(orientation.worldPosition, orientationLabel);
            float pointHandleSize = HandleUtility.GetHandleSize(orientation.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;
            Handles.ArrowCap(0, orientation.worldPosition, orientation.rotation, pointHandleSize*4);
            if (Handles.Button(orientation.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize, Handles.DotCap))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }

            if (i == selectedPointIndex)
            {
                Quaternion currentRotation = orientation.rotation;
                currentRotation = Handles.DoRotationHandle(currentRotation, orientation.worldPosition);
                if (currentRotation != orientation.rotation)
                {
                    orientation.rotation = currentRotation;
                }
            }
        }

        if(_cameraPath.showOrientationIndicators)
        {
            Handles.color = _cameraPath.orientationIndicatorColours;
            float indicatorLength = _cameraPath.orientationIndicatorUnitLength / _cameraPath.pathLength;
            for(float i = 0; i < 1; i += indicatorLength)
            {
                Vector3 indicatorPosition = _cameraPath.GetPathPosition(i) + cpPosition;
                Quaternion inicatorRotation = _cameraPath.GetPathRotation(i);
                float indicatorHandleSize = HandleUtility.GetHandleSize(indicatorPosition) * HANDLE_SCALE * 4;
                Handles.ArrowCap(0, indicatorPosition, inicatorRotation, indicatorHandleSize);
            }
        }
    }

    private static void SceneGUIFOVBased()
    {
        CameraPathFOVList fovList = _cameraPath.fovList;
        Camera sceneCamera = Camera.current;
        int pointCount = fovList.realNumberOfPoints;
        for (int i = 0; i < pointCount; i++)
        {
            CameraPathFOV fovPoint = fovList[i];
            Undo.RecordObject(fovPoint, "Modifying FOV Point");
            if (Vector3.Dot(sceneCamera.transform.forward, fovPoint.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string pointLabel = fovPoint.displayName;
            pointLabel += "\nvalue: " + fovPoint.FOV.ToString("F1");
            if (fovPoint.point == null) pointLabel += "\nat percentage: " + fovPoint.percent.ToString("F3");
            if (fovPoint.point != null) pointLabel += "\nat point: " + fovPoint.point.name;

            Handles.Label(fovPoint.worldPosition, pointLabel);
            float pointHandleSize = HandleUtility.GetHandleSize(fovPoint.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;
            if (Handles.Button(fovPoint.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize, Handles.DotCap))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }
        }
    }

    private static void SceneGUIEventBased()
    {
        CameraPathEventList eventList = _cameraPath.eventList;
        Camera sceneCamera = Camera.current;
        int pointCount = eventList.realNumberOfPoints;
        for (int i = 0; i < pointCount; i++)
        {
            CameraPathEvent eventPoint = eventList[i];
            Undo.RecordObject(eventPoint,"Modifying Event Point");
            if (Vector3.Dot(sceneCamera.transform.forward, eventPoint.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string pointLabel = eventPoint.displayName;
            pointLabel += "\ntype: " + eventPoint.type;
            if (eventPoint.type == CameraPathEvent.Types.Broadcast) pointLabel += "\nevent name: " + eventPoint.eventName;
            if (eventPoint.type == CameraPathEvent.Types.Call)
            {
                if (eventPoint.target != null)
                    pointLabel += "\nevent target: " + eventPoint.target.name + " calling: " + eventPoint.methodName;
                else
                    pointLabel += "\nno target assigned";
            }
            if (eventPoint.point == null) pointLabel += "\nat percentage: " + eventPoint.percent.ToString("F3");
            if (eventPoint.point != null) pointLabel += "\nat point: " + eventPoint.point.name;

            Handles.Label(eventPoint.worldPosition, pointLabel);
            float pointHandleSize = HandleUtility.GetHandleSize(eventPoint.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;
            if (Handles.Button(eventPoint.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize, Handles.DotCap))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }
        }
    }

    private static void SceneGUISpeedBased()
    {
        CameraPathSpeedList pointList = _cameraPath.speedList;
        Camera sceneCamera = Camera.current;
        int pointCount = pointList.realNumberOfPoints;
        for (int i = 0; i < pointCount; i++)
        {
            CameraPathSpeed point = pointList[i];
            Undo.RecordObject(point, "Modifying Speed Point");
            if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string pointLabel = point.displayName;
            pointLabel += "\nvalue: " + point.speed + " m/s";

            Handles.Label(point.worldPosition, pointLabel);
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;
            if (Handles.Button(point.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize, Handles.DotCap))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }
        }

    }

    private static void SceneGUITiltBased()
    {
        CameraPathTiltList pointList = _cameraPath.tiltList;
        Camera sceneCamera = Camera.current;
        int pointCount = pointList.realNumberOfPoints;
        for (int i = 0; i < pointCount; i++)
        {
            CameraPathTilt point = pointList[i];
            Undo.RecordObject(point, "Modifying Tilt Point");
            if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string pointLabel = point.displayName;
            pointLabel += "\nvalue: " + point.tilt.ToString("F1") + "\u00B0";

            Handles.Label(point.worldPosition, pointLabel);
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            bool pointIsSelected = i == selectedPointIndex;
            Handles.color = (pointIsSelected) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;

            float tiltSize = 2.0f;
            Vector3 pointForwardDirection = _cameraPath.GetPathDirection(point.percent, false);
            Quaternion qTilt = Quaternion.AngleAxis(-point.tilt, pointForwardDirection);
            Quaternion pointForward = Quaternion.LookRotation(pointForwardDirection);
            Handles.CircleCap(0, point.worldPosition, pointForward, tiltSize);
            Vector3 horizontalLineDirection = ((qTilt * Quaternion.AngleAxis(-90, Vector3.up)) * pointForwardDirection).normalized * tiltSize;
            Vector3 horizontalLineStart = point.worldPosition + horizontalLineDirection;
            Vector3 horizontalLineEnd = point.worldPosition - horizontalLineDirection;
            Handles.DrawLine(horizontalLineStart, horizontalLineEnd);

            Vector3 verticalLineDirection = (Quaternion.AngleAxis(-90, pointForwardDirection) * horizontalLineDirection).normalized * tiltSize;
            Vector3 verticalLineStart = point.worldPosition + verticalLineDirection;
            Vector3 verticalLineEnd = point.worldPosition;
            Handles.DrawLine(verticalLineStart, verticalLineEnd);

            if (Handles.Button(point.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize, Handles.DotCap))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }
        }

    }

    private static void SceneGUIDelayBased()
    {
        CameraPathDelayList pointList = _cameraPath.delayList;
        Camera sceneCamera = Camera.current;
        int pointCount = pointList.realNumberOfPoints;
        for (int i = 0; i < pointCount; i++)
        {
            CameraPathDelay point = pointList[i];
            Undo.RecordObject(point, "Modifying Delay Point");

            if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string pointLabel = "";
            if (point == pointList.introPoint)
                pointLabel += "start point";
            else if (point == pointList.outroPoint)
                pointLabel += "end point";
            else
            {
                pointLabel += point.displayName;

                if (point.time > 0)
                    pointLabel += "\ndelay: " + point.time.ToString("F2") + " sec";
                else
                    pointLabel += "\ndelay indefinitely";
            }

            Handles.Label(point.worldPosition, pointLabel);
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;
            if (Handles.Button(point.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize, Handles.DotCap))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }
        }
    }

    private static void SceneGUIEaseBased()
    {
        CameraPathDelayList pointList = _cameraPath.delayList;
        Camera sceneCamera = Camera.current;
        int pointCount = pointList.realNumberOfPoints;
        for (int i = 0; i < pointCount; i++)
        {
            CameraPathDelay point = pointList[i];
            Undo.RecordObject(point, "Modifying Ease Curves");

            if (Vector3.Dot(sceneCamera.transform.forward, point.worldPosition - sceneCamera.transform.position) < 0)
                continue;

            string pointLabel = "";
            if (point == pointList.introPoint)
                pointLabel += "start point";
            else if (point == pointList.outroPoint)
                pointLabel += "end point";
            else
            {
                pointLabel += point.displayName;

                if (point.time > 0)
                    pointLabel += "\ndelay: " + point.time.ToString("F2") + " sec";
                else
                    pointLabel += "\ndelay indefinitely";
            }

            Handles.Label(point.worldPosition, pointLabel);
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            Handles.color = (i == selectedPointIndex) ? _cameraPath.selectedPointColour : _cameraPath.unselectedPointColour;
            if (Handles.Button(point.worldPosition, Quaternion.identity, pointHandleSize, pointHandleSize, Handles.DotCap))
            {
                ChangeSelectedPointIndex(i);
                GUI.changed = true;
            }

            float unitPercent = 1.0f / _cameraPath.pathLength;
            if (point != pointList.outroPoint)
            {
                float outroEasePointPercent = _cameraPath.GetOutroEasePercentage(point);
                Vector3 outroEasePoint = _cameraPath.GetPathPosition(outroEasePointPercent, true);
                Vector3 outroeaseDirection = _cameraPath.GetPathDirection(outroEasePointPercent, false);

                Handles.color = CameraPathColours.RED;
                Handles.Label(outroEasePoint + cpPosition, "Ease Out\n" + point.displayName);
                Vector3 newPosition = Handles.Slider(outroEasePoint + cpPosition, outroeaseDirection) - cpPosition;

                float movement = Vector3.Distance(outroEasePoint - cpPosition, newPosition);
                if (movement > Mathf.Epsilon)
                {
                    float newPercent = _cameraPath.GetNearestPoint(newPosition);
                    float curvePercent = _cameraPath.GetCurvePercentage(_cameraPath.delayList.GetPoint(point.index), _cameraPath.delayList.GetPoint(point.index + 1), newPercent);
                    point.outroEndEasePercentage = curvePercent;
                }

                float percentWidth = (outroEasePointPercent - point.percent);
                float easeSpace = _cameraPath.pathLength * percentWidth;
                float easrLength = unitPercent / percentWidth;
                float percentMovement = easrLength / easeSpace;
                for (float e = point.percent; e < outroEasePointPercent; e += percentMovement)
                {
                    float eB = e + percentMovement;
                    Vector3 lineStart = _cameraPath.GetPathPosition(e, true) + cpPosition;
                    Vector3 lineEnd = _cameraPath.GetPathPosition(eB, true) + cpPosition;
                    Handles.DrawLine(lineStart,lineEnd);
                    float animCurvePercentA = (e - point.percent) / percentWidth;
                    float animCurvePercentB = (eB - point.percent) / percentWidth;
                    Vector3 lineEaseUpA = Vector3.up * point.outroCurve.Evaluate(animCurvePercentA);
                    Vector3 lineEaseUpB = Vector3.up * point.outroCurve.Evaluate(animCurvePercentB);
                    Handles.DrawLine(lineStart + lineEaseUpA, lineEnd + lineEaseUpB);
                }
            }

            if (point != pointList.introPoint)
            {
                float introEasePointPercent = _cameraPath.GetIntroEasePercentage(point);
                Vector3 introEasePoint = _cameraPath.GetPathPosition(introEasePointPercent, true);
                Vector3 introEaseDirection = _cameraPath.GetPathDirection(introEasePointPercent, false);

                Handles.color = CameraPathColours.RED;
                Handles.Label(introEasePoint + cpPosition, "Ease In\n" + point.displayName);
                Vector3 newPosition = Handles.Slider(introEasePoint + cpPosition, -introEaseDirection) - cpPosition;

                float movement = Vector3.Distance(introEasePoint - cpPosition, newPosition);
                if (movement > Mathf.Epsilon)
                {
                    float newPercent = _cameraPath.GetNearestPoint(newPosition);
                    float curvePercent = 1-_cameraPath.GetCurvePercentage(_cameraPath.delayList.GetPoint(point.index-1), _cameraPath.delayList.GetPoint(point.index), newPercent);
                    point.introStartEasePercentage = curvePercent;
                }

                float percentWidth = (point.percent - introEasePointPercent);
                float easeSpace = _cameraPath.pathLength * percentWidth;
                float easrLength = unitPercent / percentWidth;
                float percentMovement = easrLength / easeSpace;
                for (float e = introEasePointPercent; e < point.percent; e += percentMovement)
                {
                    float eB = e + percentMovement;
                    Vector3 lineStart = _cameraPath.GetPathPosition(e, true) + cpPosition;
                    Vector3 lineEnd = _cameraPath.GetPathPosition(eB, true) + cpPosition;
                    Handles.DrawLine(lineStart, lineEnd);
                    float animCurvePercentA = (e - introEasePointPercent) / percentWidth;
                    float animCurvePercentB = (eB - introEasePointPercent) / percentWidth;
                    Vector3 lineEaseUpA = Vector3.up * point.introCurve.Evaluate(animCurvePercentA);
                    Vector3 lineEaseUpB = Vector3.up * point.introCurve.Evaluate(animCurvePercentB);
                    Handles.DrawLine(lineStart + lineEaseUpA, lineEnd + lineEaseUpB);
                }
            }
        }
    }

    private static void AddPathPoints()
    {
        if (SceneView.focusedWindow != null)
            SceneView.focusedWindow.wantsMouseMove = true;

        Handles.color = _cameraPath.unselectedPointColour;
        int numberOfPoints = _cameraPath.realNumberOfPoints;
        for (int i = 0; i < numberOfPoints; i++)
        {
            CameraPathControlPoint point = _cameraPath[i];
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE * 0.4f;
            Handles.DotCap(0, point.worldPosition, Quaternion.identity, pointHandleSize);
        }

        float mousePercentage = NearestmMousePercentage();// _track.GetNearestPoint(mousePlanePoint);
        Vector3 mouseTrackPoint = _cameraPath.GetPathPosition(mousePercentage, true) + cpPosition;
        Handles.Label(mouseTrackPoint, "Add New Path Point");
        float newPointHandleSize = HandleUtility.GetHandleSize(mouseTrackPoint) * HANDLE_SCALE;
        Quaternion lookDirection = Quaternion.LookRotation(Camera.current.transform.forward);
        if (Handles.Button(mouseTrackPoint, lookDirection, newPointHandleSize, newPointHandleSize, Handles.DotCap))
        {
            int newPointIndex = _cameraPath.GetNextPointIndex(mousePercentage,false);
            CameraPathControlPoint newPoint = ScriptableObject.CreateInstance<CameraPathControlPoint>();
            newPoint.baseTransform = _cameraPath.transform;
            newPoint.worldPosition = mouseTrackPoint;
            _cameraPath.InsertPoint(newPoint, newPointIndex);
            ChangeSelectedPointIndex(newPointIndex);
            GUI.changed = true;
        }
    }

    private static void RemovePathPoints()
    {
        if (SceneView.focusedWindow != null)
            SceneView.focusedWindow.wantsMouseMove = true;

        int numberOfPoints = _cameraPath.realNumberOfPoints;
        Handles.color = _cameraPath.selectedPointColour;
        Ray mouseRay = Camera.current.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 30, 0));
        Quaternion mouseLookDirection = Quaternion.LookRotation(-mouseRay.direction);
        for (int i = 0; i < numberOfPoints; i++)
        {
            CameraPathControlPoint point = _cameraPath[i];
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            Handles.Label(point.worldPosition, "Remove Point: "+point.displayName);
            if (Handles.Button(point.worldPosition, mouseLookDirection, pointHandleSize, pointHandleSize, Handles.DotCap))
            {
                _cameraPath.RemovePoint(point);
                GUI.changed = true;
                return;
            }
        }
    }

    private static void AddCPathPoints()
    {
        if (SceneView.focusedWindow != null)
            SceneView.focusedWindow.wantsMouseMove = true;

        Handles.color = _cameraPath.selectedPointColour;
        CameraPathPointList pointList = null;
        switch(pointMode)
        {
            case CameraPathEditor.PointModes.AddOrientations:
                pointList = _cameraPath.orientationList;
                break;
            case CameraPathEditor.PointModes.AddFovs:
                pointList = _cameraPath.fovList;
                break;
            case CameraPathEditor.PointModes.AddTilts:
                pointList = _cameraPath.tiltList;
                break;
            case CameraPathEditor.PointModes.AddEvents:
                pointList = _cameraPath.eventList;
                break;
            case CameraPathEditor.PointModes.AddSpeeds:
                pointList = _cameraPath.speedList;
                break;
            case CameraPathEditor.PointModes.AddDelays:
                pointList = _cameraPath.delayList;
                break;
        }
        int numberOfPoints = pointList.realNumberOfPoints;
        for (int i = 0; i < numberOfPoints; i++)
        {
            CameraPathPoint point = pointList[i];
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE * 0.4f;
            Handles.DotCap(0, point.worldPosition, Quaternion.identity, pointHandleSize);
        }

        float mousePercentage = NearestmMousePercentage();// _track.GetNearestPoint(mousePlanePoint);
        Vector3 mouseTrackPoint = _cameraPath.GetPathPosition(mousePercentage, true) + cpPosition;
        Handles.Label(mouseTrackPoint, "Add New Point");
        float newPointHandleSize = HandleUtility.GetHandleSize(mouseTrackPoint) * HANDLE_SCALE;
        Ray mouseRay = Camera.current.ScreenPointToRay(new Vector3(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 30, 0));
        Quaternion mouseLookDirection = Quaternion.LookRotation(-mouseRay.direction);
        if (Handles.Button(mouseTrackPoint, mouseLookDirection, newPointHandleSize, newPointHandleSize, Handles.DotCap))
        {
            CameraPathControlPoint curvePointA = _cameraPath[_cameraPath.GetLastPointIndex(mousePercentage,false)];
            CameraPathControlPoint curvePointB = _cameraPath[_cameraPath.GetNextPointIndex(mousePercentage,false)];
            float curvePercentage = _cameraPath.GetCurvePercentage(curvePointA, curvePointB, mousePercentage);
            switch(pointMode)
            {
                case CameraPathEditor.PointModes.AddOrientations:
                    Quaternion pointRotation = Quaternion.LookRotation(_cameraPath.GetPathDirection(mousePercentage));
                    CameraPathOrientation newOrientation = ((CameraPathOrientationList)pointList).AddOrientation(curvePointA, curvePointB, curvePercentage, pointRotation);
                    ChangeSelectedPointIndex(pointList.IndexOf(newOrientation));
                    break;

                case CameraPathEditor.PointModes.AddFovs:
                    float pointFOV = _cameraPath.fovList.GetFOV(mousePercentage);
                    CameraPathFOV newFOVPoint = ((CameraPathFOVList)pointList).AddFOV(curvePointA, curvePointB, curvePercentage, pointFOV);
                    ChangeSelectedPointIndex(pointList.IndexOf(newFOVPoint));
                    break;

                case CameraPathEditor.PointModes.AddTilts:
                    float pointTilt = _cameraPath.GetPathTilt(mousePercentage);
                    CameraPathTilt newTiltPoint = ((CameraPathTiltList)pointList).AddTilt(curvePointA, curvePointB, curvePercentage, pointTilt);
                    ChangeSelectedPointIndex(pointList.IndexOf(newTiltPoint));
                    break;

                case CameraPathEditor.PointModes.AddEvents:
                    CameraPathEvent newEventPoint = ((CameraPathEventList)pointList).AddEvent(curvePointA, curvePointB, curvePercentage);
                    ChangeSelectedPointIndex(pointList.IndexOf(newEventPoint));
                    break;

                case CameraPathEditor.PointModes.AddSpeeds:
                    _cameraPath.speedList.enabled = true;//if we're adding speeds then we probable want to enable it
                    CameraPathSpeed newSpeedPoint = ((CameraPathSpeedList)pointList).AddSpeedPoint(curvePointA, curvePointB, curvePercentage);
                    newSpeedPoint.speed = _animator.pathSpeed;
                    ChangeSelectedPointIndex(pointList.IndexOf(newSpeedPoint));
                    break;

                case CameraPathEditor.PointModes.AddDelays:
                    CameraPathDelay newDelayPoint = ((CameraPathDelayList)pointList).AddDelayPoint(curvePointA, curvePointB, curvePercentage);
                    ChangeSelectedPointIndex(pointList.IndexOf(newDelayPoint));
                    break;
            }
            GUI.changed = true;
        }
    }


    private static void RemoveCPathPoints()
    {
        if (SceneView.focusedWindow != null)
            SceneView.focusedWindow.wantsMouseMove = true;

        CameraPathPointList pointList = null;
        switch (pointMode)
        {
            case CameraPathEditor.PointModes.RemoveOrientations:
                pointList = _cameraPath.orientationList;
                break;
            case CameraPathEditor.PointModes.RemoveFovs:
                pointList = _cameraPath.fovList;
                break;
            case CameraPathEditor.PointModes.RemoveTilts:
                pointList = _cameraPath.tiltList;
                break;
            case CameraPathEditor.PointModes.RemoveEvents:
                pointList = _cameraPath.eventList;
                break;
            case CameraPathEditor.PointModes.RemoveSpeeds:
                pointList = _cameraPath.speedList;
                break;
            case CameraPathEditor.PointModes.RemoveDelays:
                pointList = _cameraPath.delayList;
                break;
        }

        int numberOfPoints = pointList.realNumberOfPoints;
        Handles.color = _cameraPath.selectedPointColour;
        Quaternion mouseLookDirection = Quaternion.LookRotation(Camera.current.transform.forward);
        for (int i = 0; i < numberOfPoints; i++)
        {
            CameraPathPoint point = pointList[i];
            float pointHandleSize = HandleUtility.GetHandleSize(point.worldPosition) * HANDLE_SCALE;
            Handles.Label(point.worldPosition, "Remove Point " + i);
            if (Handles.Button(point.worldPosition, mouseLookDirection, pointHandleSize, pointHandleSize, Handles.DotCap))
            {
                pointList.RemovePoint(point);
                GUI.changed = true;
                return;
            }
        }
    }

    /// <summary>
    /// Get the nearest point on the track curve to the  mouse position
    /// We essentailly project the track onto a 2D plane that is the editor camera and then find a point on that
    /// </summary>
    /// <returns>A percentage of the nearest point on the track curve to the nerest metre</returns>
    private static float NearestmMousePercentage()
    {
        Camera cam = Camera.current;
        float screenHeight = cam.pixelHeight;
        Vector2 mousePos = Event.current.mousePosition;
        mousePos.y = screenHeight - mousePos.y;
        int numberOfSearchPoints = (int)_cameraPath.pathLength;

        Vector2 zeropoint = cam.WorldToScreenPoint(_cameraPath.GetPathPosition(0, true) + cpPosition);
        float nearestPointSqrMag = Vector2.SqrMagnitude(zeropoint - mousePos);
        float nearestT = 0;
        float nearestPointSqrMagB = Vector2.SqrMagnitude(zeropoint - mousePos);
        float nearestTb = 0;

        for (int i = 1; i < numberOfSearchPoints; i++)
        {
            float t = i / (float)numberOfSearchPoints;
            Vector2 point = cam.WorldToScreenPoint(_cameraPath.GetPathPosition(t, true) + cpPosition);
            float thisPointMag = Vector2.SqrMagnitude(point - mousePos);
            if (thisPointMag < nearestPointSqrMag)
            {
                nearestPointSqrMagB = nearestPointSqrMag;
                nearestTb = nearestT;

                nearestT = t;
                nearestPointSqrMag = thisPointMag;
            }
        }
        float lerpvalue = nearestPointSqrMag / (nearestPointSqrMag + nearestPointSqrMagB);
        return Mathf.Lerp(nearestT, nearestTb, lerpvalue);
    }


    private static void ChangeSelectedPointIndex(int newPointSelected)
    {
        selectedPointIndex = newPointSelected;
        NewPointSelected(newPointSelected);
    }
}
