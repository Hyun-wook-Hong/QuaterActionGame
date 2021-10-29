using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;

    // 충돌로직
    private void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.tag == "Floor"){
            Destroy(gameObject, 3);
        }
        else if(collision.gameObject.tag == "Wall"){
            Destroy(gameObject);
        }

    }
}