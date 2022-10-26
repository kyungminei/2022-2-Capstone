using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, ChargeMelee};
    public Type type;
    public int meleeDamage;
    public int chargeDamage;
    public float rate;
    public float maxChargeTime;


    public BoxCollider meleeArea; //일반공격
    public BoxCollider ChargeMeleeArea;  //차지공격
    public TrailRenderer meleeTrailEffect;

    public void MeleeAttack()
    {
        StopCoroutine("Swing");
        StartCoroutine("Swing");
    }

    public void ChargeAttack()
    {
        StartCoroutine("ChargeShot");
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.1f); // yield : 결과를 낸다
        meleeArea.enabled = true;
        meleeTrailEffect.enabled = true;

        yield return new WaitForSeconds(0.4f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f);
        meleeTrailEffect.enabled = false;

        //yield break; //코루틴 탈출
    }

    IEnumerator ChargeShot()
    {
        ChargeMeleeArea.enabled = true;

        yield return new WaitForSeconds(0.4f);
        ChargeMeleeArea.enabled = false;


        yield return null;
    }
    /*IEnumerator Shot()
    {
        //1 총알발사
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;

        yield return null;

        //2 탄피 배출
        GameObject instantBulletCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody bulletCaseRigid = instantBulletCase.GetComponent<Rigidbody>();
        Vector3 CaseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        bulletCaseRigid.AddForce(CaseVec, ForceMode.Impulse);
        bulletCaseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse); //탄피 회전
    }*/

    //Use() 메인루틴 -> Swing() 서브루틴 -> Use() 메인루틴
    //Use() 메인루틴 + Swing() 코루틴 (Co-op)
}
