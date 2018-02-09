using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;
using UnityEngine.UI;
using Assets;
using _footMan_client_;
using UI;
using Network;
using usercmd;

public class UIBattleResult : MonoBehaviour {

    public Image Bg;
    public Image ShowImage;
    public GameObject Table;
    public Text RoundText;
    public float delTime;
    private Dictionary<uint, GameObject> _tableItems;
    private uint _roundNum;
    private bool isStart;
    private float timer;

	// Use this for initialization
	void Start ()
	{
    }
	
	// Update is called once per frame
	void Update () {
	    if (isStart)
	    {
	        timer -= Time.deltaTime;
	    }

	    if (timer <= 0)
	    {
	        if (BattleManager.IsFinished)
	        {
	            UIManager.SwitchUI(UIManager.UIState.MainTitle);
	            MsgHandler.SendMessage((int)MsgType_wzb.deactiv_REQ);
	            BattleManager.ClearWar();
	        }
	        else
	        {
	            UIManager.SwitchUI(UIManager.UIState.FootManChoose);
            }
	        isStart = false;
	    }
	}

    public void Init()
    {
        Bg.sprite = AssetsManager.LoadImage("Image/bg");
        _tableItems = new Dictionary<uint, GameObject>
        {
            {1, Table.transform.Find("tableItem1").gameObject},
            {2, Table.transform.Find("tableItem2").gameObject},
            {3, Table.transform.Find("tableItem3").gameObject},
            {4, Table.transform.Find("tableItem4").gameObject},
            {5, Table.transform.Find("tableItem5").gameObject},
            {6, Table.transform.Find("tableItem6").gameObject},
            {7, Table.transform.Find("tableItem7").gameObject},
            {8, Table.transform.Find("tableItem8").gameObject},
            {9, Table.transform.Find("tableItem9").gameObject}
        };
        _roundNum = 0;
        isStart = false;
    }

    public void UpdateShow()
    {
        UpdateImage();
        InitTable();
        UpdateTable();
        RoundText.text = _roundNum.ToString();
        ResetTimer();
    }

    private void UpdateImage()
    {
        if (BattleManager.IsWin)
        {
            ShowImage.sprite = AssetsManager.LoadImage("Image/Win");
        }
        else
        {
            ShowImage.sprite = AssetsManager.LoadImage("Image/Lose");
        }
    }

    private void UpdateTable()
    {
        _roundNum = BattleManager.BattleRds.RoundNum;
        var record = BattleManager.BattleRds.PlayerRecords[_roundNum];
        var mkeys = record.MakeDamage.Keys;
        foreach (var key in mkeys)
        {
            _tableItems[key].transform.Find("MD").GetComponent<Text>().text = record.MakeDamage[key].ToString();
            _tableItems[key].transform.Find("TD").GetComponent<Text>().text = record.TakeDamage[key].ToString();
            _tableItems[key].transform.Find("SP").GetComponent<Text>().text = record.SpecNum[key].ToString();
        }
    }

    private void InitTable()
    {
        var initkeys = _tableItems.Keys;
        foreach (var key in initkeys)
        {
            _tableItems[key].transform.Find("MD").GetComponent<Text>().text = "0";
            _tableItems[key].transform.Find("TD").GetComponent<Text>().text = "0";
            _tableItems[key].transform.Find("SP").GetComponent<Text>().text = "0";
        }
        if (BattleManager.PlayerTroops.race == BattleManager.Race.Human)
        {
            var humanList = ExcelManager.footMan_Human;
            var keys = humanList.Keys;

            foreach (var key in keys)
            {
                _Human_ human;
                humanList.TryGetValue(key, out human);
                _tableItems[key].transform.Find("FT").GetComponent<Text>().text = human.name;
            }
        }
        else
        {
            var demonList = ExcelManager.footMan_Demon;
            var keys = demonList.Keys;

            foreach (var key in keys)
            {
                _Demon_ demon;
                demonList.TryGetValue(key, out demon);
                _tableItems[key].transform.Find("FT").GetComponent<Text>().text = demon.name;
            }
        }
    }

    private void ResetTimer()
    {
        timer = delTime;
        isStart = true;
    }
}
