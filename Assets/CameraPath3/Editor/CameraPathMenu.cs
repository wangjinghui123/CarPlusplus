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

public class CameraPathMenu : EditorWindow
{
    [MenuItem("GameObject/Create New Camera Path", false, 0)]
    public static void CreateNewCameraPath()
    {
        GameObject newCameraPath = new GameObject("New Camera Path");
        Undo.RegisterCreatedObjectUndo(newCameraPath, "Created New Camera Path");
        newCameraPath.AddComponent<CameraPath>();
        CameraPathAnimator animator = newCameraPath.AddComponent<CameraPathAnimator>();
        if(Camera.main != null)
            animator.animationObject = Camera.main.transform;
        Selection.objects = new Object[] { newCameraPath };
        SceneView.lastActiveSceneView.FrameSelected();
    }
}