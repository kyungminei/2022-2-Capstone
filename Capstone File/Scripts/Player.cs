using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Player : MonoBehaviour
{
    public float speed;
    public float slowSpeed;
    public float standardSpeed; //하이어라키창에서 수정안해도 됨.
    public GameObject weapon;
    public Camera followCamera;
    public bool hasweapon;
    public GameManager manager;
    public GameObject ChargeEffect;
    public GameObject arrow;

    public AudioSource hammerSound;
    public AudioSource ChargeSound;

    public int coin;
    public int health;
    public int score;

    public int maxcoin;
    public int maxhealth;

    float hAxis;
    float vAxis;

    float curChargeTime=0;

    bool iDown; //weapon
    bool wDown;
    bool fDown;
    bool fUp;
    bool gDown;
    bool rDown;
    bool jDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;


    bool isJump;
    bool isDodge;
    bool isFireReady = true;
    bool isBorder;
    bool isDamaged;
    bool isShop;
    bool isDead;
    bool isClicked;
    bool isCharge;
    bool isAttackTurn; // 공격에 의한 회전을 하는 중인지 아닌지.
    public bool isInSlowZone;

    Vector3 movevec;
    Vector3 dodgevec;
    Vector3 mousevec;

    Animator animator;
    Rigidbody rigid;
    //MeshRenderer[] meshs;
    SkinnedMeshRenderer skMat;
    Color firstColor; //처음 캐릭터 색상 저장

    GameObject nearobject;
    public Weapon equipWeapon; //장착중인 무기
    int equipWeaponIndex= -1;
    float fireDelay;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        //meshs= GetComponentsInChildren<MeshRenderer>();
        skMat = GetComponentInChildren<SkinnedMeshRenderer>();

        PlayerPrefs.SetInt("MaxScore",0);
        Debug.Log(PlayerPrefs.GetInt("MaxScore"));

        firstColor = skMat.materials[0].color;
        standardSpeed = speed;
    }

    void Start()
    {
        
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        arrowTurn();
        //Jump();
        RecordFDownTime();
        Attack();
        Dodge();
        interation();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); //GetAxisRaw : -1, 0 ,1 
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk"); //수정 필
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButtonDown("Fire1");
        fUp = Input.GetButtonUp("Fire1");
        iDown = Input.GetButtonDown("interation");
        sDown1 = Input.GetButtonDown("swap1");
        sDown2 = Input.GetButtonDown("swap2");
        sDown3 = Input.GetButtonDown("swap3");
    }

    void Move()
    {
        movevec = new Vector3(hAxis, 0, vAxis).normalized; 

        if(isDodge) 
        {
            movevec = dodgevec;
        }

        if( isDead) //!isFireReady뺌
        {
            movevec = Vector3.zero;
        }

        if(!isBorder)
        {
            transform.position += movevec * speed  * Time.deltaTime;
        }

        animator.SetBool("Iswalk", movevec != Vector3.zero);
        //animator.SetBool("Iswalk", wDown);
    }

    void Turn()
    {
        if(!isAttackTurn)
        {
            //키보드 회전
            transform.LookAt(transform.position + movevec);

            //마우스 회전
            if (fDown && !isDead)
            {
                Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit rayHit; //ray에 닿은 오브젝트 정보를 저장하는 변수
                if (Physics.Raycast(ray, out rayHit, 100))
                {
                    Vector3 nextVec = rayHit.point - transform.position;
                    nextVec.y = 0;
                    transform.LookAt(transform.position + nextVec);
                }
            }
        }
    }

    void arrowTurn()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane GroupPlane = new Plane(Vector3.up, Vector3.zero);

        float rayLength;

        if (GroupPlane.Raycast(cameraRay, out rayLength))
        {
            Vector3 pointTolook = cameraRay.GetPoint(rayLength);
            arrow.transform.LookAt(new Vector3(pointTolook.x, transform.position.y, pointTolook.z));
        }
    }


    void Jump()
    {
        if(jDown && movevec==Vector3.zero && !isJump && !isDodge && !isDead)
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 20, ForceMode.Impulse);
            animator.SetBool("Isjump", true);
            animator.SetTrigger("Dojump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if (jDown && movevec != Vector3.zero && !isJump && !isDodge && !isDead ) 
        {
            dodgevec = movevec; 
            animator.SetTrigger("Dododge");
            speed *= 2;
            isDodge = true;
            Debug.Log(speed);

            Invoke("DodgeOut", 0.7f);
        }
    }

    void DodgeOut()
    {
        if(isInSlowZone)
        {
            speed = slowSpeed;
            isDodge = false;
            return;
        }
        else
        {
            speed = standardSpeed;
            isDodge = false;
        }
        //speed *= 0.5f;
    }

    void interation()
    {
        if (iDown && nearobject != null && !isJump && !isDodge && !isDead)
        {
            if (nearobject.tag == "Weapon")
            {
                Item item = nearobject.GetComponent<Item>();
                int weaponIndex = item.value;

                hasweapon = true;

                Destroy(nearobject);
            }
            else if (nearobject.tag == "Shop")
            {
                Shop shop = nearobject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
            }
        }
    }

    void Attack()
    {
        if (equipWeapon == null) return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (isCharge && isFireReady && !isDodge && !isShop && !isDead && !isClicked)
        {
            StartCoroutine("SeeToMousePos");

            equipWeapon.ChargeAttack();
            animator.SetTrigger("Doattack");

            //ChargeSound.Play();
            fireDelay = 0;
            isCharge = false;
        }
        else if (fUp && isFireReady && !isDodge && !isShop && !isDead && !isCharge)
        {
            StopCoroutine("SeeToMousePos");
            StartCoroutine("SeeToMousePos");

            equipWeapon.MeleeAttack();
            animator.SetTrigger("Doattack");

            //hammerSound.Play();
            fireDelay = 0;
        }
    }

    void SaveMousePos()
    {    
        Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit; //ray에 닿은 오브젝트 정보를 저장하는 변수
        if (Physics.Raycast(ray, out rayHit, 100))
        {
            mousevec = rayHit.point; //마우스 누른 위치 저장
        }    
    }

    IEnumerator SeeToMousePos()
    {
        Vector3 tomouse = mousevec - transform.position;
        tomouse.y = 0;
        transform.LookAt(transform.position+tomouse);

        isAttackTurn = true;
        yield return new WaitForSeconds(0.5f);

        isAttackTurn = false;
    }
   
    void RecordFDownTime() //공격버튼 누른 시간 기록
    {
        if (fDown)
        {
            isClicked = true;
        }
        if (fUp)
        {   
            ChargeEffect.SetActive(false);
            isClicked = false;
            curChargeTime = 0;
        }

        if (isClicked)
        {
            SaveMousePos();
            ChargeEffect.SetActive(true);
            curChargeTime += Time.deltaTime;

            if (curChargeTime >= equipWeapon.maxChargeTime)
            {
                isCharge = true;
            }
        }
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.blue);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }
     void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag=="Floor")
        {
            animator.SetBool("Isjump", false);
            isJump = false;
        }
    }*/ //점프모션 추가되면 활성화

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxcoin)
                    {
                        coin = maxcoin;
                    }
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxhealth)
                    {
                        health = maxhealth;
                    }
                    break;
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamaged)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAttack = other.name == "Boss Melee Area";

                StartCoroutine(OnDamage(isBossAttack));
            }

            if (other.GetComponent<Rigidbody>() != null)
                Destroy(other.gameObject);
        }
        else if(other.tag=="FallingObject")
        {
            if(!isDamaged)
            {
                FallingObject fallingObj = other.GetComponent<FallingObject>();
                health -= fallingObj.Damage;
                fallingObj.particle.gameObject.SetActive(true);

                Vector3 reactVec = gameObject.transform.position - other.transform.position;
                reactVec = reactVec.normalized;
                rigid.AddForce(reactVec * 5,ForceMode.Impulse);

                StartCoroutine(OnDamage(false));

                Destroy(other.gameObject);
            }
        }
        else if (other.tag == "SlowZone")
        {
            isInSlowZone = true;
            speed = slowSpeed;
        }
    }

    IEnumerator OnDamage(bool isBossAttack)
    {
        isDamaged = true;

        skMat.materials[0].color = Color.yellow;

        if(isBossAttack)
        {
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);
        }

        if (health <= 0 && !isDead)
        {
            OnDie();
        }

        yield return new WaitForSeconds(1f);

        isDamaged = false;

        skMat.materials[0].color = firstColor;

        if (isBossAttack)
        {
            rigid.velocity = Vector3.zero;
        }

    }

    void OnDie()
    {
        animator.SetTrigger("Dodie");
        isDead = true;
        manager.GameOver();
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag=="Weapon" || other.tag == "Shop")
        {
            nearobject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {

        //if(other.tag == "Weapon")
        //{
        //    nearobject = null;
        //}
        //위 코드 때문에, weapon Shop에서 무기 구매 후 무기가 가까이에 있으면 null처리되어 아래문장에 에러가 생김.
        //(nearobject가 null이 되어버려서 상점에서 나와지지 않음)
        //위 코드가 필요해질 시, 주석을 해제하고, weapon Shop의 weapon spawn pos을 멀리 떨어진 곳으로 옮기기.
        if (other.tag == "Shop")
        {
            Shop shop = nearobject.GetComponent<Shop>();
            shop.Exit();
            isShop = false;

            nearobject = null;
        }
        else if(other.tag=="SlowZone")
        {
            isInSlowZone = false;
            speed = standardSpeed;
        }
    }
}
