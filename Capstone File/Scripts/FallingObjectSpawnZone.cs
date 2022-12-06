using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FallingObjectSpawnZone : MonoBehaviour
{
    public GameObject DropSpawnZone;
    public GameObject AttackArea;
    public GameObject FallingGameObject;
    public GameManager manager;

    GameObject spawnpos;

    float spawnZoneSize = 40f;
    float spawnzone_x;
    float spawnzone_z;
    int curSpawnedObjNum = 0; //현재 스폰되어 있는 낙하물 갯수

    bool isCreating;

    void Start()
    {
        spawnzone_x = DropSpawnZone.transform.position.x;
        spawnzone_z = DropSpawnZone.transform.position.z;
    }

    void Update()
    {
        if (manager.isBattle == true)
        {
            AttackSet();
        }
    }

    public void AttackSet()
    {
        if(!(curSpawnedObjNum>10))
        {
            float rannum = Random.Range(0, 10);

            if (rannum == 1 && !isCreating)
            {
                CreateAttackZone();
            }
        }
    }

    public void CreateAttackZone()
    {
        isCreating = true;

        float ranPos_x = Random.Range(-spawnZoneSize, spawnZoneSize);
        float ranPos_z = Random.Range(-spawnZoneSize, spawnZoneSize);

        Vector3 spawnPoint = new Vector3(spawnzone_x + ranPos_x,0, spawnzone_z + ranPos_z);

        spawnpos = Instantiate(AttackArea, spawnPoint, Quaternion.identity);
        spawnpos.transform.SetParent(this.gameObject.transform, false) ;

        StartCoroutine(SpawnObject());
    }

    IEnumerator SpawnObject()
    {
        curSpawnedObjNum += 1;

        yield return new WaitForSeconds(2.0f);

        Vector3 spawnPoint = spawnpos.transform.position + new Vector3(0, 40f, 0);
        GameObject spawnObj = Instantiate(FallingGameObject,spawnPoint,Quaternion.identity);

        Destroy(spawnpos);
        curSpawnedObjNum -= 1;
        isCreating = false;
    }
}
