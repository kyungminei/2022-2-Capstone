using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : Enemy
{
    public GameObject L_MeleeArea;

    Vector3 lookVec;
    bool isLook;
    bool isCommonAttack;
    bool onAttack;
    bool isFindPlayer;
    bool isStopCommonAttack;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        skMat = GetComponentInChildren<SkinnedMeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        firstColor = skMat.materials[0].color;
        nav.isStopped = true;

        StartCoroutine(SelectAttackPattern());

    }

    void Update()
    {
        if(isDead)
        {
            StopAllCoroutines();
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
            nav.SetDestination(Target.position);
        }
    }

    IEnumerator SelectAttackPattern()
    {
        int rannum = Random.Range(0, 4);

        switch(rannum)
        {
            case 0:
                //Debug.Log("일반 공격 기믹 시작");
                //StartCoroutine(CommonAttack());
                //StartCoroutine(ExitCommonAttack());
                //break;
            case 1:
            case 2:
            case 3:
                Debug.Log("점프 공격 기믹 시작");
                StartCoroutine(JumpAttack());
                break;
        }
        yield return null;
    }

    //case0 기믹 메소드들 (일반 공격)
    IEnumerator CommonAttack()
    {
        isCommonAttack = true;
        isLook = false;
        nav.isStopped = false;
        anim.SetBool("Iswalk", true);

        yield return null;
    }

    IEnumerator ExitCommonAttack()
    {
        yield return new WaitForSeconds(15.0f);

        isStopCommonAttack = true;
        isCommonAttack = false;
        isLook = true;
        nav.isStopped = true;
        isFindPlayer = false;
        onAttack = false;
        anim.SetBool("Iswalk", false);

        StopAllCoroutines();
        StartCoroutine(SelectAttackPattern());
    }

    IEnumerator DoAttack()
    {
        onAttack = true;
        nav.isStopped = true;
        anim.SetBool("Iswalk", false);
        anim.SetTrigger("Doattack1");
        L_MeleeArea.SetActive(true);

        yield return new WaitForSeconds(2.5f);
        L_MeleeArea.SetActive(false);

        yield return new WaitForSeconds(4.0f);
        anim.SetBool("Iswalk", true);
        nav.isStopped = false;
        isFindPlayer = false;
        onAttack = false;

        if (isStopCommonAttack == false)
        {
            StartCoroutine(CommonAttack());
        }
    }

    //case2,3 기믹 메소드들 (점프 공격)
    IEnumerator JumpAttack()
    {
        isLook = true;

        yield return new WaitForSeconds(2f);
        Vector3 jumppos = Target.position + lookVec;
        isLook = false;

        nav.speed *= 4f;
        nav.acceleration *= 10;
        nav.isStopped = false;
        nav.SetDestination(jumppos);

        boxCollider.enabled = false;
        anim.SetTrigger("Doattack3");

        yield return new WaitForSeconds(0.5f);
        MeleeArea.enabled = true;

        yield return new WaitForSeconds(0.5f);
        nav.isStopped = true;

        yield return new WaitForSeconds(1.5f);
        MeleeArea.enabled = false;
        nav.speed *= 0.25f;
        nav.acceleration *= 0.1f;
        boxCollider.enabled = true;

        yield return new WaitForSeconds(3f);
        StopAllCoroutines();
        StartCoroutine(SelectAttackPattern());

        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player" && isCommonAttack == true && !onAttack && !isFindPlayer) //일반 공격 중에 플레이어를 찾으면
        {
            Debug.Log("플레이어와 닿았다!");
            isFindPlayer = true;
            StartCoroutine(DoAttack());
        }
    }
}
