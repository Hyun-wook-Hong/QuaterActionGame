using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player script.

public class Player : MonoBehaviour
{
    public float speed;
    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;

    bool isJump;
    bool isDodge;

    Rigidbody rigid;
    Vector3 moveVec;
    Vector3 dodgeVec;


    Animator anim;

    // Start is called before the first frame update
    // 플레이어가 생성되자마자 초기화
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // 키보드로 부터 입력
        GetInput();
        // 상하좌우 대각선 이동
        Move();
        // 플레이어가 보는 방향으로 회전
        Turn();
        // 점프기능
        Jump();
        // 회피기능
        Dodge();
    }

    void GetInput(){
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
    }

    void Move(){
                // normalized: 방향값이 1로 보정된 벡터
        // 한번 이동하는데 가로1, 세로1씩 이동한다 치면
        // 대각선은 v2(2제곱근)만큼 움직이게 되는 것이므로,
        // 이를 피타고라스 정리에 따라 구해지는 대각선 방향 이동 속도가 좀 더 빠른것은 
        // 형평성에 맞지 않으므로 이 값을 똑같이 1로 보정해주는 기능.
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        
        if(isDodge)
            moveVec = dodgeVec;
        

        transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn(){
         // 객체(플레이어)가 나아가는 방향으로 바라본다
        transform.LookAt(transform.position + moveVec);       
    }

    void Jump(){
        if(jDown && moveVec == Vector3.zero && !isJump && !isDodge){
            // AddForce: 물리적인 힘 가하기
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }
    void Dodge(){
        if(jDown && moveVec != Vector3.zero && !isJump && !isDodge){
            dodgeVec = moveVec;
            // 회피는 이동속도의 2배
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f);         
        }
    }

    void DodgeOut(){
        speed *= 0.5f;
        isDodge = false;
    }

    // 바닥에 착지하는 순간
    void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.tag == "Floor"){
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }
}
