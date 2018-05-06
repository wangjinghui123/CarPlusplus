using UnityEngine;
using System.Collections;

public class gesture : MonoBehaviour
{
    public Transform Cube;
    public Transform  RotObj;
    private float radius = 1080;
    private Vector3 originalDir = new Vector3(0f, 0f, 1080f);
    private Vector3 CenterPos = new Vector3(0, 0, 0);
    private Vector2 startPos;
    private Vector2 tempPos;
    private Vector3 tempVec;
    private Vector3 normalAxis;
    private float angle;
    // Use this for initialization
    void Start()
    {
        Cube = GameObject.Find("Cube").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 1)
        {
            //Vector2 startPos = Input.compositionCursorPos;
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                startPos = Input.GetTouch(0).position;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                tempPos = Event.current.mousePosition;

                float tempX = tempPos.x - startPos.x;

                float tempY = tempPos.y - startPos.y;

                float tempZ = Mathf.Sqrt(radius * radius - tempX * tempX - tempY * tempY);

                tempVec = new Vector3(tempX, tempY, tempZ);

                angle = Mathf.Acos(Vector3.Dot(originalDir.normalized, tempVec.normalized)) * Mathf.Rad2Deg;

                normalAxis = getNormal(CenterPos, originalDir, tempVec);

                RotObj.rotation = Quaternion.AngleAxis(2 * angle, normalAxis);

            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                Cube.transform.parent = null;
                RotObj.rotation = Quaternion.identity;
                Cube.parent = RotObj;
            }
        }
    }

    void OnGUI()
    {
        GUILayout.Label("StartPos 的坐标值为： " + startPos);
        GUILayout.Label("tempPos 的坐标值为： " + tempPos);
        GUILayout.Label("tempVec 的坐标值为： " + tempVec);
        GUILayout.Label("normalAxis 的坐标值为： " + normalAxis);
        GUILayout.Label("旋转角度的值为： " + 2 * angle);
        GUILayout.Label("Cube的四元数角度： " + Cube.rotation);
        GUILayout.Label("Cube de rotation.x： " + Cube.rotation.eulerAngles.x);
        GUILayout.Label("Cube de rotation.y： " + Cube.rotation.eulerAngles.y);
        GUILayout.Label("Cube de rotation.z： " + Cube.rotation.eulerAngles.z);
    }

    private Vector3 getNormal(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float a = ((p2.y - p1.y) * (p3.z - p1.z) - (p2.z - p1.z) * (p3.y - p1.y));

        float b = ((p2.z - p1.z) * (p3.x - p1.x) - (p2.x - p1.x) * (p3.z - p1.z));

        float c = ((p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x));
        //a对应的屏幕的垂直方向，b对应的屏幕的水平方向。
        return new Vector3(a, -b, c);
    }
}