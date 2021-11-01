using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // 1. 일반적인 경우의 패턴
    // Use() 메인루틴 -> Swing() 서브루틴 -> Use() 메인루틴 (교차실행)

    // 2. 코루틴 패턴
    // Use() 메인루틴 + Swing() 코루틴 (동시실행)

    public enum Type { Melee, Range };
    public Type type;
    // 데미지
    public int damage;
    // 공격속도
    public float speedOfAttack;
    // 총알 최대갯수
    public int maxAmmo;
    // 현재 총알갯수
    public int curAmmo;
    // 근접공격 범위
    public BoxCollider meleeArea;
    // 근접공격 이펙트
    public TrailRenderer trailEffect;
    // 총알 위치
    public Transform bulletPos;
    // 총알 오브젝트
    public GameObject bullet;
    // 탄피 위치
    public Transform bulletCasePos;
    // 탄피 오브젝트
    public GameObject bulletCase;


    // 플레이어 무기사용 이벤트
    public void Use(){

        // StopCoroutine - 코루틴 중지 (일반적인 함수 호출과 살짝 다름)
        // StartCoroutine - 코루틴 호출 (일반적인 함수 호출과 살짝 다름)

        // 근접무기 장착 시
        if(type == Type.Melee){
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        // 원거리 무기 장착 시
        if(type == Type.Range && curAmmo > 0){
            curAmmo--;
            StartCoroutine("Shot");
        }

    }
    
    // 근접무기 스윙 로직
    IEnumerator Swing(){
        // yield: 결과전달 키워드, 여러개 return하여 시간차 로직 작성 가능
        //yield break; // 코루틴 탈출 가능

        // 1
        yield return new WaitForSeconds(0.3f); // 0.1초 대기
        meleeArea.enabled   = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled   = false;

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;

    }

    // 원거리 무기
    IEnumerator Shot(){
        // #1. 총알 발사
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;

        yield return null;

        // #2. 탄피배출
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3) ;
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up);
    }
}