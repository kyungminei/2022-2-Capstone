using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D };
    public Type enemyType;

    public int maxHealth;
    public int curHealth;
    public int score;
    public GameManager manager;
    public Transform Target;
    public bool isChase;
    public BoxCollider MeleeArea; //공격범위
    public GameObject bullet;
    public bool isAttack;
    public bool isDead;
    public GameObject[] coins;

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    //public MeshRenderer[] mat;
    public SkinnedMeshRenderer skMat;
    public Color firstColor; //원래 색상 저장.

    public NavMeshAgent nav;
    public Animator anim;

    public AudioClip HitSound;
    AudioSource audioSource;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        //mat = GetComponentsInChildren<MeshRenderer>();
        skMat = GetComponentInChildren<SkinnedMeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        this.audioSource = GetComponent<AudioSource>();

        firstColor = skMat.materials[0].color;

        if (enemyType!=Type.D)
        {
            Invoke("ChaseStart", 1.0f);
        }
    }

    void PlaySound(string action)
    {
        switch (action)
        {
            case "Hit":
                audioSource.clip = HitSound;
                break;
        }
        audioSource.Play();
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("Iswalk", true);
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec,true));
    }

    private void Update()
    {
        if(nav.enabled && enemyType != Type.D)
        {
            nav.SetDestination(Target.position);
            nav.isStopped = !isChase;  //쫓는 중이면 안멈추고, 쫓는 중이 아니면 멈춤
        }
    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    void Targeting()
    {
        if(!isDead && enemyType != Type.D)
        {
            float targetRadius = 0;
            float targetRange = 0;

            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;


                case Type.B:
                    targetRadius = 1.0f;
                    targetRange = 3f;
                    break;

                case Type.C:
                    targetRadius = 3.0f;
                    targetRange = 3f;
                    break;
            }
            RaycastHit[] rayHits =
                Physics.SphereCastAll(transform.position, targetRadius,
                transform.forward, targetRange,
                LayerMask.GetMask("Player"));

            if (rayHits.Length > 0 && !isAttack) //충돌한게 있으면
            {
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("Isattack", true);

        switch (enemyType)
        {
            case Type.A:

                yield return new WaitForSeconds(0.75f);
                
                MeleeArea.enabled = true;

                yield return new WaitForSeconds(0.3f);
                MeleeArea.enabled = false;

                yield return new WaitForSeconds(1.0f);
                break;


            case Type.B:
                yield return new WaitForSeconds(0.75f);
                MeleeArea.enabled = true;

                yield return new WaitForSeconds(0.3f);
                MeleeArea.enabled = false;

                yield return new WaitForSeconds(1.0f);
                break;

            case Type.C:
                yield return new WaitForSeconds(1.5f);
                MeleeArea.enabled = true;

                yield return new WaitForSeconds(0.3f);
                MeleeArea.enabled = false;

                yield return new WaitForSeconds(1.5f);
                break;
        }
        isChase = true;
        isAttack = false;
        anim.SetBool("Isattack", false);
    }

    void FixedUpdate()
    {
        Targeting();
        FreezeVelocity();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag=="Melee")
        {
            AttackAreaWeaponInfo atk = other.GetComponent<AttackAreaWeaponInfo>();
            Weapon weapon = atk.matchWeaponGameObject.GetComponent<Weapon>();
            curHealth -= weapon.meleeDamage;
            if (curHealth <= 0) curHealth = 0;

            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec, false));

        }
        else if(other.tag=="ChargeMelee")
        {
            AttackAreaWeaponInfo atk = other.GetComponent<AttackAreaWeaponInfo>();
            Weapon weapon = atk.matchWeaponGameObject.GetComponent<Weapon>();
            curHealth -= weapon.chargeDamage;
            if (curHealth <= 0) curHealth = 0;
            Debug.Log("적이 차징공격을 맞았다!!");

            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec, false));
        }
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        PlaySound("Hit");
        skMat.materials[0].color = Color.red;

        yield return new WaitForSeconds(0.1f);

        if(curHealth>0)
        {
            skMat.materials[0].color = firstColor;
        }
        else
        {
            //몬스터 시체에 또 총을 쏘면, 몬스터count가 줄어들길래 추가해봄.
            if (isDead)
            {
                yield break;
            }
            skMat.materials[0].color = Color.gray;

            gameObject.layer = 12;
            isChase = false;
            nav.enabled = false; //사망리액션을 유지하기 위해 nav 끔
            anim.SetTrigger("Dodie");

            Player player = Target.GetComponent<Player>();
            player.score += score;
            int ranCoin = Random.Range(0, 3);
            Instantiate(coins[ranCoin], transform.position, Quaternion.identity);

            switch(enemyType)
            {
                case Type.A:
                    manager.enemyCntA--;
                    break;

                case Type.B:
                    manager.enemyCntB--;
                    break;

                case Type.C:
                    manager.enemyCntC--;
                    break;

                case Type.D:
                    manager.enemyCntD--;
                    break;
            }

            /*if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
            }*/     
            
            reactVec = reactVec.normalized;
            reactVec += Vector3.up;
            rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            
             Destroy(gameObject, 4);
             isDead = true;
        }
    }
}
