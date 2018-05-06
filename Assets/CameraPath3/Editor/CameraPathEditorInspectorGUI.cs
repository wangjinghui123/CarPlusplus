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

public class CameraPathEditorInspectorGUI : MonoBehaviour 
{
    private static GUIContent[] _toolBarGUIContentA;
    private static GUIContent[] _toolBarGUIContentB;

    public static CameraPathEditor.PointModes pointMode = CameraPathEditor.PointModes.Transform;
    private static CameraPathAnimator.orientationModes _orientationmode = CameraPathAnimator.orientationModes.custom;

    public static CameraPath _cameraPath;
    public static CameraPathAnimator _animator;
    public static int selectedPointIndex = 0;//selected path point
    private static Vector3 cpPosition;

    //Preview Camera
    private static float aspect = 1.7777f;
    private static int previewResolution = 800;

    //GUI Styles
    private static GUIStyle unselectedBox;
    private static GUIStyle selectedBox;
    private static GUIStyle redText;

    private static Texture2D unselectedBoxColour;
    private static Texture2D selectedBoxColour;


    public delegate void NewPointModeHandler(CameraPathEditor.PointModes newPointMode);
    public delegate void NewPointSelectedHandler(int selectedPointIndex);
    public static event NewPointModeHandler NewPointMode;
    public static event NewPointSelectedHandler NewPointSelected;

    public static void Setup()
    {
        if(_cameraPath == null)
            return;

        SetupToolbar();

        unselectedBox = new GUIStyle();
        unselectedBoxColour = new Texture2D(1, 1);
        unselectedBoxColour.SetPixel(0, 0, CameraPathColours.DARKGREY);
        unselectedBoxColour.Apply();
        unselectedBox.normal.background = unselectedBoxColour;

        selectedBox = new GUIStyle();
        selectedBoxColour = new Texture2D(1, 1);
        selectedBoxColour.SetPixel(0, 0, CameraPathColours.DARKGREEN);
        selectedBoxColour.Apply();
        selectedBox.normal.background = selectedBoxColour;

        redText = new GUIStyle();
        redText.normal.textColor = CameraPathColours.RED;

        //Preview Camera
        if (_cameraPath.editorPreview != null)
            DestroyImmediate(_cameraPath.editorPreview);
        if (SystemInfo.supportsRenderTextures)
        {
            _cameraPath.editorPreview = new GameObject("Path Point Preview Cam");
            _cameraPath.editorPreview.hideFlags = HideFlags.HideAndDontSave;
            _cameraPath.editorPreview.AddComponent<Camera>();
            _cameraPath.editorPreview.GetComponent<Camera>().fieldOfView = 60;
            _cameraPath.editorPreview.GetComponent<Camera>().depth = -1;
            //Retreive camera settings from the main camera
            Camera[] cams = Camera.allCameras;
            bool sceneHasCamera = cams.Length > 0;
            Camera sceneCamera = null;
            Skybox sceneCameraSkybox = null;
            if (Camera.main)
            {
                sceneCamera = Camera.main;
            }
            else if (sceneHasCamera)
            {
                sceneCamera = cams[0];
            }

            if (sceneCamera != null)
                sceneCameraSkybox = sceneCamera.GetComponent<Skybox>();
            if (sceneCamera != null)
            {
                _cameraPath.editorPreview.GetComponent<Camera>().backgroundColor = sceneCamera.backgroundColor;
                if (sceneCameraSkybox != null)
                    _cameraPath.editorPreview.AddComponent<Skybox>().material = sceneCameraSkybox.material;
                else if (RenderSettings.skybox != null)
                    _cameraPath.editorPreview.AddComponent<Skybox>().material = RenderSettings.skybox;
            }
            _cameraPath.editorPreview.GetComponent<Camera>().enabled = false;
        }

        if (EditorApplication.isPlaying && _cameraPath.editorPreview != null)
            _cameraPath.editorPreview.SetActive(false);
    }

    public static void OnInspectorGUI()
    {

        cpPosition = _cameraPath.transform.position;

        if(_cameraPath.transform.rotation != Quaternion.identity)
        {
            EditorGUILayout.HelpBox("Camera Path does not support rotations of the main game object.", MessageType.Error);
            if (GUILayout.Button("Reset Rotation"))
                _cameraPath.transform.rotation = Quaternion.identity;
            return;
        }

        GUILayout.BeginVertical(GUILayout.Width(400));

        if (_cameraPath.realNumberOfPoints < 2)
        {
            EditorGUILayout.HelpBox("There are no track points defined, add a path point to begin", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("Path Length approx. " + (_cameraPath.pathLength).ToString("F2") + " units");
        bool trackloop = EditorGUILayout.Toggle("Is Looped", _cameraPath.loop);
        _cameraPath.loop = trackloop;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Link Camera Path", GUILayout.Width(110));
        CameraPath nextPath = (CameraPath)EditorGUILayout.ObjectField(_cameraPath.nextPath, typeof(CameraPath), true);
        _cameraPath.nextPath = nextPath;
        EditorGUI.BeginDisabledGroup(nextPath == null);
        EditorGUILayout.LabelField("Interpolate", GUILayout.Width(70));
        bool interpolateNextPath = EditorGUILayout.Toggle(_cameraPath.interpolateNextPath, GUILayout.Width(30));
        _cameraPath.interpolateNextPath = interpolateNextPath;
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (_orientationmode != _animator.orientationMode)
            SetupToolbar();
        ToolbarMenuGUI();

        switch (pointMode)
        {
            case CameraPathEditor.PointModes.Transform:
                ModifyPointsInspectorGUI();
                break;

            case CameraPathEditor.PointModes.ControlPoints:
                ModifyControlPointsInspector();
                break;

            case CameraPathEditor.PointModes.FOV:
                ModifyFOVInspector();
                break;

            case CameraPathEditor.PointModes.Speed:
                ModifySpeedInspector();
                break;

            case CameraPathEditor.PointModes.Orientations:
                ModifyOrientaionInspector();
                break;

            case CameraPathEditor.PointModes.Tilt:
                ModifyTiltsInspector();
                break;

            case CameraPathEditor.PointModes.Events:
                ModifyEventsInspector();
                break;

            case CameraPathEditor.PointModes.Delay:
                ModifyDelayInspector();
                break;

            case CameraPathEditor.PointModes.Ease:
                ModifyEaseInspector();
                break;

            case CameraPathEditor.PointModes.AddPathPoints:
                ModifyPointsInspectorGUI();
                break;

            case CameraPathEditor.PointModes.RemovePathPoints:
                ModifyPointsInspectorGUI();
                break;

            case CameraPathEditor.PointModes.AddOrientations:
                ModifyOrientaionInspector();
                break;

            case CameraPathEditor.PointModes.AddTilts:
                ModifyTiltsInspector();
                break;

            case CameraPathEditor.PointModes.AddEvents:
                ModifyEventsInspector();
                break;

            case CameraPathEditor.PointModes.AddSpeeds:
                ModifySpeedInspector();
                break;

            case CameraPathEditor.PointModes.AddFovs:
                ModifyFOVInspector();
                break;

            case CameraPathEditor.PointModes.AddDelays:
                ModifyDelayInspector();
                break;

            case CameraPathEditor.PointModes.RemoveOrientations:
                ModifyOrientaionInspector();
                break;

            case CameraPathEditor.PointModes.RemoveTilts:
                ModifyTiltsInspector();
                break;

            case CameraPathEditor.PointModes.RemoveEvents:
                ModifyEventsInspector();
                break;

            case CameraPathEditor.PointModes.RemoveSpeeds:
                ModifySpeedInspector();
                break;

            case CameraPathEditor.PointModes.RemoveFovs:
                ModifyFOVInspector();
                break;

            case CameraPathEditor.PointModes.RemoveDelays:
                ModifyDelayInspector();
                break;

            case CameraPathEditor.PointModes.Options:
                OptionsInspectorGUI();
                break;

        }
        GUILayout.EndVertical();
    }

    private static void ModifyPointsInspectorGUI()
    {
        CameraPathControlPoint point = null;
        if (selectedPointIndex >= _cameraPath.realNumberOfPoints)
            ChangeSelectedPointIndex(_cameraPath.realNumberOfPoints - 1);
        if (_cameraPath.realNumberOfPoints > 0)
            point = _cameraPath[selectedPointIndex];

        Undo.RecordObject(point,"Modify Path Point");
        _cameraPath.editorPreview.transform.position = point.worldPosition;
        _cameraPath.editorPreview.transform.rotation = _animator.GetAnimatedOrientation(point.normalisedPercentage);
        _cameraPath.editorPreview.GetComponent<Camera>().fieldOfView = _cameraPath.GetPathFOV(point.percentage);
        RenderPreview();
        PointListGUI();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Selected point " + selectedPointIndex);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Custom Point Name");
        point.customName = EditorGUILayout.TextField(point.customName);
        if (GUILayout.Button("Clear"))
            point.customName = "";
        EditorGUILayout.EndHorizontal();

        Vector3 pointposition = EditorGUILayout.Vector3Field("Point Position", point.localPosition);
        if (pointposition != point.localPosition)
        {
            //                    Undo.RegisterUndo(point, "Modify Point Position");
            point.localPosition = pointposition;
        }

        //ADD NEW POINTS
        if (pointMode != CameraPathEditor.PointModes.AddPathPoints)
        {
            if(GUILayout.Button("Add Path Points"))
            {
                ChangePointMode(CameraPathEditor.PointModes.AddPathPoints);
                NewPointMode(pointMode);
            }
        }
        else
        {
            if(GUILayout.Button("Done"))
            {
                ChangePointMode(CameraPathEditor.PointModes.Transform);
                NewPointMode(pointMode);
            }
        }

        if (GUILayout.Button("Add Path Point to End of Path"))
            AddPointToEnd();

        if (pointMode != CameraPathEditor.PointModes.RemovePathPoints)
        {
            if(GUILayout.Button("Delete Path Points"))
            {
                ChangePointMode(CameraPathEditor.PointModes.RemovePathPoints);
                NewPointMode(pointMode);
            }
        }
        else
        {
            if(GUILayout.Button("Done"))
            {
                ChangePointMode(CameraPathEditor.PointModes.Transform);
                NewPointMode(pointMode);
            }
        }
    }

    private static void ModifyControlPointsInspector()
    {
        bool isBezier = _cameraPath.interpolation == CameraPath.Interpolation.Bezier;

        if(!isBezier)
        {
            EditorGUILayout.HelpBox("Path interpolation is currently not set to Bezier. There are no control points to manipulate", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Interpolation Algorithm");
            _cameraPath.interpolation = (CameraPath.Interpolation)EditorGUILayout.EnumPopup(_cameraPath.interpolation);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUI.BeginDisabledGroup(!isBezier);
        CameraPathControlPoint point = null;
        if (selectedPointIndex >= _cameraPath.realNumberOfPoints)
            ChangeSelectedPointIndex(_cameraPath.realNumberOfPoints - 1);
        if (_cameraPath.realNumberOfPoints > 0)
            point = _cameraPath[selectedPointIndex];

        _cameraPath.editorPreview.transform.position = point.worldPosition;
        _cameraPath.editorPreview.transform.rotation = _animator.GetAnimatedOrientation(point.normalisedPercentage);
        _cameraPath.editorPreview.GetComponent<Camera>().fieldOfView = _cameraPath.GetPathFOV(point.percentage);
        RenderPreview();
        PointListGUI();

        bool pointsplitControlPoints = EditorGUILayout.Toggle("Split Control Points", point.splitControlPoints);
        if (pointsplitControlPoints != point.splitControlPoints)
        {
            //                    Undo.RegisterUndo(point, "Modify Split Control Points");
            point.splitControlPoints = pointsplitControlPoints;
        }
        Vector3 pointforwardControlPoint = EditorGUILayout.Vector3Field("Control Point Position", point.forwardControlPoint);
        if (pointforwardControlPoint != point.forwardControlPoint)
        {
            //                    Undo.RegisterUndo(point, "Modify Point Forward Control");
            point.forwardControlPoint = pointforwardControlPoint;
        }

        if (GUILayout.Button("Auto Place Control Points"))
        {
            float pointPercentage = point.percentage;
            Vector3 pathDirection = _cameraPath.GetPathDirection(pointPercentage);
            float forwardArcLength = _cameraPath.StoredArcLength(_cameraPath.GetCurveIndex(point.index));
            float backwardArcLength = _cameraPath.StoredArcLength(_cameraPath.GetCurveIndex(point.index - 1));
            point.forwardControlPointLocal = pathDirection * (forwardArcLength + backwardArcLength) * 0.1666f;
        }

        if (GUILayout.Button("Zero Control Points"))
        {
            point.forwardControlPointLocal = Vector3.zero;
            if(point.splitControlPoints)
                point.backwardControlPoint = Vector3.zero;
        }
        EditorGUI.EndDisabledGroup();
    }

    private static void ModifyOrientaionInspector()
    {
        CameraPathOrientationList pointList = _cameraPath.orientationList;
        CameraPathOrientation point = null;
        if (pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            _cameraPath.editorPreview.transform.position = point.worldPosition;
            _cameraPath.editorPreview.transform.rotation = _animator.GetAnimatedOrientation(point.animationPercentage);
            _cameraPath.editorPreview.GetComponent<Camera>().fieldOfView = _cameraPath.GetPathFOV(point.percent);
            RenderPreview();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Interpolation Algorithm");
        pointList.interpolation = (CameraPathOrientationList.Interpolation)EditorGUILayout.EnumPopup(pointList.interpolation);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Show Orientation Inidcators", GUILayout.Width(170));
        _cameraPath.showOrientationIndicators = EditorGUILayout.Toggle(_cameraPath.showOrientationIndicators);
        EditorGUILayout.LabelField("Every", GUILayout.Width(40));
        _cameraPath.orientationIndicatorUnitLength = EditorGUILayout.FloatField(_cameraPath.orientationIndicatorUnitLength, GUILayout.Width(30));
        EditorGUILayout.LabelField("units", GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();

        if (pointList.realNumberOfPoints == 0)
            EditorGUILayout.HelpBox("There are no orientation points in this path.", MessageType.Warning);

        CPPointArrayInspector("Orientation Points", pointList, CameraPathEditor.PointModes.Orientations, CameraPathEditor.PointModes.AddOrientations, CameraPathEditor.PointModes.RemoveOrientations);
        
        if (point != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Point Name",GUILayout.Width(120));
            point.customName = EditorGUILayout.TextField(point.customName);
            if (GUILayout.Button("Clear"))
                point.customName = "";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            Vector3 currentRotation = point.rotation.eulerAngles;
            EditorGUILayout.LabelField("Angle", GUILayout.Width(60));
            Vector3 newRotation = EditorGUILayout.Vector3Field("", currentRotation);
            if (currentRotation != newRotation)
            {
                point.rotation.eulerAngles = newRotation;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Look at Target",GUILayout.Width(100));
            point.lookAt = (Transform)EditorGUILayout.ObjectField(point.lookAt, typeof(Transform), true);
            if(GUILayout.Button("Clear",GUILayout.Width(50)))
                point.lookAt = null;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            if(GUILayout.Button("Reset Angle"))
                point.rotation = Quaternion.identity;
            
            if(GUILayout.Button("Set to Path Direction"))
                point.rotation.SetLookRotation(_cameraPath.GetPathDirection(point.percent,false));
        }
    }

    private static void ModifyFOVInspector()
    {
        CameraPathFOVList pointList = _cameraPath.fovList;
        CameraPathFOV point = null;
        if (pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            _cameraPath.editorPreview.transform.position = point.worldPosition;
            _cameraPath.editorPreview.transform.rotation = _animator.GetAnimatedOrientation(point.animationPercentage);
            _cameraPath.editorPreview.GetComponent<Camera>().fieldOfView = _cameraPath.GetPathFOV(point.percent);
            RenderPreview();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Interpolation Algorithm");
        pointList.interpolation = (CameraPathFOVList.Interpolation)EditorGUILayout.EnumPopup(pointList.interpolation);
        EditorGUILayout.EndHorizontal();

        if (pointList.realNumberOfPoints == 0)
            EditorGUILayout.HelpBox("There are no FOV points in this path.", MessageType.Warning);

        CPPointArrayInspector("Field of View Points", pointList, CameraPathEditor.PointModes.FOV, CameraPathEditor.PointModes.AddFovs, CameraPathEditor.PointModes.RemoveFovs);

        if (point != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Point Name");
            point.customName = EditorGUILayout.TextField(point.customName);
            if (GUILayout.Button("Clear"))
                point.customName = "";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Field of View Value");
            EditorGUILayout.BeginHorizontal();
            float currentFOV = point.FOV;
            float newFOV = EditorGUILayout.Slider(currentFOV, 0, 180);
            EditorGUILayout.EndHorizontal();
            if (currentFOV != newFOV)
            {
                point.FOV = newFOV;
            }

            if(GUILayout.Button("Set to Camera Default"))
            {
                if(_animator.isCamera)
                    point.FOV = _animator.animationObject.GetComponent<Camera>().fieldOfView;
                else
                    point.FOV = Camera.main.fieldOfView;
            }
        }
    }

    private static void ModifyTiltsInspector()
    {
        CameraPathTiltList pointList = _cameraPath.tiltList;
        CameraPathTilt point = null;
        if (pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            _cameraPath.editorPreview.transform.position = point.worldPosition;
            _cameraPath.editorPreview.transform.rotation = _animator.GetAnimatedOrientation(point.animationPercentage);
            _cameraPath.editorPreview.GetComponent<Camera>().fieldOfView = _cameraPath.GetPathFOV(point.percent);
            RenderPreview();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Interpolation Algorithm");
        pointList.interpolation = (CameraPathTiltList.Interpolation)EditorGUILayout.EnumPopup(pointList.interpolation);
        EditorGUILayout.EndHorizontal();

        if (pointList.realNumberOfPoints == 0)
            EditorGUILayout.HelpBox("There are no tilt points in this path.", MessageType.Warning);

        CPPointArrayInspector("Tilt Points", pointList, CameraPathEditor.PointModes.Tilt, CameraPathEditor.PointModes.AddTilts, CameraPathEditor.PointModes.RemoveTilts);

        if (point != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Point Name");
            point.customName = EditorGUILayout.TextField(point.customName);
            if (GUILayout.Button("Clear"))
                point.customName = "";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Tilt Value");
            EditorGUILayout.BeginHorizontal();
            float currentTilt = point.tilt;
            float newTilt = EditorGUILayout.FloatField(currentTilt);
            EditorGUILayout.EndHorizontal();
            if (currentTilt != newTilt)
            {
                point.tilt = newTilt;
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Auto Set Tile Points");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sensitivity");
            _cameraPath.tiltList.autoSensitivity = EditorGUILayout.Slider(_cameraPath.tiltList.autoSensitivity, 0.0f, 1.0f);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Calculate and Assign Selected Path Tilts"))
            {
                _cameraPath.tiltList.AutoSetTilt(point);
            }

            if (GUILayout.Button("Calculate and Assign All Path Tilts"))
            {
                if (EditorUtility.DisplayDialog("Auto Setting All Path Tilt Values", "Are you sure you want to set all the values in this path?", "yes", "noooooo!"))
                    _cameraPath.tiltList.AutoSetTilts();
            }
            EditorGUILayout.EndVertical();
        }
    }

    private static void ModifySpeedInspector()
    {
        CameraPathSpeedList pointList = _cameraPath.speedList;
        CameraPathSpeed point = null;
        if (pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            _cameraPath.editorPreview.transform.position = point.worldPosition;
            _cameraPath.editorPreview.transform.rotation = _animator.GetAnimatedOrientation(point.animationPercentage);
            _cameraPath.editorPreview.GetComponent<Camera>().fieldOfView = _cameraPath.GetPathFOV(point.percent);
            RenderPreview();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Interpolation Algorithm");
        pointList.interpolation = (CameraPathSpeedList.Interpolation)EditorGUILayout.EnumPopup(pointList.interpolation);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(pointList.realNumberOfPoints == 0);
        EditorGUILayout.LabelField("Enabled");
        pointList.enabled = EditorGUILayout.Toggle(pointList.enabled);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        if(pointList.realNumberOfPoints==0)
            EditorGUILayout.HelpBox("There are no speed points in this path so it is disabled.", MessageType.Warning);
        EditorGUILayout.EndVertical();

        CPPointArrayInspector("Speed Points", pointList, CameraPathEditor.PointModes.Speed, CameraPathEditor.PointModes.AddSpeeds, CameraPathEditor.PointModes.RemoveSpeeds);

        if(point != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Point Name");
            point.customName = EditorGUILayout.TextField(point.customName);
            if(GUILayout.Button("Clear"))
                point.customName = "";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Speed Value");
            EditorGUILayout.BeginHorizontal();
            float currentSpeed = point.speed;
            float newSpeed = EditorGUILayout.FloatField(currentSpeed);
            EditorGUILayout.EndHorizontal();
            point.speed = newSpeed;
        }
    }

    private static void ModifyEventsInspector()
    {
        CameraPathEventList pointList = _cameraPath.eventList;
        CameraPathEvent point = null;
        if(pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            _cameraPath.editorPreview.transform.position = point.worldPosition;
            _cameraPath.editorPreview.transform.rotation = _animator.GetAnimatedOrientation(point.animationPercentage);
            _cameraPath.editorPreview.GetComponent<Camera>().fieldOfView = _cameraPath.GetPathFOV(point.percent);
            RenderPreview();
        }
        
        CPPointArrayInspector("Event Points", pointList, CameraPathEditor.PointModes.Events, CameraPathEditor.PointModes.AddEvents, CameraPathEditor.PointModes.RemoveEvents);

        if(point != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Point Name");
            point.customName = EditorGUILayout.TextField(point.customName);
            if (GUILayout.Button("Clear"))
                point.customName = "";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Event Type");
            point.type = (CameraPathEvent.Types)EditorGUILayout.EnumPopup(point.type);
            EditorGUILayout.EndHorizontal();

            switch(point.type)
            {
                case CameraPathEvent.Types.Broadcast:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Event Name");
                    point.eventName = EditorGUILayout.TextField(point.eventName);
                    EditorGUILayout.EndHorizontal();
                    break;

                case CameraPathEvent.Types.Call:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Event Target");
                    point.target = (GameObject)EditorGUILayout.ObjectField(point.target, typeof(GameObject), true);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Event Call");
                    point.methodName = EditorGUILayout.TextField(point.methodName);
                    EditorGUILayout.EndHorizontal();
                    break;
            }
        }
    }

    private static void ModifyDelayInspector()
    {
        CameraPathDelayList pointList = _cameraPath.delayList;
        CameraPathDelay point = null;
        if (pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            _cameraPath.editorPreview.transform.position = point.worldPosition;
            _cameraPath.editorPreview.transform.rotation = _animator.GetAnimatedOrientation(point.animationPercentage);
            _cameraPath.editorPreview.GetComponent<Camera>().fieldOfView = _cameraPath.GetPathFOV(point.percent);
            RenderPreview();
        }

        CPPointArrayInspector("Delay Points", pointList, CameraPathEditor.PointModes.Delay, CameraPathEditor.PointModes.AddDelays, CameraPathEditor.PointModes.RemoveDelays);

        if (point != null)
        {
            if(point == pointList.introPoint)
            {
                EditorGUILayout.LabelField("Start Point");
            }
            else if(point == pointList.outroPoint)
            {
                EditorGUILayout.LabelField("End Point");
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Custom Point Name");
                point.customName = EditorGUILayout.TextField(point.customName);
                if (GUILayout.Button("Clear"))
                    point.customName = "";
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Delay Time");
                point.time = EditorGUILayout.FloatField(point.time);
                EditorGUILayout.LabelField("seconds", GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private static void ModifyEaseInspector()
    {
        CameraPathDelayList pointList = _cameraPath.delayList;
        CameraPathDelay point = null;
        if (pointList.realNumberOfPoints > 0)
        {
            if (selectedPointIndex >= pointList.realNumberOfPoints)
                ChangeSelectedPointIndex(pointList.realNumberOfPoints - 1);
            point = pointList[selectedPointIndex];

            _cameraPath.editorPreview.transform.position = point.worldPosition;
            _cameraPath.editorPreview.transform.rotation = _animator.GetAnimatedOrientation(point.animationPercentage);
            _cameraPath.editorPreview.GetComponent<Camera>().fieldOfView = _cameraPath.GetPathFOV(point.percent);
            RenderPreview();
        }

        CPPointArrayInspector("Ease Points", pointList, CameraPathEditor.PointModes.Ease, CameraPathEditor.PointModes.Ease, CameraPathEditor.PointModes.Ease);

        if (point != null)
        {
            if (point == pointList.introPoint)
            {
                EditorGUILayout.LabelField("Start Point");
            }
            else if (point == pointList.outroPoint)
            {
                EditorGUILayout.LabelField("End Point");
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Custom Point Name");
                point.customName = EditorGUILayout.TextField(point.customName);
                if (GUILayout.Button("Clear"))
                    point.customName = "";
                EditorGUILayout.EndHorizontal();
            }

            if (point != pointList.introPoint)
            {
                EditorGUILayout.LabelField("Ease In Curve");
                point.introCurve = EditorGUILayout.CurveField(point.introCurve, GUILayout.Height(50));

                point.introStartEasePercentage = EditorGUILayout.FloatField(point.introStartEasePercentage);

                if (GUILayout.Button("None"))
                    point.outroCurve = AnimationCurve.Linear(0, 1, 1, 1);
                if (GUILayout.Button("Linear"))
                    point.introCurve = AnimationCurve.Linear(0,1,1,0);
                if (GUILayout.Button("Ease In"))
                    point.introCurve = new AnimationCurve(new[] { new Keyframe(0, 1, 0, 0.0f), new Keyframe(1, 0, -1.0f, 0) });

            }
            if (point != pointList.outroPoint)
            {
                EditorGUILayout.LabelField("Ease Out Curve");
                point.outroCurve = EditorGUILayout.CurveField(point.outroCurve, GUILayout.Height(50));
                point.outroEndEasePercentage = EditorGUILayout.FloatField(point.outroEndEasePercentage);

                if (GUILayout.Button("None"))
                    point.outroCurve = AnimationCurve.Linear(0, 1, 1, 1);
                if (GUILayout.Button("Linear"))
                    point.outroCurve = AnimationCurve.Linear(0, 0, 1, 1);
                if (GUILayout.Button("Ease Out"))
                    point.outroCurve = new AnimationCurve(new[] { new Keyframe(0, 0, 0, 1.0f), new Keyframe(1, 1, 0, 0) });
            }
        }
    }

    private static void PointListGUI()
    {

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Interpolation Algorithm");
        _cameraPath.interpolation = (CameraPath.Interpolation)EditorGUILayout.EnumPopup(_cameraPath.interpolation);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Path Points ");

        int numberOfPoints = _cameraPath.realNumberOfPoints;
        for (int i = 0; i < numberOfPoints; i++)
        {
            bool pointIsSelected = i == selectedPointIndex;
            EditorGUILayout.BeginHorizontal((pointIsSelected) ? selectedBox : unselectedBox);
            CameraPathControlPoint cpPoint = _cameraPath[i];
            EditorGUILayout.BeginHorizontal();
            if(cpPoint.customName=="")
                EditorGUILayout.LabelField("Point " + i);
            else
                EditorGUILayout.LabelField(cpPoint.customName);
            EditorGUI.BeginDisabledGroup(pointIsSelected);
            if (GUILayout.Button("Select"))
            {
                ChangeSelectedPointIndex(i);
                GotoScenePoint(cpPoint.worldPosition);
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Insert New Point"))
            {
                int atIndex = cpPoint.index + 1;
                CameraPathControlPoint pointA = _cameraPath.GetPoint(atIndex - 1);
                CameraPathControlPoint pointB = _cameraPath.GetPoint(atIndex);
                float newPointPercent = _cameraPath.GetPathPercentage(pointA, pointB, 0.5f);
                Vector3 newPointPosition = _cameraPath.GetPathPosition(newPointPercent,true);
                Vector3 newForwardControlPoint = _cameraPath.GetPathDirection(newPointPercent, true) * ((pointA.forwardControlPointLocal.magnitude + pointB.forwardControlPointLocal.magnitude) * 0.5f);

                CameraPathControlPoint newPoint = _cameraPath.InsertPoint(atIndex);
                newPoint.localPosition = newPointPosition;
                newPoint.forwardControlPointLocal = newForwardControlPoint;
            }
            EditorGUI.BeginDisabledGroup(numberOfPoints < 3);
            if (GUILayout.Button("Delete"))
            {
                _cameraPath.RemovePoint(cpPoint);
                return;//cheap, but effective. Cancel any further actions on this frame
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
        }
    }

    private static void CPPointArrayInspector(string title, CameraPathPointList pointList, CameraPathEditor.PointModes deflt, CameraPathEditor.PointModes add, CameraPathEditor.PointModes remove)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(title);
        int numberOfPoints = pointList.realNumberOfPoints;
        if(numberOfPoints==0)
            EditorGUILayout.LabelField("There are no points", redText);

        for (int i = 0; i < numberOfPoints; i++)
        {
            bool pointIsSelected = i == selectedPointIndex;
            EditorGUILayout.BeginHorizontal((pointIsSelected) ? selectedBox : unselectedBox);
            CameraPathPoint arrayPoint = pointList[i];
            EditorGUILayout.BeginHorizontal();
            if (arrayPoint.customName == "")
                EditorGUILayout.LabelField("Point " + i, GUILayout.Width(140));
            else
                EditorGUILayout.LabelField(arrayPoint.customName, GUILayout.Width(140));

            float valueTextSize = 120;
            switch(deflt)
            {
                case CameraPathEditor.PointModes.FOV:
                    CameraPathFOV fov = (CameraPathFOV)arrayPoint;
                    EditorGUILayout.LabelField(fov.FOV.ToString("F1"), GUILayout.Width(valueTextSize));
                    break;

                case CameraPathEditor.PointModes.Speed:
                    CameraPathSpeed speed = (CameraPathSpeed)arrayPoint;
                    EditorGUILayout.LabelField(speed.speed.ToString("F1"), GUILayout.Width(valueTextSize));
                    break;

                case CameraPathEditor.PointModes.Delay:
                    CameraPathDelay delay = (CameraPathDelay)arrayPoint;
                    EditorGUILayout.LabelField(delay.time.ToString("F1"), GUILayout.Width(valueTextSize));
                    break;

                case CameraPathEditor.PointModes.Orientations:
                    CameraPathOrientation orientation = (CameraPathOrientation)arrayPoint;
                    EditorGUILayout.LabelField(orientation.rotation.eulerAngles.ToString(), GUILayout.Width(valueTextSize));
                    break;

                case CameraPathEditor.PointModes.Tilt:
                    CameraPathTilt tilt = (CameraPathTilt)arrayPoint;
                    EditorGUILayout.LabelField(tilt.tilt.ToString("F1"), GUILayout.Width(valueTextSize));
                    break;

                case CameraPathEditor.PointModes.Events:
                    CameraPathEvent point = (CameraPathEvent)arrayPoint;
                    if(point.type == CameraPathEvent.Types.Broadcast)
                        EditorGUILayout.LabelField(point.eventName, GUILayout.Width(valueTextSize));
                    else
                        EditorGUILayout.LabelField(point.methodName, GUILayout.Width(valueTextSize));
                    break;
            }

            if (!pointIsSelected)
            {

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    ChangeSelectedPointIndex(i);
                    GotoScenePoint(arrayPoint.worldPosition);
                }
            }
            else
            {
                if (GUILayout.Button("Go to", GUILayout.Width(60)))
                {
                    GotoScenePoint(arrayPoint.worldPosition);
                }
            }

            if(deflt == CameraPathEditor.PointModes.Ease || deflt == CameraPathEditor.PointModes.ControlPoints)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
                continue;
            }

            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                pointList.RemovePoint(arrayPoint);
                return;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
        }

        if(deflt == CameraPathEditor.PointModes.Ease || deflt == CameraPathEditor.PointModes.ControlPoints)
            return;

        //ADD NEW POINTS
        if (pointMode != add)
        {
            if(GUILayout.Button("Add Points"))
            {
                ChangePointMode(add);
                NewPointMode(pointMode);
            }
        }
        else
        {
            if(GUILayout.Button("Done"))
            {
                ChangePointMode(deflt);
                NewPointMode(pointMode);
            }
        }

        EditorGUI.BeginDisabledGroup(numberOfPoints==0);
        if (pointMode != remove)
        {
            if(GUILayout.Button("Delete Points"))
            {
                ChangePointMode(remove);
                NewPointMode(pointMode);
            }
        }
        else
        {
            if(GUILayout.Button("Done"))
            {
                ChangePointMode(deflt);
                NewPointMode(pointMode);
            }
        }
        EditorGUI.EndDisabledGroup();
    }

    private static void OptionsInspectorGUI()
    {
        _cameraPath.showGizmos = EditorGUILayout.Toggle("Show Gizmos", _cameraPath.showGizmos);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Selected Path Colour");
        _cameraPath.selectedPathColour = EditorGUILayout.ColorField(_cameraPath.selectedPathColour);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Unselected Path Colour");
        _cameraPath.unselectedPathColour = EditorGUILayout.ColorField(_cameraPath.unselectedPathColour);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Selected Point Colour");
        _cameraPath.selectedPointColour = EditorGUILayout.ColorField(_cameraPath.selectedPointColour);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Unselected Point Colour");
        _cameraPath.unselectedPointColour = EditorGUILayout.ColorField(_cameraPath.unselectedPointColour);
        EditorGUILayout.EndHorizontal();

        if(GUILayout.Button("Reset Colours"))
        {
            _cameraPath.selectedPathColour = CameraPathColours.GREEN;
            _cameraPath.unselectedPathColour = CameraPathColours.GREY;
            _cameraPath.selectedPointColour = CameraPathColours.RED;
            _cameraPath.unselectedPointColour = CameraPathColours.GREEN;
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// A little hacking of the Unity Editor to allow us to focus on an arbitrary point in 3D Space
    /// We're replicating pressing the F button in scene view to focus on the selected object
    /// Here we can focus on a 3D point
    /// </summary>
    /// <param name="position">The 3D point we want to focus on</param>
    private static void GotoScenePoint(Vector3 position)
    {
        Object[] intialFocus = Selection.objects;
        GameObject tempFocusView = new GameObject("Temp Focus View");
        tempFocusView.transform.position = position;
        Selection.objects = new Object[] { tempFocusView };
        SceneView.lastActiveSceneView.FrameSelected();
        Selection.objects = intialFocus;
        Object.DestroyImmediate(tempFocusView);
    }

    private static void ToolbarMenuGUI()
    {
        bool isDefaultMenu = _animator.orientationMode != CameraPathAnimator.orientationModes.custom && _animator.orientationMode != CameraPathAnimator.orientationModes.followpath && _animator.orientationMode != CameraPathAnimator.orientationModes.reverseFollowpath;
        int currentPointModeA = -1;
        int currentPointModeB = -1;
        switch (pointMode)
        {
            case CameraPathEditor.PointModes.Transform:
                currentPointModeA = 0;
                break;
            case CameraPathEditor.PointModes.AddPathPoints:
                currentPointModeA = 0;
                break;
            case CameraPathEditor.PointModes.RemovePathPoints:
                currentPointModeA = 0;
                break;

            case CameraPathEditor.PointModes.ControlPoints:
                currentPointModeA = 1;
                break;

            case CameraPathEditor.PointModes.FOV:
                currentPointModeA = 2;
                break;
            case CameraPathEditor.PointModes.AddFovs:
                currentPointModeA = 2;
                break;
            case CameraPathEditor.PointModes.RemoveFovs:
                currentPointModeA = 2;
                break;

            case CameraPathEditor.PointModes.Speed:
                currentPointModeA = 3;
                break;
            case CameraPathEditor.PointModes.AddSpeeds:
                currentPointModeA = 3;
                break;
            case CameraPathEditor.PointModes.RemoveSpeeds:
                currentPointModeA = 3;
                break;

            case CameraPathEditor.PointModes.Delay:
                currentPointModeB = 0;
                break;
            case CameraPathEditor.PointModes.AddDelays:
                currentPointModeB = 0;
                break;
            case CameraPathEditor.PointModes.RemoveDelays:
                currentPointModeB = 0;
                break;

            case CameraPathEditor.PointModes.Ease:
                currentPointModeB = 1;
                break;

            case CameraPathEditor.PointModes.Events:
                currentPointModeB = 2;
                break;
            case CameraPathEditor.PointModes.AddEvents:
                currentPointModeB = 2;
                break;
            case CameraPathEditor.PointModes.RemoveEvents:
                currentPointModeB = 2;
                break;

            case CameraPathEditor.PointModes.Orientations:
                currentPointModeB = 3;
                break;
            case CameraPathEditor.PointModes.AddOrientations:
                currentPointModeB = 3;
                break;
            case CameraPathEditor.PointModes.RemoveOrientations:
                currentPointModeB = 3;
                break;

            case CameraPathEditor.PointModes.Tilt:
                currentPointModeB = 3;
                break;
            case CameraPathEditor.PointModes.AddTilts:
                currentPointModeB = 3;
                break;
            case CameraPathEditor.PointModes.RemoveTilts:
                currentPointModeB = 3;
                break;

            case CameraPathEditor.PointModes.Options:
                currentPointModeB = (isDefaultMenu) ? 3 : 4;
                break;
        }
        int newPointModeA = GUILayout.Toolbar(currentPointModeA, _toolBarGUIContentA, GUILayout.Width(320), GUILayout.Height(64));
        int newPointModeB = GUILayout.Toolbar(currentPointModeB, _toolBarGUIContentB, GUILayout.Width((isDefaultMenu)?320:400), GUILayout.Height(64));

        if(newPointModeA != currentPointModeA)
        {
            switch(newPointModeA)
            {
                case 0:
                    if(pointMode == CameraPathEditor.PointModes.AddPathPoints)
                        return;
                    if(pointMode == CameraPathEditor.PointModes.RemovePathPoints)
                        return;
                    ChangePointMode(CameraPathEditor.PointModes.Transform);
                    break;

                case 1:
                    ChangePointMode(CameraPathEditor.PointModes.ControlPoints);
                    break;

                case 2:
                    if(pointMode == CameraPathEditor.PointModes.AddFovs)
                        return;
                    if(pointMode == CameraPathEditor.PointModes.RemoveFovs)
                        return;
                    ChangePointMode(CameraPathEditor.PointModes.FOV);
                    break;

                case 3:
                    if(pointMode == CameraPathEditor.PointModes.AddSpeeds)
                        return;
                    if(pointMode == CameraPathEditor.PointModes.RemoveSpeeds)
                        return;
                    ChangePointMode(CameraPathEditor.PointModes.Speed);
                    break;
            }
            GUI.changed = true;
        }
        if (newPointModeB != currentPointModeB)
        {
            switch(newPointModeB)
            {
                case 0:
                    if(pointMode == CameraPathEditor.PointModes.AddDelays)
                        return;
                    if(pointMode == CameraPathEditor.PointModes.RemoveDelays)
                        return;
                    ChangePointMode(CameraPathEditor.PointModes.Delay);
                    break;

                case 1:
                    ChangePointMode(CameraPathEditor.PointModes.Ease);
                    break;

                case 2:
                    if(pointMode == CameraPathEditor.PointModes.AddEvents)
                        return;
                    if(pointMode == CameraPathEditor.PointModes.RemoveEvents)
                        return;
                    ChangePointMode(CameraPathEditor.PointModes.Events);
                    break;

                case 3:
                    if(isDefaultMenu)
                        ChangePointMode(CameraPathEditor.PointModes.Options);
                    else
                    {
                        if(_animator.orientationMode == CameraPathAnimator.orientationModes.custom)
                        {
                            if(pointMode == CameraPathEditor.PointModes.AddOrientations)
                                return;
                            if(pointMode == CameraPathEditor.PointModes.RemoveOrientations)
                                return;
                            ChangePointMode(CameraPathEditor.PointModes.Orientations);
                        }
                        else
                        {
                            if(pointMode == CameraPathEditor.PointModes.AddTilts)
                                return;
                            if(pointMode == CameraPathEditor.PointModes.RemoveTilts)
                                return;
                            ChangePointMode(CameraPathEditor.PointModes.Tilt);
                        }
                    }
                    break;

                case 4:
                    ChangePointMode(CameraPathEditor.PointModes.Options);
                    break;

            }
            GUI.changed = true;
        }
    }

    private static void RenderPreview()
    {
        if (_cameraPath.realNumberOfPoints < 2)
            return;

        GameObject editorPreview = _cameraPath.editorPreview;
        if (SystemInfo.supportsRenderTextures && !EditorApplication.isPlaying)
        {
            RenderTexture rt = RenderTexture.GetTemporary(previewResolution, Mathf.RoundToInt(previewResolution / aspect), 24, RenderTextureFormat.RGB565, RenderTextureReadWrite.Default, 1);

            editorPreview.SetActive(true);
            editorPreview.GetComponent<Camera>().enabled = true;
            editorPreview.GetComponent<Camera>().targetTexture = rt;
            editorPreview.GetComponent<Camera>().Render();
            editorPreview.GetComponent<Camera>().targetTexture = null;
            editorPreview.GetComponent<Camera>().enabled = false;
            editorPreview.SetActive(false);

            GUILayout.Label(rt, GUILayout.Width(400), GUILayout.Height(225));
            RenderTexture.ReleaseTemporary(rt);
        }
        else
        {
            EditorGUILayout.LabelField("No Preview When Playing", GUILayout.Height(225));
        }
    }

    private static void AddPointToEnd()
    {
        CameraPathControlPoint newPoint = ScriptableObject.CreateInstance<CameraPathControlPoint>();
        newPoint.baseTransform = _cameraPath.transform;
        Vector3 finalPathPosition = _cameraPath.GetPathPosition(1.0f) + cpPosition;
        Vector3 finalPathDirection = _cameraPath.GetPathDirection(1.0f);
        float finalArcLength = _cameraPath.StoredArcLength(_cameraPath.numberOfCurves - 1);
        Vector3 newPathPointPosition = finalPathPosition + finalPathDirection * (finalArcLength);
        newPoint.worldPosition = newPathPointPosition;
        newPoint.forwardControlPointLocal = _cameraPath[_cameraPath.realNumberOfPoints - 1].forwardControlPointLocal;
        _cameraPath.AddPoint(newPoint);
        ChangeSelectedPointIndex(_cameraPath.realNumberOfPoints - 1);
        GUI.changed = true;
    }

    private static void ChangePointMode(CameraPathEditor.PointModes newPointMode)
    {
        pointMode = newPointMode;
        NewPointMode(newPointMode);
    }

    private static void ChangeSelectedPointIndex(int newPointSelected)
    {
        selectedPointIndex = newPointSelected;
        NewPointSelected(newPointSelected);
    }

    public static void CleanUp()
    {
        if (_cameraPath.editorPreview != null)
            DestroyImmediate(_cameraPath.editorPreview);
        DestroyImmediate(selectedBoxColour);
        DestroyImmediate(unselectedBoxColour);
    }

    private static void SetupToolbar()
    {
        int menuType = 0;
        switch (_animator.orientationMode)
        {
            case CameraPathAnimator.orientationModes.custom:
                menuType = 1;
                break;

            case CameraPathAnimator.orientationModes.followpath:
                menuType = 2;
                break;

            case CameraPathAnimator.orientationModes.reverseFollowpath:
                menuType = 2;
                break;

            case CameraPathAnimator.orientationModes.target:
                menuType = 0;
                break;

            case CameraPathAnimator.orientationModes.followTransform:
                menuType = 0;
                break;

            case CameraPathAnimator.orientationModes.mouselook:
                menuType = 0;
                break;

        }

        int menuLengthA = 0;
        int menuLengthB = 0;
        string[] menuStringA = new string[0];
        string[] menuStringB = new string[0];
        Texture2D[] toolbarTexturesA = new Texture2D[0];
        Texture2D[] toolbarTexturesB = new Texture2D[0];
        switch (menuType)
        {
            default:
                menuLengthA = 4;
                menuLengthB = 4;
                menuStringA = new[] { "Path Points", "Control Points", "FOV", "Speed" };
                menuStringB = new[] { "Delays", "Ease", "Events", "Options" };
                toolbarTexturesA = new Texture2D[menuLengthA];
                toolbarTexturesB = new Texture2D[menuLengthB];
                toolbarTexturesA[0] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/pathpoints.png", typeof(Texture2D));
                toolbarTexturesA[1] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/controlpoints.png", typeof(Texture2D));
                toolbarTexturesA[2] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/fov.png", typeof(Texture2D));
                toolbarTexturesA[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/speed.png", typeof(Texture2D));
                toolbarTexturesB[0] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/delay.png", typeof(Texture2D));
                toolbarTexturesB[1] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/easecurves.png", typeof(Texture2D));
                toolbarTexturesB[2] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/events.png", typeof(Texture2D));
                toolbarTexturesB[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/options.png", typeof(Texture2D));

                break;
            case 1:
                menuLengthA = 4;
                menuLengthB = 5;
                menuStringA = new[] { "Path Points", "Control Points", "FOV", "Speed"};
                menuStringB = new[] { "Delays", "Ease", "Events", "Orientations", "Options" };
                toolbarTexturesA = new Texture2D[menuLengthA];
                toolbarTexturesB = new Texture2D[menuLengthB];
                toolbarTexturesA[0] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/pathpoints.png", typeof(Texture2D));
                toolbarTexturesA[1] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/controlpoints.png", typeof(Texture2D));
                toolbarTexturesA[2] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/fov.png", typeof(Texture2D));
                toolbarTexturesA[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/speed.png", typeof(Texture2D));
                toolbarTexturesB[0] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/delay.png", typeof(Texture2D));
                toolbarTexturesB[1] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/easecurves.png", typeof(Texture2D));
                toolbarTexturesB[2] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/events.png", typeof(Texture2D));
                toolbarTexturesB[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/orientation.png", typeof(Texture2D));
                toolbarTexturesB[4] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/options.png", typeof(Texture2D));
                break;
            case 2:
                menuLengthA = 4;
                menuLengthB = 5;
                menuStringA = new[] { "Path Points", "Control Points", "FOV", "Speed"};
                menuStringB = new[] { "Delays", "Ease", "Events", "Tilt", "Options" };
                toolbarTexturesA = new Texture2D[menuLengthA];
                toolbarTexturesB = new Texture2D[menuLengthB];
                toolbarTexturesA[0] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/pathpoints.png", typeof(Texture2D));
                toolbarTexturesA[1] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/controlpoints.png", typeof(Texture2D));
                toolbarTexturesA[2] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/fov.png", typeof(Texture2D));
                toolbarTexturesA[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/speed.png", typeof(Texture2D));
                toolbarTexturesB[0] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/delay.png", typeof(Texture2D));
                toolbarTexturesB[1] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/easecurves.png", typeof(Texture2D));
                toolbarTexturesB[2] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/events.png", typeof(Texture2D));
                toolbarTexturesB[3] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/tilt.png", typeof(Texture2D));
                toolbarTexturesB[4] = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/CameraPath3/Icons/options.png", typeof(Texture2D));
                break;
        }
        _toolBarGUIContentA = new GUIContent[menuLengthA];
        for (int i = 0; i < menuLengthA; i++)
            _toolBarGUIContentA[i] = new GUIContent(toolbarTexturesA[i], menuStringA[i]);
        _toolBarGUIContentB = new GUIContent[menuLengthB];
        for (int i = 0; i < menuLengthB; i++)
            _toolBarGUIContentB[i] = new GUIContent(toolbarTexturesB[i], menuStringB[i]);

        _orientationmode = _animator.orientationMode;
    }
}
