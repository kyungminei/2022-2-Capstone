using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public GameObject MenuCamera;
    public GameObject GameCamera;
    public Player player;
    public Boss boss;
    public GameObject itemShop;
    public GameObject startZone;
    public GameObject weaponShop;
    public int stage;
    public float playTime;
    public bool isBattle;
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;
    public int enemyCntD;


    public Transform[] enemyZone;
    public GameObject[] enemies;
    public List<int> enemyList;

    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject overPanel;

    public Text maxScoreText;

    public Text scoreText;
    public Text stageText;
    public Text playtimeText;
    public Text playerHealth;
    public Text playerAmmo;
    public Text playerCoin;
    public Image weapon1Img;
    public Image weapon2Img;
    public Image weapon3Img;
    public Image weaponRImg;
    public Text enemyAText;
    public Text enemyBText;
    public Text enemyCText;
    public RectTransform bossHealthGroup;
    public RectTransform bossHealthBar;
    public Text curScoreText;
    public Text bestScoreText;

    private void Awake()
    {
        enemyList = new List<int>();
        maxScoreText.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));

        if(PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }
    }

    public void GameStart()
    {
        MenuCamera.SetActive(false);
        GameCamera.SetActive(true);

        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true);
    }

    public void GameOver()
    {
        gamePanel.SetActive(false);
        overPanel.SetActive(true);
        curScoreText.text = scoreText.text;

        int maxScore = PlayerPrefs.GetInt("MaxScore");
        if(player.score>maxScore)
        {
            bestScoreText.gameObject.SetActive(true);
            PlayerPrefs.GetInt("MaxScore", player.score);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void StageStart()
    {
        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);

        foreach( Transform zone in enemyZone)
        {
            zone.gameObject.SetActive(true);
        }

        isBattle = true;
        StartCoroutine(InBattle());
    }

    public void StageEnd()
    {
        player.transform.position = Vector3.up * 0.8f;

        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        startZone.SetActive(true);

        foreach (Transform zone in enemyZone)
        {
            zone.gameObject.SetActive(false);
        }

        isBattle = false;
        stage++;
    }

    IEnumerator InBattle()
    {
        if(stage%5==0)
        {
            enemyCntD++;
            GameObject instantEnemy = Instantiate(enemies[3], enemyZone[0].position, enemyZone[0].rotation);
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.Target = player.transform;
            enemy.manager = this;
            boss = instantEnemy.GetComponent<Boss>();
        }
        else
        {
            for (int index = 0; index < stage; index++)
            {
                int ran = Random.Range(0, 3);
                enemyList.Add(ran);

                switch (ran)
                {
                    case 0:
                        enemyCntA++;
                        break;
                    case 1:
                        enemyCntB++;
                        break;
                    case 2:
                        enemyCntC++;
                        break;
                }
            }
        }

        while(enemyList.Count>0)
        {
            int ran = Random.Range(0, 4);
            GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemyZone[ran].position, enemyZone[ran].rotation);
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.Target = player.transform;
            enemy.manager = this;
            enemyList.RemoveAt(0);
            yield return new WaitForSeconds(4f); //Enumerator의 while문 안에 yield return을 포함시키는 것이 좋다.
        }

        while(enemyCntA+enemyCntB+enemyCntC+enemyCntD>0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(4f);

        boss = null;
        StageEnd();
    }

    private void Update()
    {
        if(isBattle)
        {
            playTime += Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        //상단 UI
        scoreText.text= string.Format("{0:n0}", player.score);
        stageText.text = "STAGE " + stage;

        int hour = (int)(playTime / 3600);
        int min = (int)((playTime - hour * 3600) / 60);
        int second = (int)(playTime % 60);

        playtimeText.text = string.Format("{0:00}", hour) + ":" + string.Format("{0:00}", min) + ":" + string.Format("{0:00}", second);

        //플레이어 UI
        playerHealth.text = player.health + "/" + player.maxhealth;
        playerCoin.text= string.Format("{0:n0}", player.coin);
        if (player.equipWeapon == null )
        {
            playerAmmo.text = "- / " + player.ammo;
        }
        else if(player.equipWeapon.type == Weapon.Type.Melee)
        {
            playerAmmo.text = "- / " + player.ammo;
        }
        else
        {
            playerAmmo.text = player.equipWeapon.curAmmo + " / " + player.ammo;
        }

        //무기 UI
        weapon1Img.color = new Color(1, 1, 1, player.hasweapons[0] ? 1 : 0);
        weapon2Img.color = new Color(1, 1, 1, player.hasweapons[1] ? 1 : 0);
        weapon3Img.color = new Color(1, 1, 1, player.hasweapons[2] ? 1 : 0);
        weaponRImg.color = new Color(1, 1, 1, player.hasGrenades>0 ? 1 : 0);

        //몬스터 숫자UI
        enemyAText.text = enemyCntA.ToString();
        enemyBText.text = enemyCntB.ToString();
        enemyCText.text = enemyCntC.ToString();

        //보스 체력 UI
        if(boss!=null)
        {
            bossHealthGroup.anchoredPosition = Vector3.down* 30;
            bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);
        }
        else
        {
            bossHealthGroup.anchoredPosition = Vector3.up * 200;
        }
    }
}
