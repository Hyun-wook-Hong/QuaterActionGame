using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called before the first frame update
   public int maxHealth;
   public int curHealth;

   Rigidbody rigid;
   BoxCollider boxCollider;
   Material mat;

   void Awake(){
       rigid = GetComponent<Rigidbody>();
       boxCollider = GetComponent<BoxCollider>();
       mat = GetComponent<MeshRenderer>().material;
   }

   void OnTriggerEnter(Collider other) {
       // 근접공격 피격
       if(other.tag == "Melee"){
           Weapon weapon = other.GetComponent<Weapon>();
           curHealth -= weapon.damage;
           Vector3 reactVec = transform.position - other.transform.position;
           StartCoroutine(OnDamage(reactVec));
       }
       // 총알 피격
       else if(other.tag == "Bullet"){
           Bullet bullet = other.GetComponent<Bullet>();
           curHealth -= bullet.damage;
           Vector3 reactVec = transform.position - other.transform.position;
           Destroy(other.gameObject);
           StartCoroutine(OnDamage(reactVec));
       }
   }

   IEnumerator OnDamage(Vector3 reactVec)
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

           reactVec = reactVec.normalized;
           reactVec += Vector3.up;
           rigid.AddForce(reactVec * 5, ForceMode.Impulse);

           Destroy(gameObject, 4);
       }
   }
}
