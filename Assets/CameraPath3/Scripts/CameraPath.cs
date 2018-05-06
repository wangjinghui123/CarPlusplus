// Camera Path 3
// Available on the Unity Asset Store
// Copyright (c) 2013 Jasper Stocker http://support.jasperstocker.com/camera-path/
// For support contact email@jasperstocker.com
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CameraPath : MonoBehaviour
{
    [SerializeField]
    private List<CameraPathControlPoint> _points = new List<CameraPathControlPoint>();

    public enum Interpolation
    {
        Linear,
        SmoothStep,
        Hermite,
        Bezier
    }

    [SerializeField]
    private Interpolation _interpolation = Interpolation.Bezier;

    [SerializeField]
    private bool initialised = false;

    //this is the length of the arc of the entire bezier curve
    [SerializeField]
    private float _storedTotalArcLength = 0;
    //this is an arroy of arc lengths in a point by point basis
    [SerializeField]
    private float[] _storedArcLengths = null;
    //this is an array of arc lenths are intervals specified by storedValueArraySize
    //it is the main data used in normalising the bezier curve to acheive a constant speed thoughout
    [SerializeField]
    private float[] _storedArcLengthsFull = null;

    [SerializeField]
    private Vector3[] _storedPoints = null;

    [SerializeField]
    private float[] _normalisedPercentages = null;

    //the unity distance of intervals to precalculate points
    //you can modify this number to get a faster output for RecalculateStoredValues
    //higher = faster recalculation/lower accuracy
    //lower = slower recalculation/higher accuracy
    [SerializeField]
    private const float STORED_POINT_RESOLUTION = 0.1f;//world units
    [SerializeField]
    private int _storedValueArraySize = 0;//calculated from above based on path length and resolution

    [SerializeField]
    private Vector3[] _storedPathDirections = null;//a list of path directions stored for other calculation

    [SerializeField]
    private CameraPathControlPoint[] _pointALink = null;//a link to the point a for each stored point
    [SerializeField]
    private CameraPathControlPoint[] _pointBLink = null;//a link to the point a for each stored point

    [SerializeField]
    private CameraPathOrientationList _orientationList = null;

    [SerializeField]
    private CameraPathFOVList _fovList = null;//the list of FOV points

    [SerializeField]
    private CameraPathTiltList _tiltList = null;

    [SerializeField]
    private CameraPathSpeedList _speedList = null;

    [SerializeField]
    private CameraPathEventList _eventList = null;

    [SerializeField]
    private CameraPathDelayList _delayList = null;

    [SerializeField]
    private bool _addOrientationsWithPoints = true;
    
    [SerializeField]
    private bool _looped = false;//is the path looped

    [SerializeField]
    private bool _normalised = true;

    private const float CLIP_THREASHOLD = 0.5f;

    [SerializeField]
    private Bounds _pathBounds = new Bounds();

    public GameObject editorPreview = null;

    [SerializeField]
    private CameraPath _nextPath = null;//link this path to a second one

    [SerializeField]
    private bool _interpolateNextPath = false;//should we interpolate to that next path?

    [SerializeField]
    private CameraPath _lastPath = null;

    [SerializeField]
    private CameraPathControlPoint _dummyLastPoint;

    //Camera Path Options
    public bool showGizmos = true;
    public Color selectedPathColour = CameraPathColours.GREEN;
    public Color unselectedPathColour = CameraPathColours.GREY;
    public Color selectedPointColour = CameraPathColours.RED;
    public Color unselectedPointColour = CameraPathColours.GREEN;
    public bool showOrientationIndicators = false;
    public float orientationIndicatorUnitLength = 0.5f;
    public Color orientationIndicatorColours = CameraPathColours.PURPLE;

    //Camera Path Events
    public delegate void RecalculateCurvesHandler();
    public delegate void PathPointAddedHandler(CameraPathControlPoint point);
    public delegate void PathPointRemovedHandler(CameraPathControlPoint point);
    public delegate void CheckStartPointCullHandler(float percentage);
    public delegate void CheckEndPointCullHandler(float percentage);
    public delegate void CleanUpListsHandler();

    public event RecalculateCurvesHandler RecalculateCurvesEvent;
    public event PathPointAddedHandler PathPointAddedEvent;
    public event PathPointRemovedHandler PathPointRemovedEvent;
    public event CheckStartPointCullHandler CheckStartPointCullEvent;
    public event CheckEndPointCullHandler CheckEndPointCullEvent;
    public event CleanUpListsHandler CleanUpListsEvent;

    /// <summary>
    /// Get a point in the path list
    /// Handles looping, next path interpolation and index outside of range
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public CameraPathControlPoint this[int index]
    {
        get
        {
            int pointCount = _points.Count;
            if(_looped)
            {
                if (_nextPath == null && !_interpolateNextPath)
                    index = index % pointCount;
                else
                {
                    if(index == pointCount)
                        index = 0;
                    else if(index == pointCount + 1)
                        return _dummyLastPoint;
                    else
                        Debug.LogError("Index out of range");
                }
            }
            else
            {
                if (index < 0)
                    Debug.LogError("Index can't be minus");
                if (index >= _points.Count)
                {
                    if (index == _points.Count && nextPath != null)
                        return _dummyLastPoint;
                    else 
                        Debug.LogError("Index out of range");
                }
            }
            return _points[index];
        }
    }

    /// <summary>
    /// The number of points in thie path including duplcates for looping or linked path interpolation
    /// </summary>
    public int numberOfPoints
    {
        get
        {
            if (_points.Count == 0)
                return 0;
            int output = (_looped) ? _points.Count + 1 : _points.Count;
            if (_nextPath != null && _interpolateNextPath)
                output++;
            return output;
        }
    }

    /// <summary>
    /// The physical number of points this camera path has
    /// </summary>
    public int realNumberOfPoints { get { return _points.Count; } }

    /// <summary>
    /// The number of curves in this path including any additional curves generated by looping or linked paths
    /// </summary>
    public int numberOfCurves
    {
        get
        {
            if (_points.Count < 2)
                return 0;
            return numberOfPoints - 1;
        }
    }

    /// <summary>
    /// Does this path loop back on itself
    /// </summary>
    public bool loop
    {
        get { return _looped; }
        set
        {
            if (_looped != value)
            {
                _looped = value;
                RecalculateStoredValues();
            }
        }
    }

    /// <summary>
    /// The length in world units of the path
    /// </summary>
    public float pathLength { get { return _storedTotalArcLength; } }

    public CameraPathOrientationList orientationList {get {return _orientationList;}}
    public CameraPathFOVList fovList {get {return _fovList;}}
    public CameraPathTiltList tiltList {get {return _tiltList;}}
    public CameraPathSpeedList speedList {get {return _speedList;}}
    public CameraPathEventList eventList {get {return _eventList;}}
    public CameraPathDelayList delayList {get {return _delayList;}}

    /// <summary>
    /// The bounds this path occupies
    /// </summary>
    public Bounds bounds {get {return _pathBounds;}}

    /// <summary>
    /// The arc length of a specified curve in world units
    /// </summary>
    /// <param name="curve">The index of the curve</param>
    /// <returns></returns>
    public float StoredArcLength(int curve)
    {
        return _storedArcLengths[curve];
    }

    public int storedValueArraySize {get {return _storedValueArraySize;}}

    public CameraPathControlPoint[] pointALink {get {return _pointALink;}}

    public CameraPathControlPoint[] pointBLink {get {return _pointBLink;}}

    public Vector3[] storedPoints {get {return _storedPoints;}}

    /// <summary>
    /// Is the path normalised so that speed and be constant throughout the animation
    /// </summary>
    public bool normalised {get {return _normalised;} set {_normalised = value;}}

    /// <summary>
    /// What kind of path interpolation is used for this path?
    /// </summary>
    public Interpolation interpolation
    {
        get {return _interpolation;} 
        set
        {
            if(value != _interpolation)
            {
                _interpolation = value;
                RecalculateStoredValues();
            }
        }
    }

    /// <summary>
    /// Link another Camera Path to the end of this one.
    /// </summary>
    public CameraPath nextPath
    {
        get {return _nextPath;} 
        set
        {
            if(value == this)
            {
                Debug.LogError("Do not link a path to itself! The Universe would crumble and it would be your fault!! If you want to loop a path, just toggle the loop option...");
                return;
            }
            if(_nextPath != null)
            {
                _nextPath.lastPath = null;
                _nextPath = null;
            }
            if(value != null)
            {
                _nextPath = value;
                UpdateDummyPoint();
                _nextPath.lastPath = this;
            }
            else
            {
                UpdateDummyPoint();
            }
            RecalculateStoredValues();
        }
    }

    /// <summary>
    /// Should we interpolate this path into a linked one
    /// </summary>
    public bool interpolateNextPath
    {
        get { return _interpolateNextPath; } 
        set
        {
            if(_interpolateNextPath != value)
            {
                _interpolateNextPath = value;
                RecalculateStoredValues();
            }
        }
    }

    public CameraPath lastPath
    {
        set
        {
            _lastPath = value;
        }
    }

    public CameraPathControlPoint dummyLastPoint {get {return _dummyLastPoint;}}

    public int StoredValueIndex(float percentage)
    {
        int max = storedValueArraySize - 1;
        return Mathf.Clamp(Mathf.RoundToInt(max * percentage), 0, max);
    }

    /// <summary>
    /// Add a point to the camera path by position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public CameraPathControlPoint AddPoint(Vector3 position)
    {
        CameraPathControlPoint point = ScriptableObject.CreateInstance<CameraPathControlPoint>();
        point.baseTransform = transform;
        point.localPosition = position;
        _points.Add(point);

        if (_addOrientationsWithPoints) orientationList.AddOrientation(point);
        RecalculateStoredValues();
        PathPointAddedEvent(point);
        return point;
    }

    /// <summary>
    /// Add a specified point to the camera path
    /// </summary>
    /// <param name="point"></param>
    public void AddPoint(CameraPathControlPoint point)
    {
        point.baseTransform = transform;
        _points.Add(point);
        RecalculateStoredValues();
        PathPointAddedEvent(point);
    }

    /// <summary>
    /// Insert a specified point into the camera path at an index
    /// </summary>
    /// <param name="point"></param>
    /// <param name="index"></param>
    public void InsertPoint(CameraPathControlPoint point, int index)
    {
        point.baseTransform = transform;
        _points.Insert(index, point);
        RecalculateStoredValues();
        PathPointAddedEvent(point);
    }

    public CameraPathControlPoint InsertPoint(int index)
    {
        CameraPathControlPoint point = ScriptableObject.CreateInstance<CameraPathControlPoint>();
        point.baseTransform = transform;
        _points.Insert(index, point);
        RecalculateStoredValues();
        PathPointAddedEvent(point);
        return point;
    }

    public void RemovePoint(CameraPathControlPoint point)
    {
        if (_points.Count < 3)
        {
            Debug.Log("We can't see any point in allowing you to delete any more points so we're not going to do it.");
            return;
        }
        PathPointRemovedEvent(point);

        int pointIndex = _points.IndexOf(point);
        if(pointIndex == 0)
        {
            //check other points
            float percentageCull = GetPathPercentage(1);
            CheckStartPointCullEvent(percentageCull);
        }
        if (pointIndex == realNumberOfPoints - 1)
        {
            //check other points
            float percentageCull = GetPathPercentage(realNumberOfPoints - 2);
            CheckEndPointCullEvent(percentageCull);
        }   

        _points.Remove(point);
        RecalculateStoredValues();
    }

    /// <summary>
    /// Parse a percent value so it can take into account any looping or normalisation
    /// </summary>
    /// <param name="percentage">Path Percent 0-1</param>
    /// <returns>A processed percentage</returns>
    private float ParsePercentage(float percentage)
    {
        if(_looped)
            percentage = percentage % 1.0f;
        else
            percentage = Mathf.Clamp01(percentage);

        if(_normalised)
        {
            int max = storedValueArraySize - 1;
            float storedPointSize = (1.0f / storedValueArraySize);
            int normalisationIndex = Mathf.Clamp(Mathf.FloorToInt(storedValueArraySize * percentage), 0, max);
            int nextNormalisationIndex = Mathf.Clamp(normalisationIndex + 1, 0, max);
            float normalisationPercentA = normalisationIndex * storedPointSize;
            float normalisationPercentB = nextNormalisationIndex * storedPointSize;
            float normPercentA = _normalisedPercentages[normalisationIndex];
            float normPercentB = _normalisedPercentages[nextNormalisationIndex];
            if(normPercentA == normPercentB) return percentage;
            float lerpValue = (percentage - normalisationPercentA) / (normalisationPercentB - normalisationPercentA);
            percentage = Mathf.Lerp(normPercentA, normPercentB, lerpValue);
        }
        return percentage;
    }

    
    

    //Normalise the time based on the curve point
    //Put in a time and it returns a time that will account for arc lengths
    //Useful to ensure that path is animated at a constant speed
    //percentage - time(0-1)
    /// <summary>
    /// Normalise the time based on the curve point
    /// Put in a time and it returns a time that will account for arc lengths
    /// Useful to ensure that path is animated at a constant speed
    /// </summary>
    /// <param name="percentage">Path Percentage - 0-1</param>
    /// <returns></returns>
    public float CalculateNormalisedPercentage(float percentage)
    {
        if(realNumberOfPoints < 2)
            return percentage;
        if (percentage == 0)
            return 0;
        if (percentage == 1)
            return 1;

        float targetLength = percentage * _storedTotalArcLength;

        int low = 0;
        int high = _storedValueArraySize;
        int index = 0;
        while (low < high)
        {
            index = low + ((high - low) / 2);
            if (_storedArcLengthsFull[index] < targetLength)
                low = index + 1;
            else
                high = index;
        }

        if (_storedArcLengthsFull[index] > targetLength && index > 0)
            index--;

        float lengthBefore = _storedArcLengthsFull[index];
        float currentT = (float)index / (float)_storedValueArraySize;
        if (lengthBefore == targetLength)
        {
            return currentT;
        }
        else
        {
            return (index + (targetLength - lengthBefore) / (_storedArcLengthsFull[index + 1] - lengthBefore)) / _storedValueArraySize;
        }
    }

    /// <summary>
    /// Get the index of a point at the start of a curve based on path percentage
    /// </summary>
    /// <param name="percentage">Path Percent 0-1</param>
    /// <returns>Index of point</returns>
    public int GetPointNumber(float percentage)
    {
        percentage = ParsePercentage(percentage);
        float curveT = 1.0f / numberOfCurves;
        return Mathf.Clamp(Mathf.FloorToInt(percentage / curveT), 0, (_points.Count - 1));
    }

    /// <summary>
    /// Get a normalised position based on a percent of the path
    /// </summary>
    /// <param name="percentage">Path Percent 0-1</param>
    /// <returns>Path Postion</returns>
    public Vector3 GetPathPosition(float percentage)
    {
        return GetPathPosition(percentage, false);
    }

    /// <summary>
    /// Get a position based on a percent of the path specifying the result will be normalised or not
    /// </summary>
    /// <param name="percentage">Path Percent 0-1</param>
    /// <param name="ignoreNormalisation">Should we ignore path normalisation</param>
    /// <returns>Path Postion</returns>
    public Vector3 GetPathPosition(float percentage, bool ignoreNormalisation)
    {
        if (realNumberOfPoints < 2)
        {
            Debug.LogError("Not enough points to define a curve");
            return Vector3.zero;
        }
        if (!ignoreNormalisation) 
            percentage = ParsePercentage(percentage);
        float curveT = 1.0f / numberOfCurves;
        int point = Mathf.FloorToInt(percentage / curveT);
        float ct = Mathf.Clamp01((percentage - point * curveT) * numberOfCurves);
        CameraPathControlPoint pointA = GetPoint(point);
        CameraPathControlPoint pointB = GetPoint(point + 1);

        switch(interpolation)
        {
            case Interpolation.Bezier:
                return CPMath.CalculateBezier(ct, pointA.localPosition, pointA.forwardControlPoint, pointB.backwardControlPoint, pointB.localPosition);

            case Interpolation.Hermite:
                CameraPathControlPoint pointC = GetPoint(point - 1);
                CameraPathControlPoint pointD = GetPoint(point + 2);
                return CPMath.CalculateHermite(pointC.localPosition, pointA.localPosition, pointB.localPosition, pointD.localPosition,ct);

            case Interpolation.SmoothStep:
                return Vector3.Lerp(pointA.localPosition, pointB.localPosition, CPMath.SmoothStep(ct));

            case Interpolation.Linear:
                return Vector3.Lerp(pointA.localPosition, pointB.localPosition, ct);
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Retreive a rotation from the orientation list
    /// </summary>
    /// <param name="percentage">Path Percentage</param>
    /// <returns>A path rotation</returns>
    public Quaternion GetPathRotation(float percentage)
    {
        percentage = ParsePercentage(percentage);
        return orientationList.GetOrientation(percentage);
    }

    /// <summary>
    /// Retrive a path direction from stored values
    /// </summary>
    /// <param name="percentage">Path Percent 0-1</param>
    /// <returns>The direction of the path at this percent</returns>
    public Vector3 GetPathDirection(float percentage)
    {
        return GetPathDirection(percentage, true);
    }

    /// <summary>
    /// Retrive a path direction from stored values
    /// </summary>
    /// <param name="percentage">Path Percent 0-1</param>
    /// <param name="normalisePercent">Should we normalise the result</param>
    /// <returns>The direction of the path at this percent</returns>
    public Vector3 GetPathDirection(float percentage, bool normalisePercent)
    {
        if (normalisePercent) percentage = ParsePercentage(percentage);
        return _storedPathDirections[StoredValueIndex(percentage)];
    }

    /// <summary>
    /// Retreive a tilt from the tilt list
    /// </summary>
    /// <param name="percentage"></param>
    /// <returns></returns>
    public float GetPathTilt(float percentage)
    {
        percentage = ParsePercentage(percentage);
        return _tiltList.GetTilt(percentage);
    }

    public float GetPathFOV(float percentage)
    {
        percentage = ParsePercentage(percentage);
        return _fovList.GetFOV(percentage);
    }

    public float GetPathSpeed(float percentage)
    {
        percentage = ParsePercentage(percentage);
        float speed = _speedList.GetSpeed(percentage);
        speed *= _delayList.CheckEase(percentage);
        return speed;
    }

    public float GetPathEase(float percentage)
    {
        percentage = ParsePercentage(percentage);
        float output = _delayList.CheckEase(percentage);
        return output;
    }

    /// <summary>
    /// Check the event list for any events that should have been fired since last call
    /// </summary>
    /// <param name="percentage">The current path percent 0-1</param>
    public void CheckEvents(float percentage)
    {
        percentage = ParsePercentage(percentage);
        _eventList.CheckEvents(percentage);
        _delayList.CheckEvents(percentage);
    }

    /// <summary>
    /// Get the unnormalised percent value at a point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public float GetPathPercentage(CameraPathControlPoint point)
    {
        int index = _points.IndexOf(point);
        return index / (float)numberOfCurves;
    }

    /// <summary>
    /// Get the unnormalised percent value at a point
    /// </summary>
    /// <param name="pointIndex"></param>
    /// <returns></returns>
    public float GetPathPercentage(int pointIndex)
    {
        return pointIndex / (float)numberOfCurves;
    }

    public int GetNearestPointIndex(float percentage)
    {
        percentage = ParsePercentage(percentage);
        return Mathf.RoundToInt(numberOfCurves * percentage);
    }

    public int GetLastPointIndex(float percentage, bool isNormalised)
    {
        if (isNormalised) percentage = ParsePercentage(percentage);
        return Mathf.FloorToInt(numberOfCurves * percentage);
    }

    public int GetNextPointIndex(float percentage, bool isNormalised)
    {
        if (isNormalised) percentage = ParsePercentage(percentage);
        return Mathf.CeilToInt(numberOfCurves * percentage);
    }

    /// <summary>
    /// Get the percentage on the curve between two path points
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="percentage"></param>
    /// <returns></returns>
    public float GetCurvePercentage(CameraPathControlPoint pointA, CameraPathControlPoint pointB, float percentage)
    {
        float pointAPerc = GetPathPercentage(pointA);
        float pointBPerc = GetPathPercentage(pointB);
        if(pointAPerc == pointBPerc)
            return pointAPerc;
        return Mathf.Clamp01((percentage - pointAPerc) / (pointBPerc - pointAPerc));
    }

    /// <summary>
    /// Get the percentage of the curve between two points
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="percentage"></param>
    /// <returns></returns>
    public float GetCurvePercentage(CameraPathPoint pointA, CameraPathPoint pointB, float percentage)
    {
        float pointAPerc = pointA.percent;
        float pointBPerc = pointB.percent;
        return Mathf.Clamp01((percentage - pointAPerc) / (pointBPerc - pointAPerc));
    }

    /// <summary>
    /// Calculate the curve percenteage of a point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public float GetCurvePercentage(CameraPathPoint point)
    {
        float pointAPerc = GetPathPercentage(point.cpointA);
        float pointBPerc = GetPathPercentage(point.cpointB);
        point.curvePercentage = Mathf.Clamp01((point.percent - pointAPerc) / (pointBPerc - pointAPerc));
        return point.curvePercentage;
    }

    /// <summary>
    /// Retrieve the ease value of any ease outros at the specified percentage
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public float GetOutroEasePercentage(CameraPathDelay point)
    {
        float pointAPerc = point.percent;
        float pointBPerc = _delayList.GetPoint(point.index + 1).percent;
        return Mathf.Lerp(pointAPerc, pointBPerc, point.outroEndEasePercentage);
    }

    /// <summary>
    ///  Retrieve the ease value of any ease intros at the specified percentage
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public float GetIntroEasePercentage(CameraPathDelay point)
    {
        float pointAPerc = _delayList.GetPoint(point.index - 1).percent;
        float pointBPerc = point.percent;
        return Mathf.Lerp(pointAPerc, pointBPerc, 1-point.introStartEasePercentage);
    }

    /// <summary>
    /// Get the path percentage from a curve percent between two points
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="curvePercentage"></param>
    /// <returns></returns>
    public float GetPathPercentage(CameraPathControlPoint pointA, CameraPathControlPoint pointB, float curvePercentage)
    {
        float pointAPerc = GetPathPercentage(pointA);
        float pointBPerc = GetPathPercentage(pointB);
        return Mathf.Lerp(pointAPerc, pointBPerc, curvePercentage);
    }

    /// <summary>
    /// Get the path percentage from a curve percent between two points
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="curvePercentage"></param>
    /// <returns></returns>
    public float GetPathPercentage(float pointA, float pointB, float curvePercentage)
    {
        return Mathf.Lerp(pointA, pointB, curvePercentage);
    }

    public int GetStoredPoint(float percentage)
    {
        percentage = ParsePercentage(percentage);
        int returnIndex = Mathf.Clamp(Mathf.FloorToInt(storedValueArraySize* percentage),0,storedValueArraySize-1);
        return returnIndex;
    }

    private void Awake()
    {
        Init();
    }

    private void OnValidate()
    {
        //on script recompilation
        InitialiseLists();
#if UNITY_EDITOR
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying)
            return;
#endif
        if (!Application.isPlaying)
            RecalculateStoredValues();
    }

    private void OnDestroy()
    {
        Clear();
        if(CleanUpListsEvent != null)
            CleanUpListsEvent();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(!showGizmos)
            return;

        if(Selection.Contains(gameObject))
            return;

        if (numberOfCurves < 1)
            return;

        //Draw path outline
        Camera sceneCamera = Camera.current;
        Gizmos.color = unselectedPathColour;
        for (int i = 0; i < numberOfPoints - 1; i++)
        {
            CameraPathControlPoint pointA = GetPoint(i);
            CameraPathControlPoint pointB = GetPoint(i + 1);

            float dotPA = Vector3.Dot(sceneCamera.transform.forward, pointA.worldPosition - sceneCamera.transform.position);
            float dotPB = Vector3.Dot(sceneCamera.transform.forward, pointB.worldPosition - sceneCamera.transform.position);

            if (dotPA < 0 && dotPB < 0)//points are both behind camera - don't render
                continue;

            float pointAPercentage = pointA.percentage;
            float pointBPercentage = pointB.percentage;
            float arcPercentage = pointBPercentage - pointAPercentage;
            Vector3 arcCentre = (pointA.worldPosition + pointB.worldPosition) * 0.5f;
            float arcLength = StoredArcLength(GetCurveIndex(pointA.index));
            float arcDistance = Vector3.Distance(sceneCamera.transform.position, arcCentre);
            int arcPoints = Mathf.RoundToInt(arcLength * (40 / Mathf.Max(arcDistance, 20)));
            float arcTime = 1.0f / arcPoints;

            float endLoop = 1.0f - arcTime;
            Vector3 lastPoint = Vector3.zero;
            for (float p = 0; p < endLoop; p += arcTime)
            {
                float p2 = p + arcTime;
                float pathPercentageA = pointAPercentage + arcPercentage * p;
                float pathPercentageB = pointAPercentage + arcPercentage * p2;
                Vector3 lineStart = GetPathPosition(pathPercentageA, true);
                Vector3 lineEnd = GetPathPosition(pathPercentageB, true);
                Gizmos.DrawLine(lineStart + transform.position, lineEnd + transform.position);
                lastPoint = lineEnd;
            }
            Gizmos.DrawLine(lastPoint + transform.position, GetPathPosition(pointB.percentage, true) + transform.position);
        }
    }
#endif

    /// <summary>
    /// Calculate stored values that camera path uses
    /// Mostly this is used to establish a normalised curve so speed can be maintained.
    /// A few other functions are completed too like assigning values to points like name
    /// </summary>
    public void RecalculateStoredValues()
    {
        //Assign basic values to points
        for (int i = 0; i < realNumberOfPoints; i++)
        {
            _points[i].percentage = GetPathPercentage(i);//assign point percentages
            _points[i].normalisedPercentage = CalculateNormalisedPercentage(_points[i].percentage);//assign point percentages
            _points[i].name = "Point " + i;
            _points[i].index = i;
        }

        if (_points.Count < 2)
            return;//nothing to cache
        float curveT;
        if (numberOfCurves < 1)
            curveT = 1.0f;
        else
            curveT = 1.0f / (float)numberOfCurves;

        //Calculate some rough arc lengths
        _storedTotalArcLength = 0;
        for (int i = 0; i < numberOfCurves; i++)
        {
            CameraPathControlPoint pointA = GetPoint(i);
            CameraPathControlPoint pointB = GetPoint(i+1);
            float thisArcLength = 0;
            thisArcLength += Vector3.Distance(pointA.localPosition, pointA.forwardControlPoint);
            thisArcLength += Vector3.Distance(pointA.forwardControlPoint, pointB.backwardControlPoint);
            thisArcLength += Vector3.Distance(pointB.backwardControlPoint, pointB.localPosition);
            _storedTotalArcLength += thisArcLength;
        }

        _storedValueArraySize = Mathf.Max(Mathf.RoundToInt(_storedTotalArcLength / STORED_POINT_RESOLUTION), 1);

        _storedArcLengths = new float[numberOfCurves];
        float alTime = 1.0f / (_storedValueArraySize);
        float calculatedTotalArcLength = 0;
        _storedArcLengthsFull = new float[_storedValueArraySize];
        _storedArcLengthsFull[0] = 0.0f;
        for (int i = 0; i < _storedValueArraySize - 1; i++)
        {
            float altA = alTime * (i + 1);
            float altB = alTime * (i + 1) + alTime;
            Vector3 pA = GetPathPosition(altA, true);
            Vector3 pB = GetPathPosition(altB, true);
            float arcLength = Vector3.Distance(pA, pB);
            calculatedTotalArcLength += arcLength;
            int arcpoint = Mathf.FloorToInt(altA * numberOfCurves);
            _storedArcLengths[arcpoint] += arcLength;
            _storedArcLengthsFull[i + 1] = calculatedTotalArcLength;
        }
        _storedTotalArcLength = calculatedTotalArcLength;

        _storedPoints = new Vector3[_storedValueArraySize];
        _storedPathDirections = new Vector3[_storedValueArraySize];
        _normalisedPercentages = new float[_storedValueArraySize];
        for(int i = 0; i < _storedValueArraySize; i++)
        {
            float altA = alTime * (i + 1);
            float altB = alTime * (i + 1);
            float altC = alTime * (i - 1);
            _normalisedPercentages[i] = CalculateNormalisedPercentage(altA);
            Vector3 pA = GetPathPosition(altA, true);
            Vector3 pB = GetPathPosition(altB, true);
            Vector3 pC = GetPathPosition(altC, true);
            _storedPathDirections[i] = (((pB - pA) + (pB - pC)) * 0.5f).normalized;
        }

        for(int i = 0; i < _storedValueArraySize; i++)
        {
            float altA = alTime * (i);
            float altB = alTime * (i + 1);
            float altC = alTime * (i - 1);
            Vector3 pA = GetPathPosition(altA);
            _storedPoints[i] = pA;
        }

        if (RecalculateCurvesEvent != null)
            RecalculateCurvesEvent();

        //TODO: Solve stack overflow when links are cicular...
//        if(_lastPath!=null)
//        {
//            _lastPath.RecalculateStoredValues();//update last path
//        }

        UpdateDummyPoint();
    }

    /// <summary>
    /// Find the nearest point on the path to a point in world space
    /// </summary>
    /// <param name="fromPostition">A point in world space</param>
    /// <returns></returns>
    public float GetNearestPoint(Vector3 fromPostition)
    {
        int testPoints = 10;
        float testResolution = 1.0f / testPoints;
        float nearestPercentage = 0;
        float nextNearestPercentage = 0;
        float nearestPercentageSqrDistance = Mathf.Infinity;
        float nextNearestPercentageSqrDistance = Mathf.Infinity;
        for (float i = 0; i < 1; i += testResolution)
        {
            Vector3 point = GetPathPosition(i);
            Vector3 difference = point - fromPostition;
            float newSqrDistance = Vector3.SqrMagnitude(difference);
            if (nearestPercentageSqrDistance > newSqrDistance)
            {
                nearestPercentage = i;
                nearestPercentageSqrDistance = newSqrDistance;
            }
        }
        nextNearestPercentage = nearestPercentage;
        nextNearestPercentageSqrDistance = nearestPercentageSqrDistance;
        int numberOfRefinments = Mathf.RoundToInt(Mathf.Pow(pathLength * 10, 1.0f / 5.0f));
        for (int r = 0; r < numberOfRefinments; r++)
        {
            float refinedResolution = testResolution / testPoints;
            float startSearch = nearestPercentage - testResolution / 2;
            float endSearch = nearestPercentage + testResolution / 2;
            for (float i = startSearch; i < endSearch; i += refinedResolution)
            {
                Vector3 point = GetPathPosition(i);
                Vector3 difference = point - fromPostition;
                float newSqrDistance = Vector3.SqrMagnitude(difference);
                if (nearestPercentageSqrDistance > newSqrDistance)
                {
                    nextNearestPercentage = nearestPercentage;
                    nextNearestPercentageSqrDistance = nearestPercentageSqrDistance;

                    nearestPercentage = i;
                    nearestPercentageSqrDistance = newSqrDistance;
                }
                else
                {
                    if(nextNearestPercentageSqrDistance > newSqrDistance)
                    {
                        nextNearestPercentage = i;
                        nextNearestPercentageSqrDistance = newSqrDistance;
                    }
                }
            }
            testResolution = refinedResolution;
        }
        float lerpvalue = nearestPercentageSqrDistance / (nearestPercentageSqrDistance + nextNearestPercentageSqrDistance);
        return Mathf.Lerp(nearestPercentage, nextNearestPercentage, lerpvalue);
    }

    public void Clear()
    {
        for (int i = 0; i < realNumberOfPoints; i++)
        {
            DestroyImmediate(_points[i]);
        }
        _points.Clear();
    }


    public CameraPathControlPoint GetPoint(int index)
    {
        return this[GetPointIndex(index)];
    }

    public int GetPointIndex(int index)
    {
        if (_points.Count == 0)
            return -1;
        if (!_looped)
        {
            return Mathf.Clamp(index, 0, numberOfCurves);
        }
        if (index >= numberOfCurves)
            index = index - numberOfCurves;
        if (index < 0)
            index = index + numberOfCurves;

        return index;
    }

    public int GetCurveIndex(int startPointIndex)
    {
        if (_points.Count == 0)
            return -1;
        if (!_looped)
        {
            return Mathf.Clamp(startPointIndex, 0, numberOfCurves-1);
        }
        if (startPointIndex >= numberOfCurves - 1)
            startPointIndex = startPointIndex - numberOfCurves - 1;
        if (startPointIndex < 0)
            startPointIndex = startPointIndex + numberOfCurves - 1;

        return startPointIndex;
    }

    private void Init()
    {
        InitialiseLists();

        if(initialised)
            return;

        CameraPathControlPoint p0 = ScriptableObject.CreateInstance<CameraPathControlPoint>();
        CameraPathControlPoint p1 = ScriptableObject.CreateInstance<CameraPathControlPoint>();
        CameraPathControlPoint p2 = ScriptableObject.CreateInstance<CameraPathControlPoint>();
        CameraPathControlPoint p3 = ScriptableObject.CreateInstance<CameraPathControlPoint>();

        p0.baseTransform = transform;
        p1.baseTransform = transform;
        p2.baseTransform = transform;
        p3.baseTransform = transform;

        p0.localPosition = new Vector3(-20, 0, -20);
        p1.localPosition = new Vector3(20, 0, -20);
        p2.localPosition = new Vector3(20, 0, 20);
        p3.localPosition = new Vector3(-20, 0, 20);

        p0.forwardControlPoint = new Vector3(0, 0, -20);
        p1.forwardControlPoint = new Vector3(40, 0, -20);
        p2.forwardControlPoint = new Vector3(0, 0, 20);
        p3.forwardControlPoint = new Vector3(-40, 0, 20);

        AddPoint(p0);
        AddPoint(p1);
        AddPoint(p2);
        AddPoint(p3);

        initialised = true;
    }

    private void InitialiseLists()
    {
        if (_orientationList == null)
            _orientationList = ScriptableObject.CreateInstance<CameraPathOrientationList>();
        if (_fovList == null)
            _fovList = ScriptableObject.CreateInstance<CameraPathFOVList>();
        if (_tiltList == null)
            _tiltList = ScriptableObject.CreateInstance<CameraPathTiltList>();
        if (_speedList == null)
            _speedList = ScriptableObject.CreateInstance<CameraPathSpeedList>();
        if (_eventList == null)
            _eventList = ScriptableObject.CreateInstance<CameraPathEventList>();
        if (_delayList == null)
            _delayList = ScriptableObject.CreateInstance<CameraPathDelayList>();

        _orientationList.Init(this);
        _fovList.Init(this);
        _tiltList.Init(this);
        _speedList.Init(this);
        _eventList.Init(this);
        _delayList.Init(this);
    }

    private void UpdateDummyPoint()
    {
        if (_nextPath != null && _interpolateNextPath)
        {
            if (_dummyLastPoint == null)
                _dummyLastPoint = ScriptableObject.CreateInstance<CameraPathControlPoint>();
            _dummyLastPoint.name = "Last Point Dummy";
            _dummyLastPoint.baseTransform = transform;
            _dummyLastPoint.index = realNumberOfPoints + ((_looped) ? 2 : +1);
            _dummyLastPoint.percentage = 1;
            _dummyLastPoint.worldPosition = _nextPath[0].worldPosition;
            Vector3 positionDifference = _nextPath.transform.position - transform.position;
            _dummyLastPoint.forwardControlPoint = _nextPath[0].forwardControlPoint + positionDifference;
            _dummyLastPoint.backwardControlPoint = _nextPath[0].backwardControlPoint + positionDifference;
            _orientationList.SetDummyLastPoint(true);
        }
        else
        {
            if (_dummyLastPoint != null)
            {
                _orientationList.SetDummyLastPoint(false);
                DestroyImmediate(_dummyLastPoint);
            }
        }
    }
}
