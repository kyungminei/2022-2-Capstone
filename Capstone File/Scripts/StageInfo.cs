using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageInfo : MonoBehaviour
{
    public GameManager manager;
    public int stage_num;

    public void clickedSpringStage()
    {
        manager.SetGround(stage_num, manager.StageType = GameManager.stageType.spring);
    }

    public void clickedSummerStage()
    {
        manager.SetGround(stage_num, manager.StageType = GameManager.stageType.summer);
    }

    public void clickedfallStage()
    {
        manager.SetGround(stage_num, manager.StageType = GameManager.stageType.fall);
    }

    public void clickedwinterStage()
    {
        manager.SetGround(stage_num, manager.StageType = GameManager.stageType.winter);
    }

}


