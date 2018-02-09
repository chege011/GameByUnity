using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network;
using usercmd;
using UnityEngine.UI;
using Battle;

namespace UI
{

    public class UILogin : MonoBehaviour
    {
        private string IP;
        // 按下登陆按钮时执行的操作
        public void OnClick()
        {
            UIManager.audioManager.PlayAudioClick();
            if (IP != null)
            {
                MsgHandler.StartConnetToServer(IP);
            }
            else
            {
                MsgHandler.StartConnetToServer();
            }

            MsgHandler.SendMessage((int)MsgType_wzb.login_REQ);
            Debug.Log("Login_REQ send");
            UIManager.UIMainTitle.GetComponent<UIMainTitle>().Init();
            UIManager.UILoading.GetComponent<UILoading>().SetLoginToMain();
            UIManager.SwitchUI(UIManager.UIState.LoadingLoM);
        }

        public void OnEndEditor(InputField input)
        {
            if (input.text.Length > 0)
            {
                BattleManager.PlayerTroops.name = input.text;
            }
            else
            {
                BattleManager.PlayerTroops.name = "playerNoName";
            }
        }

        public void OnEndEditorIP(InputField input)
        {
            if (input.text.Length > 0)
            {
                IP = input.text;
            }
        }
    }
}

