
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;
public class CarIconClick : MonoBehaviour
{
    public Toggle toggleLight;
    public Toggle toggleDoor;
    public GameObject[] emissives;
    public GameObject door;//车门
    public GameObject camera;
    public GameObject cameraTarget;//车内移动目标点
    public GameObject car;
    public RectTransform doorIcon;
    private SecondCameraRotate secondCameraRotate;
    public GameObject cameraInitPos;//摄像机初始位置
    public GameObject uiMain;
    public GameObject cameraPath;
    private TransformOperation transformOperation;
    RectTransform rect;
    Tweener tweenner;
    public Image[] icon;
    void Start()
    {
        transformOperation = car.GetComponent<TransformOperation >();
        secondCameraRotate = camera.GetComponent<SecondCameraRotate>();
        secondCameraRotate.enabled = false;
        EventTriggerListener.Get(toggleLight.gameObject).onClick = OnButtonClick;
        EventTriggerListener.Get(toggleDoor.gameObject).onClick = OnButtonClick;
    }

    private void OnButtonClick(GameObject go)
    {
        rect = go.GetComponent<RectTransform>();
        rect.DOScale(new Vector3(0.008f, 0.008f, 0.008f), 0.2f).OnComplete(() =>
        {
            rect.DOScale(new Vector3(0.007f, 0.007f, 0.007f), 0.2f);
        });
        //在这里监听按钮的点击事件
        if (go == toggleLight.gameObject)
        {
            // Debug.Log("DoSomeThings");
            LightOn(toggleLight.isOn);

        }
        if (go == toggleDoor.gameObject)
        {
            DoorOpenOrClose(door, camera, cameraTarget, cameraInitPos, toggleDoor.isOn);
        }


    }
    public void LightOn(bool isOn)
    {
        for (int i = 0; i < emissives.Length; i++)
        {
            emissives[i].SetActive(isOn);
        }
    }



    public void DoorOpenOrClose(GameObject dorObj, GameObject camera, GameObject cameraE, GameObject cameraS, bool isopen)
    {
        transformOperation.enabled = !isopen;
        for (int i = 0; i < icon.Length; i++)
        {
            icon[i].raycastTarget = false;
        }
        dorObj.transform.DOLocalRotate(new Vector3(0, 60, 0), 1.0f, RotateMode.LocalAxisAdd).OnComplete(() =>
        {
            // GameObject obj = isopen ? cameraE : cameraS;
            GameObject temp;
           
            camera.transform.DOScaleX (1.0f, 2.0f).OnStart (() =>
            {
                if (isopen)
                {
                    cameraPath.GetComponent<CameraPathMove>().SetPoint(cameraS.transform, cameraE.transform);
                }
                else
                {

                     cameraPath.GetComponent<CameraPathMove>().SetPoint(cameraE.transform,cameraS.transform);
                 
                    print(cameraPath.GetComponent<CameraPathAnimator>().isPlaying);
                }
               
                
                //Vector3 ro = new Vector3(obj.transform.rotation.eulerAngles.x, obj.transform.rotation.eulerAngles.y, obj.transform.rotation.eulerAngles.z);
                //camera.transform.DORotate(ro, 2.0f);

            }).OnComplete(() =>
            {
                uiMain.SetActive(!isopen);
                secondCameraRotate.enabled = isopen;
                float moveValue = isopen ? 0.4f : -1.2f;
                tweenner = doorIcon.DOLocalMoveX(moveValue, 0.5f);
                dorObj.transform.DOLocalRotate(new Vector3(0, -60, 0), 1.0f, RotateMode.LocalAxisAdd).OnComplete(()=> {
                    for (int i = 0; i < icon.Length; i++)
                    {
                        icon[i].raycastTarget = true ;
                    }
                });
            });

        });
    }


  

}