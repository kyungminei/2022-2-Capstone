using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject missile;
    public Transform missilePortA;
    public Transform missilePortB;

    Vector3 lookVec;
    Vector3 tauntVec;
    public bool isLook;

    //awake는 자식 스크립트만 실행된다 (주의하기!)
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        nav.isStopped = true;

        StartCoroutine(Think());
    }

    void Update()
    {
        if(isDead)
        {
            StopAllCoroutines();
            return;
        }

        if(isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;
            transform.LookAt(Target.position + lookVec);
        }
        else
        {
            nav.SetDestination(tauntVec);
        }
    }

    IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);

        int ranAction = Random.Range(0, 5); //0 1 2 3 4
        switch (ranAction) 
        {
            case 0:
            case 1:
                // 미사일 발사 패턴
                StartCoroutine(MissileShot());
                break;

            case 2:
            case 3:
                //돌 굴러가는 패턴
                StartCoroutine(RockShot());
                break;

            case 4:
                //점프 공격 패턴
                StartCoroutine(Taunt());
                break;
        }

    }

    IEnumerator MissileShot()
    {
        anim.SetTrigger("Doshot");
        yield return new WaitForSeconds(0.2f);
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        BossMissile bossmissileA = instantMissileA.GetComponent<BossMissile>();
        bossmissileA.target = Target;

        yield return new WaitForSeconds(0.3f);
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        BossMissile bossmissileB = instantMissileB.GetComponent<BossMissile>();
        bossmissileB.target = Target;

        yield return new WaitForSeconds(2f);
        StartCoroutine(Think());
    }

    IEnumerator RockShot()
    {
        isLook = false;
        anim.SetTrigger("Dobigshot");
        Instantiate(bullet, transform.position, transform.rotation);
        yield return new WaitForSeconds(3f) ;

        isLook = true;
        StartCoroutine(Think());
    }

    IEnumerator Taunt()
    {
        tauntVec = Target.position + lookVec;

        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;
        anim.SetTrigger("Dotaunt");
        yield return new WaitForSeconds(1.5f);
        MeleeArea.enabled = true;

        yield return new WaitForSeconds(0.5f);
        MeleeArea.enabled = false;

        yield return new WaitForSeconds(1f);
        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;

        StartCoroutine(Think());
    }
}

