using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    // 카메라는 플레이어를 따라간다
    void Update(){
        transform.position = target.position + offset;
    }
}
