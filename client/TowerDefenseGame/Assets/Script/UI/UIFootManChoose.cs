using System.Collections;
using System.Collections.Generic;
using Network;
using usercmd;
using UnityEngine.UI;
using Battle;
using UnityEngine;
using _footMan_client_;
using Assets;

namespace UI
{
    public class UIFootManChoose : MonoBehaviour
    {
        // 用到的组件
        public Image ShowImage;
        public Text ShowTotalMoney;
        public Text ShowCost;
        public Text ShowName;
        public Text ShowRank;
        public GameObject FootManPn;

        private Text _showNum;

        private Dictionary<uint, uint> _troops;
        private uint _id;
        private int _money;
        private int _cost;
        private const string PicHuman = "Human/";
        private const string PicDemon = "Demon/";
        private const string ButtonImage = "Button/";
        private const string FullBody = "FullBody/";
        private const string PicShow = "Show/";
        private const string PicPress = "Press/";
        private Dictionary<uint, ButtonData> ButtonText;

        public class ButtonData
        {
            public Text number;
            public Text text;
            public Image pic;
            public Button btn;
            public SpriteState ss;

            public ButtonData(Text number, Text text, Image pic, Button btn)
            {
                this.text = text;
                this.number = number;
                this.pic = pic;
                this.btn = btn;
                this.ss = new SpriteState();
            }
        }

        public void Start()
        {
        }

        public void Update()
        {
        }

        public void Init()
        {
            _troops = new Dictionary<uint, uint>{{1, 0}, {2, 0}, {3, 0}, {4, 0}, {5, 0}, {6, 0}, {7, 0}, {8, 0}, {9, 0}};
            _id = 1;
            _money = 5000;
            ButtonText = new Dictionary<uint, ButtonData>();
            for (uint i = 1; i < 10; i++)
            {
                var temp = FootManPn.transform.Find(i.ToString());
                var data = new ButtonData(
                    temp.Find("number").GetComponent<Text>(), 
                    temp.Find("Text").GetComponent<Text>(), 
                    temp.Find("Image").GetComponent<Image>(),
                    temp.GetComponent<Button>()
                    );
                ButtonText.Add(i, data);
            }
        }

        public void Clear()
        {
            _id = 1;
            _money = 5000;
            _showNum = ButtonText[1].number;
            for (uint i = 1; i < 10; i++)
            {
                _troops[i] = 0;
            }

            InitFootManPn();
            UpdateShowAndData();
        }

        // 按下箭头的操作
        public void OnStart()
        {
            UIManager.audioManager.PlayAudioClick();
            BattleData battleData = new BattleData();
            var keys = _troops.Keys;
            foreach (var key in keys)
            {
                var num = _troops[key];
                if (num <= 0) continue;

                BattleManager.PlayerTroops.troops.Add(key, num);
                var footMan = new Dictionary
                {
                    key = key,
                    num = BattleManager.PlayerTroops.troops[key]
                };
                battleData.troops.Add(footMan);
            }
            MsgHandler.SendMessage((int)MsgType_wzb.action, battleData);
            UIManager.SwitchUI(UIManager.UIState.LoadingWData);
            BattleManager.IsDataSet = true;
        }

        public void OnClickBtn(int num)
        {
            UIManager.audioManager.PlayAudioClick();
            _id = (uint)num;
            UpdateShowAndData();
            _showNum = ButtonText[_id].number;
        }

        public void OnClickAdd()
        {
            UIManager.audioManager.PlayAudioAdd();
            if (_money - _cost >= 0)
            {
                _money -= _cost;
                _troops[_id] += 1;
                ShowTotalMoney.text = _money.ToString();
                _showNum.text = (_troops[_id]).ToString();
            }
        }

        public void OnClickMinus()
        {
            UIManager.audioManager.PlayAudioSub();
            if (_troops[_id] > 0)
            {
                _troops[_id] -= 1;
                _money += _cost;
                ShowTotalMoney.text = _money.ToString();
                _showNum.text = (_troops[_id]).ToString();
            }
        }

        private void UpdateShowAndData()
        {
            LoadImage();
            LoadData();
        }

        private void InitFootManPn()
        {
            var keys = ButtonText.Keys;
            foreach (var key in keys)
            {
                if (BattleManager.PlayerTroops.race == BattleManager.Race.Human)
                {
                    _Human_ footMan;
                    ExcelManager.footMan_Human.TryGetValue(key, out footMan);
                    ButtonText[key].text.text = footMan.name;
                    ButtonText[key].pic.sprite = AssetsManager.LoadImage("Image/" + ButtonImage + PicShow + PicHuman + key);
                    ButtonText[key].ss.highlightedSprite =
                        AssetsManager.LoadImage("Image/" + ButtonImage + PicPress + PicHuman + key);
                    ButtonText[key].ss.pressedSprite =
                        AssetsManager.LoadImage("Image/" + ButtonImage + PicPress + PicHuman + key);
                    ButtonText[key].ss.disabledSprite =
                        AssetsManager.LoadImage("Image/" + ButtonImage + PicShow + PicHuman + key);
                    ButtonText[key].btn.spriteState = ButtonText[key].ss;

                }
                else
                {
                    _Demon_ footMan;
                    ExcelManager.footMan_Demon.TryGetValue(key, out footMan);
                    ButtonText[key].text.text = footMan.name;
                    ButtonText[key].pic.sprite = AssetsManager.LoadImage("Image/" + ButtonImage + PicShow +PicDemon + key);
                    ButtonText[key].ss.highlightedSprite =
                        AssetsManager.LoadImage("Image/" + ButtonImage + PicPress + PicDemon + key);
                    ButtonText[key].ss.pressedSprite =
                        AssetsManager.LoadImage("Image/" + ButtonImage + PicPress + PicDemon + key);
                    ButtonText[key].ss.disabledSprite =
                        AssetsManager.LoadImage("Image/" + ButtonImage + PicShow + PicDemon + key);
                    ButtonText[key].btn.spriteState = ButtonText[key].ss;
                }
                ButtonText[key].number.text = "0";
            }
        }

        private void LoadImage()
        {
            if (BattleManager.PlayerTroops.race == BattleManager.Race.Human)
            {
                ShowImage.sprite = AssetsManager.LoadImage("Image/" + FullBody + PicHuman + _id);
            }
            else
            {
                ShowImage.sprite = AssetsManager.LoadImage("Image/" + FullBody + PicDemon + _id);
            }
            
        }

        private void LoadData()
        {
            if (BattleManager.PlayerTroops.race == BattleManager.Race.Human)
            {
                _Human_ footMan;
                ExcelManager.footMan_Human.TryGetValue(_id, out footMan);
                _cost = (int)footMan.cost;
                ShowCost.text = _cost.ToString();
                ShowName.text = footMan.name;
                ShowRank.text = "等级:" + footMan.level.ToString();
            }
            else
            {
                _Demon_ footMan;
                ExcelManager.footMan_Demon.TryGetValue(_id, out footMan);
                _cost = (int)footMan.cost;
                ShowCost.text = _cost.ToString();
                ShowName.text = footMan.name;
                ShowRank.text = "等级:" + footMan.level.ToString();
            }
            ShowTotalMoney.text = _money.ToString();
        }
    }
}

