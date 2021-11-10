using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D };
    public Type enemyType;
    // Start is called before the first frame update
   public int maxHealth;
   public int curHealth;
   public Transform target;
   public BoxCollider meleeArea;
   public GameObject bullet;
   public bool isChase;
   public bool isAttack;
   public bool isDead;

   public Rigidbody rigid;
   public BoxCollider boxCollider;
   public MeshRenderer[] meshes;
   public NavMeshAgent nav;
   public Animator anim;

   void Awake(){
       rigid        = GetComponent<Rigidbody>();
       boxCollider  = GetComponent<BoxCollider>();
       meshes          = GetComponentsInChildren<MeshRenderer>();
       nav          = GetComponent<NavMeshAgent>();
       anim         = GetComponentInChildren<Animator>();

       if(enemyType != Type.D)
        Invoke("ChaseStart", 2);
   }
   void ChaseStart(){
       isChase = true;
       anim.SetBool("isWalk", true);
   }
   void Update(){
       if(nav.enabled && enemyType != Type.D){
        nav.SetDestination(target.position);
        nav.isStopped = !isChase;           
       }
   }

   void Targeting(){
       if(!isDead && enemyType != Type.D)
       {
            float targetRadius = 0;
            float targetRange  = 0;
            switch(enemyType){
                case Type.A:
                        targetRadius   = 1.5f;
                        targetRange    = 3.0f;
                    break;
                case Type.B:
                        targetRadius   = 1f;
                        targetRange    = 12f;
                    break;
                case Type.C:
                        targetRadius   = 0.5f;
                        targetRange    = 25f;
                    break;
            }

                // 플레이어가 범위에 있을 시 피격 이벤트 생성
                RaycastHit[] rayHits = 
                    Physics.SphereCastAll(transform.position, 
                                        targetRadius, 
                                        transform.forward, 
                                        targetRange,
                                        LayerMask.GetMask("Player"));

                if(rayHits.Length > 0 && !isAttack){
                    StartCoroutine(Attack());
                }
       }

   }

    IEnumerator Attack(){
        // 그 자리에서 정지 후 공격 시작
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch(enemyType){
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;
                
                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                // 잠시대기 후 돌진
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;
            case Type.C:
                // 미사일 생성 후 공격
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);
                break;
        }


        // 공격 끝나면 다시 원래대로
        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);

    }

   void FixedUpdate() {
        Targeting();
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
       foreach(MeshRenderer mesh in meshes){
           mesh.material.color = Color.red;
       }
       //mat.color = Color.red;
       yield return new WaitForSeconds(0.1f);

       // HP가 아직 남아있다면
       if(curHealth > 0){
           //mat.color = Color.red;
           foreach(MeshRenderer mesh in meshes){
            mesh.material.color = Color.red;
           }
       }
       // 죽었다면
       else{
           foreach(MeshRenderer mesh in meshes){
                mesh.material.color = Color.grey;
           }
           gameObject.layer = 12;
           // 죽었으니 isdead true, 추적 네비 비활성화
           isDead = true;
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
           if(enemyType != Type.D)
                Destroy(gameObject, 4);
       }
   }
}
