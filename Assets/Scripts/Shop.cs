using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public RectTransform uiGroup;
    public Animator anim;
    public GameObject[] itemObj;
    public int[] itemPrice;
    public Transform[] itemPos;
    public string[] talkData;
    public Text talkText;
    Player enterPlayer;

    // 상점 입장
    public void Enter(Player player)
    {
        enterPlayer = player;   
        uiGroup.anchoredPosition = Vector3.zero;
    }

    // 상점 퇴장
    public void Exit()
    {
        anim.SetTrigger("doHello");
        uiGroup.anchoredPosition = Vector3.down * 1000; 
    }

    public void Buy(int index)
    {
        // 현재 선택한 item의 금액
        int price = itemPrice[index];

        // 잔액이 모자랄 경우
        if(price > enterPlayer.coin){
            StopCoroutine(Talk());
            StartCoroutine(Talk());
            return;
        }
        enterPlayer.coin -= price;
        Vector3 ranVec = Vector3.right * Random.Range(-3, 3)
                       + Vector3.forward * Random.Range(-3, 3);
        Instantiate(itemObj[index], itemPos[index].position + ranVec,
                                    itemPos[index].rotation);


    }
    IEnumerator Talk(){
        talkText.text = talkData[1];
        yield return new WaitForSeconds(2f);
        talkText.text = talkData[0];
    }
}
