using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour {
    public    Transform  camera;
  
	// Use this for initialization
	void Start () {
      
	}
	
	// Update is called once per frame
	void Update () {
        LookCamera();

    }
    public void LookCamera()
    {
        transform.LookAt(camera);

    }
}
