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

/// <summary>
/// This is the main script in the Third Person Camera exmaple
/// It creates a God of War style camera that follows the player while sticking to the path
/// </summary>
public class PlayerPathFollow : MonoBehaviour
{
    [SerializeField]
    private Transform player;

    [SerializeField]
    private Transform camera;

    [SerializeField]
    private CameraPath path;

    private float lastPercent = 0;

    //Set the initial position of the camera so we don't jump at the start of the demo
    void Start()
    {
        float nearestPercent = path.GetNearestPoint(player.position);
        lastPercent = nearestPercent;

        Vector3 nearestPoint = path.GetPathPosition(nearestPercent, true) + path.transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(player.position - camera.position);

        camera.position = nearestPoint;
        camera.rotation = lookRotation;
    }

    //Update the camera animation 
    void LateUpdate()
    {
        float nearestPercent = path.GetNearestPoint(player.position);
        float usePercent = Mathf.Lerp(lastPercent, nearestPercent, 0.4f);
        lastPercent = usePercent;
        Vector3 nearestPoint = path.GetPathPosition(usePercent, true) + path.transform.position;

        camera.position = Vector3.Lerp(camera.position, nearestPoint, 0.4f);

        Quaternion lookRotation = Quaternion.LookRotation(player.position - camera.position);

        camera.rotation = Quaternion.Slerp(camera.rotation, lookRotation, 0.4f);

    }
}
