using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject menuCam;
    public GameObject gameCam;
    public Player player;
    public Boss boss;
    public GameObject itemShop;
    public GameObject weaponShop;
    public GameObject startZone;
    public int stage;
    public float playTime;
    public bool isBattle;
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;
    public int enemyCntD;

    public Transform [] enemyZones;
    public GameObject[] enemies;
    public List<int> enemyList;

    // UI Control
    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject overPanel;

    // 상단의 텍스트들
    public Text maxScoreTxt;
    public Text scoreTxt;
    public Text stageTxt;
    public Text playTimeTxt;
    
    // 좌측하단
    public Text playerHealthTxt;
    public Text playerAmmoTxt;
    public Text playerCoinTxt;

    // 무기 UI
    public Image weapon1Img;
    public Image weapon2Img;
    public Image weapon3Img;
    public Image weaponRImg;

    public Text enemyAtxt;
    public Text enemyBtxt;
    public Text enemyCtxt;

    // Boss
    public RectTransform bossHealthGroup;
    public RectTransform bossHealthBar;
    public Text curScoreText;
    public Text bestText;

    void Awake(){
        enemyList = new List<int>();
        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));
    }

    void Update(){
        if(isBattle)
            playTime += Time.deltaTime;
    }

    // 게임 시작
    public void GameStart()
    {
        menuCam.SetActive(false);
        gameCam.SetActive(true);

        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true);
    }
    // 게임 오버
    public void GameOver()
    {
        gamePanel.SetActive(false);
        overPanel.SetActive(true);
        curScoreText.text = scoreTxt.text;

        int maxScore = PlayerPrefs.GetInt("MaxScore");
        if (player.score > maxScore){
            bestText.gameObject.SetActive(true);
            PlayerPrefs.GetInt("MaxScore", player.score);
        }
    }

    public void Restart(){
        SceneManager.LoadScene(0);
    }

    public void StageStart(){
        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);

        foreach(Transform zone in enemyZones)
            zone.gameObject.SetActive(true);

        isBattle = true;
        StartCoroutine(InBattle());
    }

    public void StageEnd(){
        player.transform.position = Vector3.up * 0.8f;

        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        startZone.SetActive(true);

        foreach(Transform zone in enemyZones)
            zone.gameObject.SetActive(false);

        isBattle = false;
        stage++;
    }

    IEnumerator InBattle()
    {
        // Stage 5단위마다 보스 소환
        if((stage % 5) == 0)
        {
            // Boss type enemy counting
            enemyCntD++;

            GameObject instantEnemy = Instantiate(enemies[3], 
                enemyZones[0].position, enemyZones[0].rotation);
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.target = player.transform;
            enemy.manager = this;

            boss = instantEnemy.GetComponent<Boss>();
        }
        else{
                for(int index=0; index < stage; index++){
                int ran = Random.Range(0, 3);
                enemyList.Add(ran);

            // counting
                switch(ran){
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

                while(enemyList.Count > 0){
                int ranZone = Random.Range(0, 4);
                GameObject instantEnemy = Instantiate(enemies[enemyList[0]], 
                                        enemyZones[ranZone].position, 
                                        enemyZones[ranZone].rotation);
                Enemy enemy = instantEnemy.GetComponent<Enemy>();
                enemy.target = player.transform;
                enemy.manager = this;
                enemyList.RemoveAt(0);

                yield return new WaitForSeconds(4f);
                }  
        }
        // **update처럼 실행됨, 트렌드임
        // 필드에 소환된 총 몬스터 마릿수가 남아 있으면 대기
        while((enemyCntA + enemyCntB + enemyCntC + enemyCntD) > 0){
            yield return null;
        }
        // 아니라면 while 문을 벗어나서 StageEnd() 호출
        yield return new WaitForSeconds(4f);
        boss = null;
        StageEnd();
    }

    // 인게임 UI 로직
    // LateUpdate - Update()가 끝난 후 호출되는 생명주기
    void LateUpdate(){
        // 상단 UI
        scoreTxt.text = string.Format("{0:n0}", player.score);
        stageTxt.text = "STAGE " + stage;

        int hour =      (int)(playTime / 3600);
        int min =       (int)((playTime - hour * 3600) / 60);
        int second =    (int)(playTime % 60);

        playTimeTxt.text = string.Format("{00:n0}", hour) + ":" 
                         + string.Format("{00:n0}", min)  + ":"
                         + string.Format("{00:n0}", second);


        // 플레이어 상태 UI
        playerHealthTxt.text = player.health + " / " + player.maxHealth;
        playerCoinTxt.text = string.Format("{0:n0}", player.coin);

        if(player.equipWeapon == null)
            playerAmmoTxt.text = " - / " + player.ammo;
        else if(player.equipWeapon.type == Weapon.Type.Melee)
            playerAmmoTxt.text = " - / " + player.ammo;
        else
            playerAmmoTxt.text = player.equipWeapon.curAmmo + " / " + player.ammo;

        // 무기 UI
        weapon1Img.color = new Color(1, 1, 1, player.hasWeapons[0] ? 1 : 0);
        weapon2Img.color = new Color(1, 1, 1, player.hasWeapons[1] ? 1 : 0);
        weapon3Img.color = new Color(1, 1, 1, player.hasWeapons[2] ? 1 : 0);
        weaponRImg.color = new Color(1, 1, 1, player.hasGrenades > 0 ? 1 : 0);

        // 남은 몬스터 수 UI
        enemyAtxt.text = enemyCntA.ToString();
        enemyBtxt.text = enemyCntB.ToString();
        enemyCtxt.text = enemyCntC.ToString();

        // 보스 체력 UI
        if(boss != null){
            bossHealthGroup.anchoredPosition = Vector3.down * 30;
            bossHealthBar.localScale = new Vector3(((float)boss.curHealth / boss.maxHealth), 1, 1);
        }
        else{
            bossHealthGroup.anchoredPosition = Vector3.up * 200;
        }
    }
}
