using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class ChangeType : MonoBehaviour
{

   
    public float wheelTranSpeed = 50.0f;
    public Material[] car02_Parts_Mat, car02_Interior_Mat, Car02_Body_Mat;
    public GameObject[] wheelObjs;
    public FindAllChild findAllChild;
    private UiMain uiMain;
    private int index;
   
    public GameObject wheelParent;
  //  public RuntimeAnimatorController ani;
    public GameObject cube;

    public GameObject[] emissives;
    public LightTest[] lightTest;
    public Material[] skyBoxs;
   
    public static ChangeType instence;
    public  CanvasGroup canvasGroup;
    private TransformOperation transformOperation;
    public Dictionary<int, MoveBtnMgr> moveMgr;

    public void Awake()
    {
        instence = this;
    }
    Coroutine coLight;
    Coroutine coRotate;
   
    void Start()
    {
        //texts = uiMain.texts;
              // canvasGroup = this.GetComponent<CanvasGroup>();
              uiMain = this.GetComponent<UiMain>();
        ChangeMaterial(index);
        transformOperation = findAllChild.gameObject.GetComponent<TransformOperation>( );
    }
    public void SetMoveMgr(int idx, MoveBtnMgr mgr)
    {
        if (moveMgr == null)
        {
            moveMgr = new Dictionary<int, MoveBtnMgr>();
        }
        moveMgr[idx] = mgr;
    }

    void Update()
    {
        if (uiMain.isClick)
        {

            if (moveMgr != null && (moveMgr[1].gameObject.activeInHierarchy || moveMgr[2].gameObject.activeInHierarchy || moveMgr[3].gameObject.activeInHierarchy))
            {
                //Debug.Log(66666666666666);
                index = moveMgr[1].gameObject.activeInHierarchy ? moveMgr[1].CurIndex : moveMgr[2].gameObject.activeInHierarchy ? moveMgr[2].CurIndex : moveMgr[3].gameObject.activeInHierarchy ? moveMgr[3].CurIndex : -1;
                if (index == -1)
                {
                    return;
                }
                
               
                
                switch (uiMain .CurPage )
                {
                    case 0:
                        ChangeMaterial(index);
                        break;
                    case 1:
                        CanvasGroup group = gameObject .AddComponent<CanvasGroup>();
                        group.blocksRaycasts = false;
                        transformOperation.enabled = false;
                        ChangeModelBtn(index);
                      
                        break;
                    case 2:
                        ChangeSkyBox(index, skyBoxs);
                        break;


                }
               
                uiMain.isClick = false;
            }


        }
    }




    public void ChangeModelBtn(int carModelIndex)
    {

        
     
        float rota = findAllChild.transform.localRotation.eulerAngles.y;
        //Debug.Log(rota);
        List<Vector3> trans = new List<Vector3>();
        for (int i = 0; i < 4; i++)
        {
            trans.Add(wheelParent.transform.GetChild(i).transform.position);

        }
        for (int i = 0; i < 4; i++)
        {

            Destroy(wheelParent.transform.GetChild(i).transform.gameObject);
            GameObject go = Instantiate(wheelObjs[carModelIndex], wheelParent.transform, false);

            go.transform.position = trans[i];
            //go.AddComponent<Animator>();
            //go.GetComponent<Animator>().runtimeAnimatorController = ani;
            go.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            if (i == 1 || i == 3)
            {
                Quaternion ro = Quaternion.AngleAxis(180, go.transform.up);
                go.transform.localRotation = ro;

                PlayAnimation(go, -0.5f, 1);
            }
            else
            {
                PlayAnimation(go, -0.5f, -1);
            }
        }

       
       

    }


    public void ChangeSkyBox(int index,Material [] skyBoxs)
    {
        RenderSettings.skybox = skyBoxs[index];

    }

    public void PlayAnimation(GameObject g, float offest, float dir)
    {
       // this.GetComponent<CanvasGroup>().blocksRaycasts = false;
        GameObject item = g.transform.GetChild(0).gameObject;
           // item.transform.DORestart();
           // findAllChild.gameObject.GetComponent<TestRotate>().DoKill();

            CloseLight();
            //  item.transform.DOLocalMoveY(0.1f,0.01f);
            item.transform.DOLocalMoveY(-offest / 2.0f, 0.01f);

            item.transform.DOLocalMoveX(offest, 0.01f).OnComplete(() =>
            {
                item.transform.DOLocalMoveX(0, 1.5f);
                item.transform.DOLocalMoveY(0, 1.5f).OnUpdate(() =>
                {
                    item.transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 1.5f);
                });
                item.transform.DOLocalRotate(new Vector3(360 * dir, 0, 0), 1.0f, RotateMode.LocalAxisAdd).OnComplete(() =>
                {
                    item.transform.DOLocalRotate(new Vector3(30 * dir, 0, 0), 0.25f, RotateMode.LocalAxisAdd).OnComplete(() =>
                    {
                        item.transform.DOLocalRotate(new Vector3(-30 * dir, 0, 0), 0.25f, RotateMode.LocalAxisAdd).OnComplete(() =>
                        {
                            item.transform.DOLocalRotate(new Vector3(15 * dir, 0, 0), 0.1f, RotateMode.LocalAxisAdd).OnComplete(() =>
                            {
                                item.transform.DOLocalRotate(new Vector3(-15 * dir, 0, 0), 0.1f, RotateMode.LocalAxisAdd).OnComplete(() =>
                                {
                                    for (int i = 0; i < emissives.Length; i++)
                                    {
                                        emissives[i].SetActive(true);
                                    }

                                    for (int i = 0; i < lightTest.Length; i++)
                                    {
                                        lightTest[i].enabled = true;

                                    }

                                    if (coLight == null)
                                    {
                                        coLight = StartCoroutine(WaitCloseLight());
                                    }
                                    else
                                    {
                                        StopCoroutine(coLight);
                                        coLight = StartCoroutine(WaitCloseLight());
                                    }
                                    if (coRotate == null)
                                    {
                                        coRotate = StartCoroutine(WaitRotate());
                                    }
                                    else
                                    {
                                        StopCoroutine(coRotate);
                                        coRotate = StartCoroutine(WaitRotate());
                                    }

                                });
                            });
                        });
                    });
                });
            });
           
      
      
        // item.transform.DOLocalMoveX(0, 1.5f);

    }

   
    public void ChangeMaterial(int carColorIndex)
    {

        for (int i = 0; i < findAllChild.objs.Count; i++)
        {
            if (findAllChild.objs[i] == null)
            {

            }
            else
            {
                if (findAllChild.objs[i].GetComponent<MeshRenderer>() != null && findAllChild.objs[i].GetComponent<MeshFilter>() != null)
                {
                    Material[] carMaterial = findAllChild.objs[i].GetComponent<MeshRenderer>().materials;
                    // Debug.Log(carMaterial.Length);
                    if (carMaterial.Length == 1)
                    {
                        string str = carMaterial[0].name;
                        //Debug.Log(str);
                        if (str.Length >= 19 && str.Substring(0, 19) == "Car02_Interior_Mat0")
                        {


                            if (carColorIndex == 4)
                            {

                                findAllChild.objs[i].GetComponent<MeshRenderer>().material = car02_Interior_Mat[0];

                            }
                            else
                            {
                                findAllChild.objs[i].GetComponent<MeshRenderer>().material = car02_Interior_Mat[1];
                            }
                        }
                    }
                    if (carMaterial.Length == 2)
                    {


                        for (int j = 0; j < carMaterial.Length; j++)
                        {
                            string str = carMaterial[j].name;
                            if (str.Substring(0, 15) == "Car02_Body_Mat0")
                            {
                                findAllChild.objs[i].GetComponent<MeshRenderer>().materials[j].CopyPropertiesFromMaterial(Car02_Body_Mat[carColorIndex]);
                            }

                        }

                        //  Debug.Log(str);

                    }
                    if (carMaterial.Length == 4)
                    {

                        for (int j = 0; j < carMaterial.Length; j++)
                        {
                            string str = carMaterial[j].name;
                            if (str.Substring(0, 15) == "Car02_Body_Mat0")
                            {

                                findAllChild.objs[i].GetComponent<MeshRenderer>().materials[j].CopyPropertiesFromMaterial(Car02_Body_Mat[carColorIndex]);
                            }

                            if (str.Substring(0, 19) == "Car02_Interior_Mat0")
                            {

                                if (carColorIndex == 4)
                                {


                                    findAllChild.objs[i].GetComponent<MeshRenderer>().materials[j].CopyPropertiesFromMaterial(car02_Interior_Mat[0]);


                                }
                                else
                                {
                                    findAllChild.objs[i].GetComponent<MeshRenderer>().materials[j].CopyPropertiesFromMaterial(car02_Interior_Mat[1]);


                                }
                            }
                            if (str.Substring(0, 16) == "Car02_Parts_Mat0")
                            {
                                if (carColorIndex == 5 || carColorIndex == 2 || carColorIndex == 1)
                                {


                                    findAllChild.objs[i].GetComponent<MeshRenderer>().materials[j].CopyPropertiesFromMaterial(car02_Parts_Mat[1]);


                                }
                                else
                                {
                                    findAllChild.objs[i].GetComponent<MeshRenderer>().materials[j].CopyPropertiesFromMaterial(car02_Parts_Mat[0]);


                                }
                            }



                        }
                    }
                }
            }

        }
  
      



    }
  

    IEnumerator WaitRotate()
    {
        yield return new WaitForSeconds(2.0f);
        findAllChild.gameObject.GetComponent<TestRotate>().OnMove();

    }
    IEnumerator WaitCloseLight()
    {
        yield return new WaitForSeconds(6.0f);

        CloseLight();
        Destroy(gameObject.GetComponent<CanvasGroup>());
        transformOperation.enabled = true ;
    }
    void CloseLight()
    {
        for (int i = 0; i < lightTest.Length; i++)
        {
            lightTest[i].enabled = false;

        }
        for (int i = 0; i < emissives.Length; i++)
        {
            emissives[i].SetActive(false);
        }
       
    }


}
