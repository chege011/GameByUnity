using System.Collections;
using System.Collections.Generic;
using usercmd;
using UnityEngine;
using UI;
using Battle;

namespace Network
{
    public class MsgReceiver : MonoBehaviour
    {
        private float timer;
        private float playTime;
        public float playAnimTime;
        private float showPlayerLeave;

        private bool IsDataReceive;
        private bool IsStartWar;
        private bool IsReceiveResult;
        private bool IsPlayPlayerLeave;

        public GameObject PlayerLeaveObj;
        

        private IProto proto;

        private bool EnemyRun;
        // Update is called once per frame
        void Update()
        {
            if (MsgHandler.IsConnect())
            {
                if (IsDataReceive && BattleManager.IsDataSet)
                {
                    IsDataReceive = false;
                    BattleManager.IsDataSet = false;
                    BattleManager.StartWar();
                    IsStartWar = true;
                }

                MsgHandler.ProcessReceiveMessage(ProcessRecevivePackage);

                timer += Time.deltaTime;
                HeartBeatPack();
            }

            if (IsStartWar && IsReceiveResult)
            {
                IsStartWar = false;
                IsReceiveResult = false;
                ReceiveResult(proto);
            }

            if (BattleManager.IsBattlePlayerAnim)
            {
                playTime += Time.deltaTime;
                if (playTime > playAnimTime)
                {
                    playTime = 0;
                    BattleManager.IsBattlePlayerAnim = false;
                    UIManager.SwitchUI(UIManager.UIState.BattleResult);
                }
            }

            if (EnemyRun)
            {
                if (UIManager.State != UIManager.UIState.MainTitle && UIManager.State != UIManager.UIState.Battle && !BattleManager.IsFinished)
                {
                        BattleManager.IsWin = true;
                        BattleManager.IsFinished = true;
                        EnemyRun = false;
                        IsPlayPlayerLeave = true;
                        PlayerLeaveObj.SetActive(true);
                }
                else if (UIManager.State == UIManager.UIState.MainTitle)
                {
                    EnemyRun = false;
                }
            }

            if (IsPlayPlayerLeave)
            {
                showPlayerLeave += Time.deltaTime;
                if (showPlayerLeave > 3)
                {
                    BattleManager.ClearWar();
                    BattleManager.DestoryActionIme();
                    if (MsgHandler.IsConnect())
                    {
                        UIManager.SwitchUI(UIManager.UIState.MainTitle);
                    }
                    IsPlayPlayerLeave = false;
                    PlayerLeaveObj.SetActive(false);
                    showPlayerLeave = 0;
                }
            }
        }

        void OnApplicationQuit()
        {
            if (MsgHandler.IsConnect())
            {
                MsgHandler.SendMessage((int)MsgType_wzb.deactiv_REQ);
                Debug.Log("Edit Exit");
            }
                
        }

        public void Init()
        {
            timer = 0;
            IsDataReceive = false;
            IsStartWar = false;
            IsReceiveResult = false;
            playTime = 0;
            EnemyRun = false;
            MsgHandler.Regist((int)MsgType_wzb.login_CNF, ReceiveLogin);
            MsgHandler.Regist((int)MsgType_wzb.match_CNF, ReceiveMatch);
            MsgHandler.Regist((int) MsgType_wzb.reserved2, ReceiveReserved2);
            MsgHandler.Regist((int)MsgType_wzb.action, ReceiveAction);
            MsgHandler.Regist((int) MsgType_wzb.reserved3, ReceiveReserved3);
            MsgHandler.Regist((int) MsgType_wzb.deactiv_CNF, ReceiveDeactice);
            MsgHandler.Regist((int) MsgType_wzb.reserved1, ReceiveResvered1);
            //MsgHandler.Regist((int)MsgType_wzb.deactiv_CNF, ReceiveFinishBattle);
        }

        // 用于处理空包
        public void ReceiveResvered1(IProto proto)
        {
            Debuger.Log("ReceiveReserved1");
        }

        // 内部函数用于收到数据包的消息处理
        private void ProcessRecevivePackage(object obj)
        {
            var package = obj as Package;
            if (package != null) MsgHandler.Dispatch(package.Id, package.Body);
        }

        // 收到登陆消息
        public void ReceiveLogin(IProto proto)
        {
            UIManager.SwitchUI(UIManager.UIState.MainTitle);
            Debug.Log("Login success");
            Debuger.Log("Login success");
        }

        // 收到匹配成功消息
        public void ReceiveMatch(IProto proto)
        {
            var isCal = proto.ToObj<ChooseCal>().isCal;
            BattleManager.IsCalculate = isCal;
            UIManager.SwitchUI(UIManager.UIState.RaceChose);
        }

        // 收到用户名种族数据
        public void ReceiveReserved2(IProto proto)
        {
            var gamePlayer = proto.ToObj<GamePlayer>();
            BattleManager.EnemyTroops.name = gamePlayer.username;
            BattleManager.EnemyTroops.race = gamePlayer.isHuman ? BattleManager.Race.Human : BattleManager.Race.Demon;
            Debug.Log("Receive 种族同步数据");
            Debuger.Log("Receive 种族同步数据");
        }

        // 用于同步battle 数据
        public void ReceiveAction(IProto proto)
        {
            var battleData = proto.ToObj<BattleData>();
            foreach (var data in battleData.troops)
            {
                BattleManager.EnemyTroops.troops.Add(data.key, data.num);
            }

            Debug.Log("ReceiveActionPack");
            Debuger.Log("ReceiveActionPack");
            IsDataReceive = true;
        }

        // 用于接受对方客户端的消息，得出消息战报和战斗结果  现在处于战场状态中的等待阶段
        public void ReceiveResult(IProto proto)
        {
            BattleManager.StartAction();

            var battleResult = proto.ToObj<BattleResult>();

            //加载战斗数据  从远端客户端发来的
            BattleManager.BattleRds.RoundNum = battleResult.roundNum;
            BattleManager.IsWin = battleResult.isWin;
            BattleManager.IsFinished = battleResult.isFinished;

            var makeDamageDic = BattleManager.BattleRds.PlayerRecords[battleResult.roundNum].MakeDamage;
            var takeDamageDic = BattleManager.BattleRds.PlayerRecords[battleResult.roundNum].TakeDamage;
            var specialDic = BattleManager.BattleRds.PlayerRecords[battleResult.roundNum].SpecNum;
            foreach (var make in battleResult.makeDamage)
            {
                makeDamageDic[make.key] = make.num;
            }
            foreach (var take in battleResult.takeDamage)
            {
                takeDamageDic[take.key] = take.num;
            }
            foreach (var spec in battleResult.specNum)
            {
                specialDic[spec.key] = spec.num;
            }


            if (battleResult.isWin)
            {
                BattleManager.BattleRds.PlayerWinNum += 1;
            }
            else
            {
                BattleManager.BattleRds.EnemyWinNum += 1;
            }
        }

        public void ReceiveReserved3(IProto proto)
        {
            Debug.Log("Receive reserved3");
            Debuger.Log("Receive reserved3");
            // 在接受到地方传来的战斗数据后开始场景表现
            //BattleManager.StartAction();
            IsReceiveResult = true;
            this.proto = new Proto(proto.ToBytes());
        }

        public void ReceiveDeactice(IProto proto)
        {
            Debug.Log("ReceiveDeactive");
            Debuger.Log("ReceiveDeactive");
            if (!BattleManager.IsFinished)
            {
                EnemyRun = true;
            }
        }

        //public void ReceiveStartBattle(IProto proto)
        //{
        //    UIManager.SwitchUI(UIManager.UIState.Battle);
        //}

        //public void ReceiveFinishBattle(IProto proto)
        //{

        //    UIManager.SwitchUI(UIManager.UIState.MainTitle);
        //}

        // 心跳包
        private void HeartBeatPack()
        {
            if (timer > 5)
            {
                timer = 0;
                MsgHandler.SendMessage((int)MsgType_wzb.reserved1);
                Debuger.Log("Send Beat");
            }
        }
    }
}

