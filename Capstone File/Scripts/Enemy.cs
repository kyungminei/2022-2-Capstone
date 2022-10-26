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
    public MeshRenderer[] mat;
    public NavMeshAgent nav;
    public Animator anim;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if (enemyType!=Type.D)
        {
            Invoke("ChaseStart", 1.0f);
        }
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
                yield return new WaitForSeconds(0.5f);
                MeleeArea.enabled = true;

                yield return new WaitForSeconds(1.0f);
                MeleeArea.enabled = false;

                yield return new WaitForSeconds(1.0f);
                break;

            case Type.B:
                yield return new WaitForSeconds(0.5f);
                MeleeArea.enabled = true;

                yield return new WaitForSeconds(0.7f);
                MeleeArea.enabled = false;

                yield return new WaitForSeconds(1.0f);
                break;

            case Type.C:
                yield return new WaitForSeconds(0.7f);
                MeleeArea.enabled = true;

                yield return new WaitForSeconds(1.0f);
                MeleeArea.enabled = false;

                yield return new WaitForSeconds(1.5f);
                break;
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
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;

            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec, false));

        }
        else if (other.tag=="Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;

            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);
            StartCoroutine(OnDamage(reactVec, false));

        }
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {

        foreach (MeshRenderer mesh in mat)
        {
            mesh.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        if(curHealth>0)
        {
            foreach (MeshRenderer mesh in mat)
            {
                mesh.material.color = Color.white;
            }
        }
        else
        {
            foreach (MeshRenderer mesh in mat)
            {
                mesh.material.color = Color.gray;
            }

            gameObject.layer = 14;
            isDead = true;
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

            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            }
             Destroy(gameObject, 4);
            
        }
    }
}
