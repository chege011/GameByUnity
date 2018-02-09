using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network;
using usercmd;
using Battle;

namespace UI
{
    public class UIRaceChose : MonoBehaviour
    {
        public void OnClick(bool isHunman)
        {
            UIManager.audioManager.PlayAudioClick();
            var gamePlayer = new GamePlayer
            {
                username = BattleManager.PlayerTroops.name,
                isHuman = isHunman
            };

            BattleManager.PlayerTroops.race = isHunman ? BattleManager.Race.Human : BattleManager.Race.Demon;

            MsgHandler.SendMessage((int)MsgType_wzb.reserved2, gamePlayer);
            UIManager.SwitchUI(UIManager.UIState.FootManChoose);
        }
    }
}

