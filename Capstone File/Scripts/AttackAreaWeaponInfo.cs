using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAreaWeaponInfo : MonoBehaviour
{
    //캐릭터 오른손에 있는 weapon에 바로 collider를 넣을시, 휘두를때마다 collider도 같이 움직여 몹 죽이는 난이도가 상승해버림.
    //그렇다고 weapon을 밖으로 빼버리면, 캐릭터 손에 들려있는것 같지 않아서 무척 부자연스러워보임.
    //player오브젝트 직계자손 오브젝트에 collider를 넣고, 무기와 weapon스크립트는 그대로 player오른손에 위치하도록 함.
    //enemy 스크립트에서 trigger로 weapon스크립트의 정보를 요구하기 때문에, 이 스크립트로 player오른손에 들려있는 무기에 붙여진 weapon스크립트에 접근함.
    //이렇게 하면, collider는 움직이지 않으면서, 적에게 데미지 또한 정상적으로 들어감.

    //사용방법 요약: player의 직계자손오브젝트에 해당 스크립트 어태치, 인스펙터 창에서 player오른손에 위치해놓은 무기 오브젝트 끌어와 놓기.

    //이 방법 말고 다른 방법이 있는지 계속 고민해볼 것.

    //이 스크립트가 들어가는 오브젝트의 tag 잘 설정하기. melee or chargemelee
    public GameObject matchWeaponGameObject;

}
