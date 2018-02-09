using System.Collections;
using System.Collections.Generic;
using Battle;
using Network;
using UnityEngine;
using usercmd;
using UnityEngine.UI;

namespace UI
{
    public class UIMainTitle : MonoBehaviour
    {
        public Text Name;

        public void Init()
        {
            Name.text = BattleManager.PlayerTroops.name;
        }

        public void OnMatch()
        {
            UIManager.audioManager.PlayAudioClick();
            MsgHandler.SendMessage((int) MsgType_wzb.match_REQ);
            UIManager.UILoading.GetComponent<UILoading>().SetMainToRace();
            UIManager.SwitchUI(UIManager.UIState.LoadingLoM);
        }

        public void OnLogout()
        {
            UIManager.audioManager.PlayAudioClick();
            MsgHandler.Disconnect();
            UIManager.SwitchUI(UIManager.UIState.Login);
        }

        public void OnExit()
        {
            UIManager.audioManager.PlayAudioClick();
            //MsgHandler.SendMessage((int)MsgType_wzb.match_CNF);
            MsgHandler.Close();
            Application.Quit();
        }
    }
}

