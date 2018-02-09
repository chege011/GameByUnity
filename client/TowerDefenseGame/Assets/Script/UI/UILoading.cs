using System.Collections;
using System.Collections.Generic;
using Network;
using UnityEngine;
using usercmd;
using UnityEngine.UI;
using Battle;

public class UILoading : MonoBehaviour
{

    public Image Image;
    public Text Text;
    public Sprite LoginToMain;
    public Sprite MainToRace;
    public Sprite FtToBattleHuman;
    public Sprite FtToBattleDemon;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void SetLoginToMain()
    {
        Image.sprite = LoginToMain;
        Text.text = "连接服务器......";
    }

    public void SetMainToRace()
    {
        Image.sprite = MainToRace;
        Text.text = "寻找对手......";
    }

    public void SetFtToBattle()
    {
        Image.sprite = BattleManager.PlayerTroops.race == BattleManager.Race.Demon ? FtToBattleDemon : FtToBattleHuman;

        Text.text = "对手出兵......";
    }
}
