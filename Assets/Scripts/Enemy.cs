using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    // Start is called before the first frame update
   public int maxHealth;
   public int curHealth;
   public Transform target;
   public bool isChase;

   Rigidbody rigid;
   BoxCollider boxCollider;
   Material mat;
   NavMeshAgent nav;
   Animator anim;

   void Awake(){
       rigid        = GetComponent<Rigidbody>();
       boxCollider  = GetComponent<BoxCollider>();
       mat          = GetComponentInChildren<MeshRenderer>().material;
       nav          = GetComponent<NavMeshAgent>();
       anim         = GetComponentInChildren<Animator>();

       Invoke("ChaseStart", 2);
   }
   void ChaseStart(){
       isChase = true;
       anim.SetBool("isWalk", true);
   }
   void Update(){
       if(isChase)
        nav.SetDestination(target.position);
   }
   void FixedUpdate() {
        FreezeVelocity();    
   }

   // 수류탄에 의한 피격
   public void HitByGrenade(Vector3 explosionPos){
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));
   }
    // 물리충돌 시 자동회전 방지
    void FreezeVelocity(){
        if(isChase){
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

   void OnTriggerEnter(Collider other) {
       // 근접공격 피격
       if(other.tag == "Melee"){
           Weapon weapon = other.GetComponent<Weapon>();
           curHealth -= weapon.damage;
           Vector3 reactVec = transform.position - other.transform.position;
           StartCoroutine(OnDamage(reactVec, false));
       }
       // 총알 피격
       else if(other.tag == "Bullet"){
           Bullet bullet = other.GetComponent<Bullet>();
           curHealth -= bullet.damage;
           Vector3 reactVec = transform.position - other.transform.position;
           Destroy(other.gameObject);
           StartCoroutine(OnDamage(reactVec, false));
       }
   }

   IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
   {
       mat.color = Color.red;
       yield return new WaitForSeconds(0.1f);

       // HP가 아직 남아있다면
       if(curHealth > 0){
           mat.color = Color.white;
       }
       // 죽었다면
       else{
           mat.color = Color.grey;
           gameObject.layer = 12;
           // 죽었으니 추적 네비 비활성화
           isChase = false;
           nav.enabled = false;
           anim.SetTrigger("doDie");


           // 수류탄일경우 좀 더 사망 액션 크게
           if(isGrenade){
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
           }
           // 그 외
           else{
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);              
           }

           Destroy(gameObject, 4);
       }
   }
}
