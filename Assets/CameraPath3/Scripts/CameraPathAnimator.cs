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

public class CameraPathAnimator : MonoBehaviour
{
    public enum animationModes
    {
        once,
        loop,
        reverse,
        reverseLoop,
        pingPong
    }

    public enum orientationModes
    {
        custom,//rotations will be decided by defining orientations along the curve
        target,//camera will always face a defined transform
        mouselook,//camera will have a mouse free look
        followpath,//camera will use the path to determine where to face - maintaining world up as up
        reverseFollowpath,//camera will use the path to determine where to face, looking back on the path
        followTransform//move the object to the nearest point on the path and look at target
    }

    public Transform orientationTarget;
    [SerializeField]
    private CameraPath _cameraPath;
    //do you want this path to automatically animate at the start of your scene
    public bool playOnStart = true;
    //the actual transform you want to animate
    public Transform animationObject = null;
    //a link to the camera component
    private Camera animationObjectCamera = null;
    //is the transform you are animating a camera?
    private bool _isCamera = true;
    private bool playing = false;
    public animationModes animationMode = animationModes.once;
    public orientationModes orientationMode = orientationModes.custom;
    private float pingPongDirection = 1;

    public bool normalised = true;

    //the time used in the editor to preview the path animation
    public float editorPercentage = 0;
    //the time the path animation should last for
    [SerializeField]
    private float _pathTime = 10;
    //the time the path animation should last for
    [SerializeField]
    private float _pathSpeed = 10;
    private float _percentage = 0;
//    private float usePercentage;
    private int atPointNumber = 0;
    public float nearestOffset = 0;
    private float delayTime = 0;

    //the sensitivity of the mouse in mouselook
    public float sensitivity = 5.0f;
    //the minimum the mouse can move down
    public float minX = -90.0f;
    //the maximum the mouse can move up
    public float maxX = 90.0f;
    private float rotationX = 0;
    private float rotationY = 0;


    public bool showPreview = true;
    public GameObject editorPreview = null;
    public bool showScenePreview = true;
    public bool animateSceneObjectInEditor = false;

    public Vector3 animatedObjectStartPosition;
    public Quaternion animatedObjectStartRotation;

    //Events
    public delegate void AnimationStartedEventHandler();
    public delegate void AnimationPausedEventHandler();
    public delegate void AnimationStoppedEventHandler();
    public delegate void AnimationFinishedEventHandler();
    public delegate void AnimationLoopedEventHandler();
    public delegate void AnimationPingPongEventHandler();
    public delegate void AnimationPointReachedEventHandler();
    public delegate void AnimationCustomEventHandler(string eventName);

    public delegate void AnimationPointReachedWithNumberEventHandler(int pointNumber);

    public event AnimationStartedEventHandler AnimationStartedEvent;
    public event AnimationPausedEventHandler AnimationPausedEvent;
    public event AnimationStoppedEventHandler AnimationStoppedEvent;
    public event AnimationFinishedEventHandler AnimationFinishedEvent;
    public event AnimationLoopedEventHandler AnimationLoopedEvent;
    public event AnimationPingPongEventHandler AnimationPingPongEvent;
    public event AnimationPointReachedEventHandler AnimationPointReachedEvent;
    public event AnimationPointReachedWithNumberEventHandler AnimationPointReachedWithNumberEvent;
    public event AnimationCustomEventHandler AnimationCustomEvent;

    //PUBLIC METHODS

    //Script based controls - hook up your scripts to these to control your

    /// <summary>
    /// Gets or sets the path speed.
    /// </summary>
    /// <value>
    /// The path speed.
    /// </value>
    public float pathSpeed
    {
        get
        {
            return _pathSpeed;// _cameraPath.pathLength / pathTime;
        }
        set
        {
            _pathSpeed = Mathf.Max(value, 0.001f);
//            float newPathSpeed = value;
//            pathTime = _cameraPath.pathLength / Mathf.Max(newPathSpeed, 0.000001f);
        }
    }

    /// <summary>
    /// Retreive the current time of the path animation
    /// </summary>
    public float currentTime
    {
        get { return _pathTime * _percentage; }
    }

    /// <summary>
    /// Play the path. If path has finished do not play it.
    /// </summary>
    public void Play()
    {
        playing = true;
        if (!isReversed)
        {
            if(_percentage == 0)
            {
                if (AnimationStartedEvent != null) AnimationStartedEvent();
                cameraPath.eventList.OnAnimationStart(0);
            }
        }
        else
        {
            if(_percentage == 1)
            {
                if (AnimationStartedEvent != null) AnimationStartedEvent();
                cameraPath.eventList.OnAnimationStart(1);
            }
        }
    }

    /// <summary>
    /// Stop and reset the animation back to the beginning
    /// </summary>
    public void Stop()
    {
        playing = false;
        _percentage = 0;
        if (AnimationStoppedEvent != null) AnimationStoppedEvent();
    }

    /// <summary>
    /// Pause the animation where it is
    /// </summary>
    public void Pause()
    {
        playing = false;
        if (AnimationPausedEvent != null) AnimationPausedEvent();
    }

    /// <summary>
    /// set the time of the animtion
    /// </summary>
    /// <param name="value">Seek Percent 0-1</param>
    public void Seek(float value)
    {
        _percentage = Mathf.Clamp01(value);
        //thanks kelnishi!
        UpdateAnimationTime(false);
        UpdatePointReached();
        bool p = playing;
        playing = true;
        UpdateAnimation();
        playing = p;
    }

    /// <summary>
    /// Is the animation playing
    /// </summary>
    public bool isPlaying
    {
        get { return playing; }
    }

    /// <summary>
    /// Current percent of animation
    /// </summary>
    public float percentage
    {
        get { return _percentage; }
    }

    /// <summary>
    /// Is the animation ping pong direction forward
    /// </summary>
    public bool pingPongGoingForward
    {
        get { return pingPongDirection == 1; }
    }

    /// <summary>
    /// Reverse the animation
    /// </summary>
    public void Reverse()
    {
        switch (animationMode)
        {
            case animationModes.once:
                animationMode = animationModes.reverse;
                break;
            case animationModes.reverse:
                animationMode = animationModes.once;
                break;
            case animationModes.pingPong:
                pingPongDirection = pingPongDirection == -1 ? 1 : -1;
                break;
            case animationModes.loop:
                animationMode = animationModes.reverseLoop;
                break;
            case animationModes.reverseLoop:
                animationMode = animationModes.loop;
                break;
        }
    }

    /// <summary>
    /// A link to the Camera Path component
    /// </summary>
    public CameraPath cameraPath
    {
        get
        {
            if (!_cameraPath)
                _cameraPath = GetComponent<CameraPath>();
            return _cameraPath;
        }
    }

    /// <summary>
    /// Retrieve the animation orientation at a percent based on the animation mode
    /// </summary>
    /// <param name="percent">Path Percent 0-1</param>
    /// <returns>Rotation</returns>
    public Quaternion GetAnimatedOrientation(float percent)
    {
        Quaternion output = Quaternion.identity;
        Vector3 currentPosition, forward;
        switch (orientationMode)
        {
            case orientationModes.custom:
                output = cameraPath.GetPathRotation(percent);
                break;

            case orientationModes.target:
                currentPosition = cameraPath.GetPathPosition(percent);
                if(orientationTarget != null)
                    forward = orientationTarget.transform.position - currentPosition;
                else
                    forward = Vector3.forward;
                output = Quaternion.LookRotation(forward);
                break;

            case orientationModes.followpath:
                output = Quaternion.LookRotation(cameraPath.GetPathDirection(percent));
                output *= Quaternion.Euler(transform.forward * -cameraPath.GetPathTilt(percent));
                break;

            case orientationModes.reverseFollowpath:
                output = Quaternion.LookRotation(-cameraPath.GetPathDirection(percent));
                output *= Quaternion.Euler(transform.forward * -cameraPath.GetPathTilt(percent));
                break;

            case orientationModes.mouselook:
                if(!Application.isPlaying)
                {
                    output = Quaternion.LookRotation(cameraPath.GetPathDirection(percent));
                    output *= Quaternion.Euler(transform.forward * -cameraPath.GetPathTilt(percent));
                }
                else
                {
                    output = GetMouseLook();
                }
                break;

            case orientationModes.followTransform:
                if(orientationTarget == null)
                    return Quaternion.identity;
                float nearestPerc = cameraPath.GetNearestPoint(orientationTarget.position);
                nearestPerc = Mathf.Clamp01(nearestPerc + nearestOffset);
                currentPosition = cameraPath.GetPathPosition(nearestPerc);
                forward = orientationTarget.transform.position - currentPosition;
                output = Quaternion.LookRotation(forward);
                break;
        }
        return output;
    }

    //MONOBEHAVIOURS

    private void Awake()
    {
        if(animationObject == null)
            _isCamera = false;
        else
        {
            animationObjectCamera = animationObject.GetComponentInChildren<Camera>();
            _isCamera = animationObjectCamera != null;
        }

        Camera[] cams = Camera.allCameras;
        if (cams.Length == 0)
        {
            Debug.LogWarning("Warning: There are no cameras in the scene");
            _isCamera = false;
        }

        if (!isReversed)
        {
            _percentage = 0;
            atPointNumber = -1;
        }
        else
        {
            _percentage = 1;
            atPointNumber = cameraPath.numberOfPoints - 1;
        }

        Vector3 initalRotation = cameraPath.GetPathRotation(0).eulerAngles;
        rotationX = initalRotation.y;
        rotationY = initalRotation.x;
    }

    private void OnEnable()
    {
        cameraPath.eventList.CameraPathEventPoint += OnCustomEvent;
        cameraPath.delayList.CameraPathDelayEvent += OnDelayEvent;
    }

    private void Start()
    {
        if (playOnStart)
            Play();

        if(Application.isPlaying && orientationTarget==null && (orientationMode==orientationModes.followTransform || orientationMode == orientationModes.target))
            Debug.LogWarning("There has not been an orientation target specified in the Animation component of Camera Path.",transform);
    }

    private void Update()
    {
        if (!isCamera)
        {
            if (playing)
            {
                UpdateAnimationTime();
                UpdateAnimation();
                UpdatePointReached();
            }
            else
            {
                if (_cameraPath.nextPath != null && _percentage >= 1)
                {
                    PlayNextAnimation();
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (isCamera)
        {
            if (playing)
            {
                UpdateAnimationTime();
                UpdateAnimation();
                UpdatePointReached();
            }
            else
            {
                if (_cameraPath.nextPath != null && _percentage >= 1)
                {
                    PlayNextAnimation();
                }
            }
        }
    }

    private void OnDisable()
    {
        CleanUp();
    }

    private void OnDestroy()
    {
        CleanUp();
    }

    //PRIVATE METHODS

    private void PlayNextAnimation()
    {
        if (_cameraPath.nextPath != null)
        {
            _cameraPath.nextPath.GetComponent<CameraPathAnimator>().Play();
            _percentage = 0;
            Stop();
        }
    }

    void UpdateAnimation()
    {
        if (animationObject == null)
        {
            Debug.LogError("There is no animation object specified in the Camera Path Animator component. Nothing to animate.\nYou can find this component in the main camera path game object called "+gameObject.name+".");
            Stop();
            return;
        }

        if (!playing)
            return;

        float minSpeed = 0.5f;
        if(cameraPath.speedList.enabled)
            _pathTime = _cameraPath.pathLength / Mathf.Max(cameraPath.GetPathSpeed(_percentage), minSpeed);
        else
            _pathTime = _cameraPath.pathLength / Mathf.Max(_pathSpeed * cameraPath.GetPathEase(_percentage), minSpeed);

        //TODO: Use stored positions
        animationObject.position = cameraPath.GetPathPosition(_percentage) + cameraPath.transform.position;
        animationObject.rotation = GetAnimatedOrientation(_percentage);

        if(isCamera)
            animationObjectCamera.fieldOfView = _cameraPath.GetPathFOV(_percentage);

        CheckEvents();
    }

    private void UpdatePointReached()
    {
        int currentPointNumber = cameraPath.GetPointNumber(_percentage);

        if (currentPointNumber != atPointNumber)
        {
            //we've hit a point
            if (AnimationPointReachedEvent != null) AnimationPointReachedEvent();
            if (!isReversed)
                if (AnimationPointReachedWithNumberEvent != null) AnimationPointReachedWithNumberEvent(currentPointNumber);
                else
                    if (AnimationPointReachedWithNumberEvent != null) AnimationPointReachedWithNumberEvent(atPointNumber);
        }

        atPointNumber = currentPointNumber;
    }

    private void UpdateAnimationTime()
    {
        UpdateAnimationTime(true);
    }

    private void UpdateAnimationTime(bool advance)
    {
        if(orientationMode == orientationModes.followTransform)
            return;

        if(delayTime > 0)
        {
            delayTime += -Time.deltaTime;
            return;
        }

        if(advance)
        {
            switch(animationMode)
            {

                case animationModes.once:
                    if(_percentage >= 1)
                    {
                        playing = false;
                        if(AnimationPointReachedEvent != null) AnimationPointReachedEvent();
                        if(AnimationPointReachedWithNumberEvent != null) AnimationPointReachedWithNumberEvent(cameraPath.numberOfPoints - 1);
                        if(AnimationFinishedEvent != null) AnimationFinishedEvent();
                    }
                    else
                    {
                        _percentage += Time.deltaTime * (1.0f / _pathTime);
                    }
                    break;

                case animationModes.loop:
                    if(_percentage >= 1)
                    {
                        _percentage = 0;
                        if(AnimationPointReachedEvent != null) AnimationPointReachedEvent();
                        if(AnimationPointReachedWithNumberEvent != null) AnimationPointReachedWithNumberEvent(cameraPath.numberOfPoints - 1);
                        if(AnimationLoopedEvent != null) AnimationLoopedEvent();
                    }
                    _percentage += Time.deltaTime * (1.0f / _pathTime);
                    break;

                case animationModes.reverseLoop:
                    if(_percentage <= 0)
                    {
                        _percentage = 1;
                        if(AnimationPointReachedEvent != null) AnimationPointReachedEvent();
                        if(AnimationPointReachedWithNumberEvent != null) AnimationPointReachedWithNumberEvent(0);
                        if(AnimationLoopedEvent != null) AnimationLoopedEvent();
                    }
                    _percentage += -Time.deltaTime * (1.0f / _pathTime);
                    break;

                case animationModes.reverse:
                    if(_percentage <= 0)
                    {
                        playing = false;
                        if(AnimationPointReachedEvent != null) AnimationPointReachedEvent();
                        if(AnimationPointReachedWithNumberEvent != null) AnimationPointReachedWithNumberEvent(0);
                        if(AnimationFinishedEvent != null) AnimationFinishedEvent();
                    }
                    else
                    {
                        _percentage += -Time.deltaTime * (1.0f / _pathTime);
                    }
                    break;

                case animationModes.pingPong:
                    _percentage += Time.deltaTime * (1.0f / _pathTime) * pingPongDirection;
                    if(_percentage >= 1)
                    {
                        _percentage = 0.99f;
                        pingPongDirection = -1;
                        if(AnimationPointReachedEvent != null) AnimationPointReachedEvent();
                        if(AnimationPointReachedWithNumberEvent != null) AnimationPointReachedWithNumberEvent(cameraPath.numberOfPoints - 1);
                        if(AnimationPingPongEvent != null) AnimationPingPongEvent();
                    }
                    if(_percentage <= 0)
                    {
                        _percentage = 0.01f;
                        pingPongDirection = 1;
                        if(AnimationPointReachedEvent != null) AnimationPointReachedEvent();
                        if(AnimationPointReachedWithNumberEvent != null) AnimationPointReachedWithNumberEvent(0);
                        if(AnimationPingPongEvent != null) AnimationPingPongEvent();
                    }
                    break;
            }
        }
        _percentage = Mathf.Clamp01(_percentage);
    }

    private Quaternion GetMouseLook()
    {
        if (animationObject == null)
            return Quaternion.identity;
        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY += -Input.GetAxis("Mouse Y") * sensitivity;

        rotationY = Mathf.Clamp(rotationY, minX, maxX);

        return Quaternion.Euler(new Vector3(rotationY, rotationX, 0));
    }

    private void CheckEvents()
    {
        cameraPath.CheckEvents(_percentage);
    }

    private bool isReversed
    {
        get { return (animationMode == animationModes.reverse || animationMode == animationModes.reverseLoop || pingPongDirection < 0); }
    }

    public bool isCamera
    {
        get
        {
            if (animationObject == null)
                _isCamera = false;
            else
            {
                animationObjectCamera = animationObject.GetComponentInChildren<Camera>();
                _isCamera = animationObjectCamera != null;
            }
            return _isCamera;
        }
    }

    private void CleanUp()
    {
        cameraPath.eventList.CameraPathEventPoint += OnCustomEvent;
        cameraPath.delayList.CameraPathDelayEvent += OnDelayEvent;
    }

    private void OnDelayEvent(float time)
    {
        if(time > 0)
            delayTime = time;//start delay timer
        else
            Pause();//indeffinite delay
    }

    private void OnCustomEvent(string eventName)
    {
        if(AnimationCustomEvent != null)
            AnimationCustomEvent(eventName);
    }
}
