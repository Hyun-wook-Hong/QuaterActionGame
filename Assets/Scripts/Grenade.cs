using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject meshObj;
    public GameObject effectObj;
    public Rigidbody rigid;

    // Start is called before the first frame update
    void Start()
    {
        // 3초뒤
        StartCoroutine(Explosion());

    }

    IEnumerator Explosion(){
        yield return new WaitForSeconds(3.0f);
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        meshObj.SetActive(false);
        effectObj.SetActive(true);

        // 적들이 범위에 있을 시 피격 이벤트 생성
        RaycastHit[] rayHits = 
            Physics.SphereCastAll(transform.position, 
                                  15, 
                                  Vector3.up, 0f,
                                  LayerMask.GetMask("Enemy"));
        foreach(RaycastHit hitObj in rayHits){
            hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }
        
        Destroy(gameObject, 5);
    }
}
