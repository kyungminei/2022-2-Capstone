using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public GameObject[] grenades;
    public Camera followCamera;
    public bool[] hasweapons;
    public int hasGrenades;
    public GameObject grenadeObj;
    public GameManager manager;

    public AudioSource hammerSound;

    public int ammo;
    public int coin;
    public int health;
    public int score;

    public int maxammo;
    public int maxcoin;
    public int maxhealth;
    public int maxhasGrenades;


    float hAxis;
    float vAxis;

    bool iDown; //weapon
    bool wDown;
    bool fDown;
    bool gDown;
    bool rDown;
    bool jDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;


    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isFireReady = true;
    bool isBorder;
    bool isDamaged;
    bool isShop;
    bool isDead;

    Vector3 movevec;
    Vector3 dodgevec;

    Animator animator;
    Rigidbody rigid;
    MeshRenderer[] meshs;

    GameObject nearobject;
    public Weapon equipWeapon; //장착중인 무기
    int equipWeaponIndex= -1;
    float fireDelay;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        meshs= GetComponentsInChildren<MeshRenderer>();

        PlayerPrefs.SetInt("MaxScore",11200);
        Debug.Log(PlayerPrefs.GetInt("MaxScore"));
    }

    void Start()
    {
        
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Grenade();
        Reload();
        Dodge();
        interation();
        Swap();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); //GetAxisRaw : -1, 0 ,1 
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButton("Fire2");
        rDown = Input.GetButtonDown("Reload");
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

        if(isSwap || !isFireReady || isReload || isDead)
        {
            movevec = Vector3.zero;
        }

        if(!isBorder)
        {
            transform.position += movevec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;
        }

        animator.SetBool("Isrun", movevec != Vector3.zero); 
        animator.SetBool("Iswalk", wDown);
    }

    void Turn()
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

    void Jump()
    {
        if(jDown && movevec==Vector3.zero && !isJump && !isDodge &&!isSwap && !isDead)
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 20, ForceMode.Impulse);
            animator.SetBool("Isjump", true);
            animator.SetTrigger("Dojump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if (jDown && movevec != Vector3.zero && !isJump && !isDodge && !isSwap && !isDead ) 
        {
            dodgevec = movevec; 
            animator.SetTrigger("Dododge");
            speed *= 2;
            isDodge = true;

            Invoke("DodgeOut", 0.5f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void interation()
    {
        if (iDown && nearobject != null && !isJump && !isDodge && !isDead)
        {
            if (nearobject.tag == "Weapon")
            {
                Item item = nearobject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasweapons[weaponIndex] = true;

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

    void Swap()
    {
        if (sDown1 && (!hasweapons[0] || equipWeaponIndex == 0)) return; //획득하지 않은 무기거나, 이미 들고있는 무기면 return
        if (sDown2 && (!hasweapons[1] || equipWeaponIndex == 1)) return;
        if (sDown3 && (!hasweapons[2] || equipWeaponIndex == 2)) return;


        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if((sDown1 || sDown2 || sDown3) && !isJump && !isDodge && !isDead)
        {
            if(equipWeapon!=null)
            {
                equipWeapon.gameObject.SetActive(false);
            }
            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            animator.SetTrigger("Doswap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Attack()
    {
        if (equipWeapon == null) return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge && !isSwap && !isShop && !isDead)
        {
            equipWeapon.Use();
            animator.SetTrigger(equipWeapon.type==Weapon.Type.Melee ? "Doswing" : "Doshot");
            if (equipWeapon.type == Weapon.Type.Melee) //오디오 테스트
                hammerSound.Play();
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if (equipWeapon == null) return;

        if (equipWeapon.type == Weapon.Type.Melee) return;

        if (ammo == 0) return;

        if(rDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop && !isDead)
        {
            animator.SetTrigger("Doreload");
            isReload = true;

            Invoke("ReloadOut", 1.0f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo; //maxAmmo:40, ammo:10일 때, ammo만큼 장전되어야함.
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo; //장전한 만큼 빼주기
        isReload = false;
    }

    void Grenade()
    {
        if (hasGrenades == 0) return;

        if(gDown &&  !isReload && !isSwap && !isShop && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit; //ray에 닿은 정보를 저장하는 변수
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
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

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag=="Floor")
        {
            animator.SetBool("Isjump", false);
            isJump = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag=="Item")
        {
            Item item = other.GetComponent<Item>();
            switch(item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if( ammo >maxammo)
                    {
                        ammo = maxammo;
                    }
                    break;
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
                        health=maxhealth;
                    }
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxhasGrenades)
                    {
                        hasGrenades = maxhasGrenades;
                    }
                    break;
            }
            Destroy(other.gameObject);
        }
        else if(other.tag=="EnemyBullet")
        {
            if(!isDamaged)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAttack = other.name == "Boss Melee Area";

                StartCoroutine(OnDamage(isBossAttack));
            }

            if (other.GetComponent<Rigidbody>() != null)
                Destroy(other.gameObject);
        }
    }

    IEnumerator OnDamage(bool isBossAttack)
    {
        isDamaged = true;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }

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
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

        if(isBossAttack)
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
        if(other.tag == "Weapon")
        {
            nearobject = null;
        }
        else if (other.tag == "Shop")
        {
            Shop shop = nearobject.GetComponent<Shop>();
            shop.Exit();
            isShop = false;

            nearobject = null;
        }
    }
}
