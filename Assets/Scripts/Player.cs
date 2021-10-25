using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player script

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;
    bool iDown;

    //weapon swap
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isJump;
    bool isDodge;
    bool isSwap;

    Rigidbody rigid;
    Vector3 moveVec;
    Vector3 dodgeVec;


    Animator anim;

    GameObject nearObject;
    GameObject equipWeapon;
    int equipWeaponIndex = -1;

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
        // 떨어진 아이템과 상호작용
        Interaction();
        Swap();
    }

    void GetInput(){
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
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
        
        if(isSwap)
            moveVec = Vector3.zero;

        transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn(){
         // 객체(플레이어)가 나아가는 방향으로 바라본다
        transform.LookAt(transform.position + moveVec);       
    }

    void Jump(){
        if(jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap){
            // AddForce: 물리적인 힘 가하기
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }
    void Dodge(){
        if(jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap){
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

    void Swap(){

        // 입수하지 않은 무기에 대해 swap 시도하는 경우 예외 case
        if(sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if(sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if(sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;


        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {
            // 빈손인 경우 예외 case
            if (equipWeapon != null)
                equipWeapon.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex];
            equipWeapon.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.5f);   
        }    

    }
    
    void SwapOut(){
        isSwap = false;
    }

    void Interaction(){
        if(iDown && nearObject != null && !isJump && !isDodge){
            if(nearObject.tag == "Weapon"){
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }

    }

    // 바닥에 착지하는 순간
    void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.tag == "Floor"){
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    // 아이템 입수
    private void OnTriggerStay(Collider other) {
        if(other.tag == "Weapon")
            nearObject = other.gameObject;
        
        Debug.Log(nearObject);
    }
    private void OnTriggerExit(Collider other) {
        if(other.tag == "Weapon"){
            nearObject = null;
        }
    }
}
