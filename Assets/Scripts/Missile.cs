using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // 날아가며 회전
        transform.Rotate(Vector3.right * 30 * Time.deltaTime);
    }
}
