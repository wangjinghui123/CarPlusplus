using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TestRotate : MonoBehaviour
{
    Tweener tweener;
    private void Start()
    {
        tweener = transform.DOLocalRotate(new Vector3(0, 360, 0), 2f, RotateMode.LocalAxisAdd);
        tweener.SetAutoKill(false);
        tweener.Pause();
    }
    public void DoKill()
    {
        tweener.Restart();
        tweener.Pause();
    }

    public void OnMove()
    {
        transform.DORestart();

    }

}
