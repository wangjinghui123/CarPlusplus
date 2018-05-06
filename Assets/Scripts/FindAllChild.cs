using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FindAllChild : MonoBehaviour
{

    private  Transform[] grandFa;
    // public GameObject[] objs;
    public List<GameObject> objs;
    public  bool isGet = false;
    // Use this for initialization  
    void Start()
    {


        
        grandFa = GetComponentsInChildren<Transform>();

        //foreach (Transform child in grandFa)
        //{
        //   // print(child.name);
        //}

        FindChild();


    }

    public void FindChild()
    {
        objs.Clear();
        foreach (Transform child in grandFa)
        {
            objs.Add(child.gameObject);
        }
        isGet = true;
    }
}