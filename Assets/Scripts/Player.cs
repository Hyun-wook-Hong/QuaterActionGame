using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player script

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    // 수류탄: 필살기 - 공전물체로 보여주기 위해 배열 생성
    public GameObject[] grenades;
    public int hasGrenades;
    public Camera followCamera;

    /* 현재 총알, 코인, 체력, 수류탄 보유갯수 */
    public int ammo;
    public int coin;
    public int health;

    /* 최대 총알, 코인, 체력, 수류탄 보유갯수 */
    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool fDown;
    bool rDown;
    bool iDown;

    //weapon swap
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isFireReady = true;
    bool isBorder;

    Rigidbody rigid;
    Vector3 moveVec;
    Vector3 dodgeVec;


    Animator anim;

    GameObject nearObject;
    Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float fireDelay;

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
        // 공격 (L마우스)
        Attack();
        // 원거리 무기 재장전
        Reload();
        // 회피기능
        Dodge();
        // 떨어진 아이템과 상호작용
        Interaction();
        Swap();
    }

    void GetInput(){
        hAxis  = Input.GetAxisRaw("Horizontal");
        vAxis  = Input.GetAxisRaw("Vertical");
        wDown  = Input.GetButton("Walk");
        jDown  = Input.GetButtonDown("Jump");
        fDown  = Input.GetButton("Fire1");
        rDown  = Input.GetButtonDown("Reload");
        iDown  = Input.GetButtonDown("Interaction");
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
        
        if(isSwap || isReload || !isFireReady)
            moveVec = Vector3.zero;
        // 감지한 장애물이 벽이 아닌 경우에만 이동 (관통방지)
        if(!isBorder)
            transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }

    void Turn(){
        // #1. 키보드에 의한 회전 - 객체(플레이어)가 나아가는 방향으로 바라본다
        transform.LookAt(transform.position + moveVec);  

        // #2. 마우스에 의한 회전
        if(fDown){
        Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit;
        // out: return처럼 반환값을 주어진 변수에 저장하는 키워드
        if(Physics.Raycast(ray, out rayHit, 100)){
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }


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
    void Attack(){
        if(equipWeapon == null)
            return;
        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.speedOfAttack < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap){
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Reload(){
        if (equipWeapon ==  null)
            return;
        if (equipWeapon.type == Weapon.Type.Melee)
            return;
        if (ammo == 0)
            return;

        if(rDown && !isJump && !isDodge && !isSwap && isFireReady)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 1.0f); 
        }
    }

    // 재장전 모션 끝난 후
    void ReloadOut()
    {
        // 총알 계산로직
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;

        isReload = false;
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
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

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
    // 플레이어 자동회전 방지
    void FreezeRotation(){
        rigid.angularVelocity = Vector3.zero;
    }

    // 플레이어 벽 관통 방지
    void StopToWall(){
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, 
                                   transform.forward, 5, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate() {
        FreezeRotation();
        StopToWall();
    }

    // 바닥에 착지하는 순간
    void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.tag == "Floor"){
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    // 총알, 코인, 체력, 수류탄 아이템 입수 (OnTriggerEnter 사용)
    void OnTriggerEnter(Collider other) {
        if(other.tag == "Item"){
            Item item = other.GetComponent<Item>();
            switch(item.type){
                case Item.Type.Ammo:
                    ammo += item.value;
                    // 입수 시 수치를 더했을때 최댓값을 넘는 수치가 된다면 최댓값으로 고정
                    if(ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    // 입수 시 수치를 더했을때 최댓값을 넘는 수치가 된다면 최댓값으로 고정
                    if(coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    // 입수 시 수치를 더했을때 최댓값을 넘는 수치가 된다면 최댓값으로 고정
                    if(health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    // 이미 max일 경우 더이상 입수 이벤트 로직 발동할 필요 없음
                    if(hasGrenades == maxHasGrenades)
                        return;
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    // 입수 시 수치를 더했을때 최댓값을 넘는 수치가 된다면 최댓값으로 고정
                    if(hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;
                    break;
            }
            Destroy(other.gameObject);
        }
    }

    // 무기 아이템 입수, 입수 시 필드에 있는 아이템 오브젝트 없애기
    void OnTriggerStay(Collider other) {
        if(other.tag == "Weapon")
            nearObject = other.gameObject;
        
        //Debug.Log(nearObject);
    }
    void OnTriggerExit(Collider other) {
        if(other.tag == "Weapon"){
            nearObject = null;
        }
    }
}
