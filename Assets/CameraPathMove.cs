using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPathMove : MonoBehaviour {


    private CameraPath cameraPath;
    private CameraPathAnimator cameraPathAni;
    public Transform middlePoint;
	// Use this for initialization
	void Start () {
        cameraPath = gameObject.GetComponent<CameraPath >();
        cameraPathAni  = gameObject.GetComponent<CameraPathAnimator>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PathPoint(Transform  point,int  index )
    {

        cameraPath.GetPoint(index).worldPosition = point.position;
        cameraPath.orientationList[index].rotation  = point.rotation ;
    
    }

    public void SetPoint(Transform start,Transform end)
    {
        cameraPathAni.Stop();
        PathPoint(start ,0);
        PathPoint(middlePoint, 1);
        PathPoint(end ,2);
        cameraPathAni.Play();
       // Debug.Log("执行动画次数");
    }

   
}
