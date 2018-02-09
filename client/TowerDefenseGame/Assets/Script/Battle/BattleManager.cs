using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _footMan_client_;
using Assets;
using Network;
using UI;
using usercmd;

namespace Battle{
    public static class BattleManager
    {
        public enum Race
        {
            Human,
            Demon
        }

        public class FootManTroops
        {
            public string name;
            public Race race;
            public Dictionary<uint, uint> troops;
            //public Dictionary<uint, BasicAttr> attrs;

        }

        public class RoundRecords
        {
            public Dictionary<uint, uint> MakeDamage;
            public Dictionary<uint, uint> TakeDamage;
            public Dictionary<uint, uint> SpecNum;

            public RoundRecords()
            {
                MakeDamage = new Dictionary<uint, uint>();
                TakeDamage = new Dictionary<uint, uint>();
                SpecNum = new Dictionary<uint, uint>();
            }
        }

        public class BattleRecords
        {
            public uint RoundNum;
            public uint PlayerWinNum;
            public uint EnemyWinNum;
            public Dictionary<uint, RoundRecords> PlayerRecords;
            public Dictionary<uint, RoundRecords> EnemyRecords;

            public void ClearRecord()
            {
                RoundNum = 0;
                PlayerWinNum = 0;
                EnemyWinNum = 0;
                PlayerRecords.Clear();
                EnemyRecords.Clear();
            }
        }

        //public class BasicAttr
        //{
        //    public uint attck;
        //    public uint defence;
        //    public uint heart;
        //    public uint speed;
        //    public uint level;
        //    public uint spec;
        //    public uint chDef;
        //    public uint chDefVal;
        //    public uint chAtk;
        //    public uint chAtkVal;
        //    public uint extraDmg;
        //    public uint extraDmgVal;
        //    public  BasicAttr()
        //    {
        //        attck=0;
        //        defence=0;
        //        heart=0;
        //        speed=0;
        //        level=0;
        //        spec=0;
        //        chDef=0;
        //        chDefVal=0;
        //        chAtk=0;
        //        chAtkVal=0;
        //        extraDmg=0;
        //        extraDmgVal=0;
        //    }
        //}

        public static bool IsCalculate; // 使用随机数来随机选择一个客户端结算
        public static bool IsWin; //战斗是否胜利
        public static bool IsFinished;
        public static bool IsDataSet;
        public static bool IsBattlePlayerAnim;

        public static BattleRecords BattleRds;
        public static Dictionary<uint, GameObject> HumanPrefabs { get; private set; }
        public static Dictionary<uint, GameObject> DemonPrefabs { get; private set; }


        public static FootManTroops PlayerTroops { get; private set; }
        private static List<GameObject> _playerObjTroops;

        public static FootManTroops EnemyTroops { get; private set; }
        private static List<GameObject> _enemyObjTroops;

        public static void Init()
        {
            PlayerTroops = new FootManTroops();
            EnemyTroops = new FootManTroops();
            PlayerTroops.troops = new Dictionary<uint, uint>();
            EnemyTroops.troops = new Dictionary<uint, uint>();
            //PlayerTroops.attrs = new Dictionary<uint, BasicAttr>();
            //EnemyTroops.attrs = new Dictionary<uint, BasicAttr>();
            _playerObjTroops = new List<GameObject>();
            _enemyObjTroops = new List<GameObject>();
            HumanPrefabs = new Dictionary<uint, GameObject>();
            DemonPrefabs = new Dictionary<uint, GameObject>();
            IsCalculate = false;
            IsWin = false;
            IsDataSet = false;
            IsBattlePlayerAnim = false;
            BattleRds = new BattleRecords {
                RoundNum = 0,
                PlayerWinNum = 0,
                EnemyWinNum = 0,
                PlayerRecords = new Dictionary<uint, RoundRecords>(),
                EnemyRecords = new Dictionary<uint, RoundRecords>()
            };

            PreloadPrefabs();
        }

        public static void ClearWar()
        {
            IsCalculate = false;
            IsWin = false;
            IsDataSet = false;
            BattleRds.ClearRecord();
        }

        public static void ClearBattle()
        {
            PlayerTroops.troops.Clear();
            EnemyTroops.troops.Clear();
            //PlayerTroops.attrs.Clear();
            //EnemyTroops.attrs.Clear();
        }

        // 总体战场逻辑
        public static void StartWar()
        {
            BattleRds.RoundNum += 1;

            // 设置表现部分
            InstantiatePlayerAndEnemy();
            InitPlayerAndEnemyPDR();
            SetAllActive();

            // 切换到战场
            UIManager.SwitchUI(UIManager.UIState.Battle);

            // 初始化战斗LOG
            InitBattleRecord();

            // 不计算的客户端逻辑在MsgReceiver 中的 ReceiveReserved3 中
            if (!IsCalculate) { ClearBattle(); return;}

            // 开始逻辑计算
            StartBattle(out IsWin);

            // 判断输赢
            if (IsWin)
            {
                BattleRds.PlayerWinNum += 1;

            }
            else
            {
                BattleRds.EnemyWinNum += 1;
            }

            IsFinished = IsBattleFinish();

            // 开始战斗表现
            StartAction();

            // 发送本次战斗信息给对方
            var battleResult = new BattleResult
            {
                roundNum = BattleRds.RoundNum,
                isWin = !IsWin,
                isFinished = IsFinished

            };

            var keys = BattleRds.EnemyRecords[BattleRds.RoundNum].MakeDamage.Keys;
            var makeDamageDic = BattleRds.EnemyRecords[BattleRds.RoundNum].MakeDamage;
            var takeDamageDic = BattleRds.EnemyRecords[BattleRds.RoundNum].TakeDamage;
            var specNumDic = BattleRds.EnemyRecords[BattleRds.RoundNum].SpecNum;
            foreach (var key in keys)
            {
                var make = new Dictionary
                {
                    key = key,
                    num = makeDamageDic[key]
                };

                var take = new Dictionary
                {
                    key = key,
                    num = takeDamageDic[key]
                };

                var spec = new Dictionary
                {
                    key = key,
                    num = specNumDic[key]
                };

                battleResult.makeDamage.Add(make);
                battleResult.takeDamage.Add(take);
                battleResult.specNum.Add(spec);
            }

            MsgHandler.SendMessage((int)MsgType_wzb.reserved3, battleResult);

            ClearBattle();
        }

        // 表现部分

        private static void InstantiatePlayerAndEnemy()
        {
            // player 部分
            InstantiateObj(PlayerTroops, _playerObjTroops);

            // enemy 部分
            InstantiateObj(EnemyTroops, _enemyObjTroops);
        }

        private static void InitPlayerAndEnemyPDR()
        {
            int plNum = 0;
            int neNum = 0;

            foreach (var playerFT in _playerObjTroops)
            {
                var footMan = playerFT.GetComponent<FootMan>();
                footMan.InitPDR(new Vector3((-5 - plNum / 5) * 10, 0, (-2 + plNum % 5) * 10), new Vector3(10, 0, 0), new Vector3(0, 90, 0));
                footMan.gameObject.transform.localScale = new Vector3(10, 10, 10);
                footMan.ActionTime = 5;
                footMan.centerPoint = new Vector3(0, 0, 0);
                footMan.deleteTime = 10;
                plNum++;
            }

            foreach (var enemyFT in _enemyObjTroops)
            {
                var footMan = enemyFT.GetComponent<FootMan>();
                footMan.InitPDR(new Vector3((5 + neNum / 5) * 10, 0, (-2 + neNum % 5) * 10), new Vector3(-10, 0, 0), new Vector3(0, -90, 0));
                footMan.gameObject.transform.localScale = new Vector3(10, 10, 10);
                footMan.ActionTime = 5;
                footMan.centerPoint = new Vector3(0, 0, 0);
                footMan.deleteTime = 10;
                neNum++;
            }
        }

        public static void DestoryActionIme()
        {
            if (_playerObjTroops != null && _enemyObjTroops != null)
            {
                foreach (var playerFT in _playerObjTroops)
                {
                    playerFT.GetComponent<FootMan>().DestroyIm();
                }

                foreach (var enemyFT in _enemyObjTroops)
                {
                    enemyFT.GetComponent<FootMan>().DestroyIm();
                }
            }
        }

        private static void SetAllActive()
        {
            foreach (var playerFT in _playerObjTroops)
            {
                playerFT.SetActive(true);
            }

            foreach (var enemyFT in _enemyObjTroops)
            {
                enemyFT.SetActive(true);
            }
        }

        public static void StartAction()
        {
            foreach (var playerFT in _playerObjTroops)
            {
                var footMan = playerFT.GetComponent<FootMan>();
                footMan.StartAction();
            }

            foreach (var enemyFT in _enemyObjTroops)
            {
                var footMan = enemyFT.GetComponent<FootMan>();
                footMan.StartAction();
            }

            IsBattlePlayerAnim = true;

            _playerObjTroops.Clear();
            _enemyObjTroops.Clear();

        }

        private static void PreloadPrefabs()
        {
            var keysHuman = ExcelManager.footMan_Human.Keys;
            foreach (var key in keysHuman)
            {
                _Human_ footMan;
                ExcelManager.footMan_Human.TryGetValue(key, out footMan);
                string path = footMan.path;

                HumanPrefabs.Add(key, AssetsManager.LoadAsset(path, false));
            }

            var keysDemon = ExcelManager.footMan_Demon.Keys;
            foreach (var key in keysDemon)
            {
                _Demon_ footMan;
                ExcelManager.footMan_Demon.TryGetValue(key, out footMan);
                string path = footMan.path;

                DemonPrefabs.Add(key, AssetsManager.LoadAsset(path, false));
            }

        }

        private static void InstantiateObj(FootManTroops troops, List<GameObject> list)
        {
            var dic = troops.troops;
            var keys = dic.Keys;

            if (troops.race == Race.Human)
            {
                foreach (var key in keys)
                {
                    uint num;
                    dic.TryGetValue(key, out num);

                    GameObject footMan;
                    HumanPrefabs.TryGetValue(key, out footMan);

                    for (var i = 0; i < num; i++)
                    {
                        var gameObj = GameObject.Instantiate(footMan);
                        gameObj.GetComponent<FootMan>().Init();
                        gameObj.SetActive(false);
                        list.Add(gameObj);
                    }
                }
            }
            else
            {
                foreach (var key in keys)
                {
                    uint num;
                    dic.TryGetValue(key, out num);

                    GameObject footMan;
                    DemonPrefabs.TryGetValue(key, out footMan);

                    for (var i = 0; i < num; i++)
                    {
                        var gameObj = GameObject.Instantiate(footMan);
                        gameObj.GetComponent<FootMan>().Init();
                        gameObj.SetActive(false);
                        list.Add(gameObj);
                    }
                }
            }
        }

        private static _Human_ CopyHumanData(_Human_ res)
        {
            _Human_ human = new _Human_
            {
                attack = res.attack,
                chAtk = res.chAtk,
                chAtkVal = res.chAtkVal,
                chDef = res.chDef,
                chDefVal = res.chDefVal,
                cost = res.cost,
                defence = res.defence,
                extraDmg = res.extraDmg,
                extraDmgVal = res.extraDmgVal,
                heart = res.heart,
                id = res.id,
                level = res.level,
                maxNum = res.maxNum,
                name = res.name,
                spec = res.spec,
                speed = res.speed
            };

            return human;
        }

        private static _Demon_ CopyDemonData(_Demon_ res)
        {
            _Demon_ demon = new _Demon_
            {
                attack = res.attack,
                chAtk = res.chAtk,
                chAtkVal = res.chAtkVal,
                chDef = res.chDef,
                chDefVal = res.chDefVal,
                cost = res.cost,
                defence = res.defence,
                extraDmg = res.extraDmg,
                extraDmgVal = res.extraDmgVal,
                heart = res.heart,
                id = res.id,
                level = res.level,
                maxNum = res.maxNum,
                name = res.name,
                spec = res.spec,
                speed = res.speed
            };

            return demon;
        }

        // 逻辑部分
        private static void StartBattle(out bool PlayerVictory)
        {
            /*****
             BattleRDS.playerRecords[BattleRDS.roundNum].makeDamage[1] += 2;
             //使用BattleRDS 
             roundNum 是用来表示现在是第几回合
             playerRecords 里存的是玩家的记录
             enemyRecords 里存的是敌人的记录
            /*** */
            List<_Human_> troopsHuman = new List<_Human_>();//玩家自己-人族
            List<_Demon_> troopsDemon = new List<_Demon_>();//玩家自己-恶魔
            List<_Human_> enemyHuman = new List<_Human_>();//敌人-人族
            List<_Demon_> enemyDemon = new List<_Demon_>();//敌人-恶魔

            List<_Human_> troopsHumancocopy = new List<_Human_>();//玩家自己-人族
            List<_Demon_> troopsDemoncopy = new List<_Demon_>();//玩家自己-恶魔
            List<_Human_> enemyHumancopy = new List<_Human_>();//敌人-人族
            List<_Demon_> enemyDemoncopy = new List<_Demon_>();//敌人-恶魔
            int playerhuman = 0, playerdemo = 0, enemyhuman = 0, enemydemo = 0;
            string BattleReport = "";//战报
            //玩家小兵初始化
            if (PlayerTroops.race == Race.Human)
            {
                playerhuman = 1;
                var keys = PlayerTroops.troops.Keys;
                foreach (var key in keys)
                {
                    _Human_ HumanMan;
                    
                    ExcelManager.footMan_Human.TryGetValue(key, out HumanMan);
                    for (var i = 0; i < PlayerTroops.troops[key]; i++)
                    { 
                        troopsHuman.Add(CopyHumanData(HumanMan));
                        troopsHumancocopy.Add(CopyHumanData(HumanMan));
                    }

                }
            }
            else
            {
                playerdemo = 1;
                var keys = PlayerTroops.troops.Keys;
                foreach (var key in keys)
                {
                    _Demon_ DemonMan;
                    ExcelManager.footMan_Demon.TryGetValue(key, out DemonMan);
                    for (var i = 0; i < PlayerTroops.troops[key]; i++)
                    {
                        troopsDemon.Add(CopyDemonData(DemonMan));
                        troopsDemoncopy.Add(CopyDemonData(DemonMan));
                    }
                        
                }
             }
            //敌人小兵初始化
            if (EnemyTroops.race == Race.Human)
            {
                enemyhuman = 1;
                var keys = EnemyTroops.troops.Keys;
                foreach (var key in keys)
                {
                    _Human_ HumanMan;
                    ExcelManager.footMan_Human.TryGetValue(key, out HumanMan);
                    for (var i = 0; i < EnemyTroops.troops[key]; i++)
                    {
                        enemyHuman.Add(CopyHumanData(HumanMan));
                        enemyHumancopy.Add(CopyHumanData(HumanMan));
                    }
                 }
            }
            else
            {
                enemydemo = 1;
                var keys = EnemyTroops.troops.Keys;
                foreach (var key in keys)
                {
                    _Demon_ DemonMan;
                    ExcelManager.footMan_Demon.TryGetValue(key, out DemonMan);
                    for (var i = 0; i < EnemyTroops.troops[key]; i++)
                    {
                        enemyDemon.Add(CopyDemonData(DemonMan));
                        enemyDemoncopy.Add(CopyDemonData(DemonMan));
                    }
                       
                }
             }
            if (playerhuman == 1 && enemyhuman == 1)
            {//都是人族
                
                while (true)
                {
                    if(troopsHuman.Count > 0 && enemyHuman.Count > 0)
                    {
                        troopsHuman.Sort(new HU_BattleSpeed_DES());
                        troopsHumancocopy.Sort(new HU_BattleSpeed_DES());
                        enemyHuman.Sort(new HU_BattleDefence_ASC());
                        enemyHumancopy.Sort(new HU_BattleDefence_ASC());
                        Human_A_Human(BattleRds.PlayerRecords[BattleRds.RoundNum], BattleRds.EnemyRecords[BattleRds.RoundNum], troopsHuman, troopsHumancocopy, enemyHuman, enemyHumancopy, ref BattleReport);
                    }
                    else
                    {
                        if (troopsHuman.Count >= enemyHuman.Count)
                        {//玩家胜利
                            PlayerVictory = true;
                        }
                        else
                        {//敌方获胜
                            PlayerVictory = false;
                        }
                        break;
                    }
                    if (troopsHuman.Count > 0 && enemyHuman.Count > 0)
                    {
  
                        enemyHuman.Sort(new HU_BattleSpeed_DES());
                        enemyHumancopy.Sort(new HU_BattleSpeed_DES());
                        troopsHuman.Sort(new HU_BattleDefence_ASC());
                        troopsHumancocopy.Sort(new HU_BattleDefence_ASC());
                        Human_A_Human(BattleRds.EnemyRecords[BattleRds.RoundNum], BattleRds.PlayerRecords[BattleRds.RoundNum], enemyHuman, enemyHumancopy, troopsHuman, troopsHumancocopy,  ref BattleReport);
                    }
                    else
                    {
                        if (troopsHuman.Count > enemyHuman.Count)
                        {//玩家胜利
                            PlayerVictory = true;
                        }
                        else
                        {//敌方获胜
                            PlayerVictory = false;
                        }
                        break;
                    }

                }
            }
            else if (playerdemo == 1 && enemydemo == 1)
            {//都是恶魔

                while (true)
                {
                    if (troopsDemon.Count > 0 && enemyDemon.Count > 0)
                    {
                        troopsDemon.Sort(new DE_BattleSpeed_DES());
                        troopsDemoncopy.Sort(new DE_BattleSpeed_DES());
                        enemyDemon.Sort(new DE_BattleDefence_ASC());
                        enemyDemoncopy.Sort(new DE_BattleDefence_ASC());
                        Demon_A_Demon(BattleRds.PlayerRecords[BattleRds.RoundNum],BattleRds.EnemyRecords[BattleRds.RoundNum],troopsDemon, troopsDemoncopy, enemyDemon, enemyDemoncopy, ref BattleReport);
                    }
                    else
                    {
                        if (troopsDemon.Count >= enemyDemon.Count)
                        {     //玩家胜利
                            PlayerVictory = true;
                        }
                        else
                        {    //敌方获胜
                            PlayerVictory = false;
                        }
                        break;
                    }
                    if (troopsDemon.Count > 0 && enemyDemon.Count > 0)
                    {
                        enemyDemon.Sort(new DE_BattleSpeed_DES());
                        enemyDemoncopy.Sort(new DE_BattleSpeed_DES());
                        troopsDemon.Sort(new DE_BattleDefence_ASC());
                        troopsDemoncopy.Sort(new DE_BattleDefence_ASC());
 
                        Demon_A_Demon(BattleRds.EnemyRecords[BattleRds.RoundNum], BattleRds.PlayerRecords[BattleRds.RoundNum], enemyDemon, enemyDemoncopy, troopsDemon, troopsDemoncopy,  ref BattleReport);
                    }
                    else
                    {
                        if (troopsDemon.Count > enemyDemon.Count)
                        {     //玩家胜利
                            PlayerVictory = true;
                        }
                        else
                        {    //敌方获胜
                            PlayerVictory = false;
                        }
                        break;
                    }
                }
            }
            else if (playerhuman == 1 && enemydemo == 1)
            {//玩家人族，敌方恶魔
                
                while (true)
                {
                    if(troopsHuman.Count > 0 && enemyDemon.Count > 0)
                    {
                        troopsHuman.Sort(new HU_BattleSpeed_DES());
                        troopsHumancocopy.Sort(new HU_BattleSpeed_DES());
                        enemyDemon.Sort(new DE_BattleDefence_ASC());
                        enemyDemoncopy.Sort(new DE_BattleDefence_ASC());
                        Human_A_Demon(BattleRds.PlayerRecords[BattleRds.RoundNum], BattleRds.EnemyRecords[BattleRds.RoundNum], troopsHuman, troopsHumancocopy, enemyDemon, enemyDemoncopy, ref BattleReport);
                    }
                    else
                    {
                        if (troopsHuman.Count >= enemyDemon.Count)
                        {//玩家胜利
                            PlayerVictory = true;
                        }
                        else
                        {//敌方获胜
                            PlayerVictory = false;
                        }
                        break;
                    }
                    if (troopsHuman.Count > 0 && enemyDemon.Count > 0)
                    {
                        enemyDemon.Sort(new DE_BattleSpeed_DES());
                        enemyDemoncopy.Sort(new DE_BattleSpeed_DES());
                        troopsHuman.Sort(new HU_BattleDefence_ASC());
                        troopsHumancocopy.Sort(new HU_BattleDefence_ASC());
                        Demon_A_Human(BattleRds.EnemyRecords[BattleRds.RoundNum], BattleRds.PlayerRecords[BattleRds.RoundNum], enemyDemon, enemyDemoncopy, troopsHuman, troopsHumancocopy,  ref BattleReport);
                    }
                    else
                    {
                        if (troopsHuman.Count > enemyDemon.Count)
                        {//玩家胜利
                            PlayerVictory = true;
                        }
                        else
                        {//敌方获胜
                            PlayerVictory = false;
                        }
                        break;
                    }

                }
                
            }
            else
            {//玩家恶魔，敌方人族
               
                while (true)
                {//BattleRds.EnemyRecords[BattleRds.RoundNum]
                    if (troopsDemon.Count > 0 && enemyHuman.Count > 0)
                    {
                        troopsDemon.Sort(new DE_BattleSpeed_DES());
                        troopsDemoncopy.Sort(new DE_BattleSpeed_DES());
                        enemyHuman.Sort(new HU_BattleDefence_ASC());
                        enemyHumancopy.Sort(new HU_BattleDefence_ASC());
                        Demon_A_Human(BattleRds.PlayerRecords[BattleRds.RoundNum], BattleRds.EnemyRecords[BattleRds.RoundNum], troopsDemon, troopsDemoncopy, enemyHuman, enemyHumancopy, ref BattleReport);
                    }
                    else
                    {
                        if (troopsDemon.Count >= enemyHuman.Count)
                        { //玩家胜利
                            PlayerVictory = true;
                        }
                        else
                        {//敌方获胜
                            PlayerVictory = false;
                        }
                        break;
                    }
                    if (troopsDemon.Count > 0 && enemyHuman.Count > 0)
                    {
                        enemyHuman.Sort(new HU_BattleSpeed_DES());
                        enemyHumancopy.Sort(new HU_BattleSpeed_DES());
                        troopsDemon.Sort(new DE_BattleDefence_ASC());
                        troopsDemoncopy.Sort(new DE_BattleDefence_ASC());
                        Human_A_Demon(BattleRds.EnemyRecords[BattleRds.RoundNum], BattleRds.PlayerRecords[BattleRds.RoundNum], enemyHuman, enemyHumancopy, troopsDemon, troopsDemoncopy,  ref BattleReport);
                    }
                    else
                    {
                        if (troopsDemon.Count > enemyHuman.Count)
                        { //玩家胜利
                            PlayerVictory = true;
                        }
                        else
                        {//敌方获胜
                            PlayerVictory = false;
                        }
                        break;
                    }
                }
                
            }
        }

        static void Human_A_Demon(RoundRecords activeRd,RoundRecords passiveRd,List<_Human_> active, List<_Human_> activecopy, List<_Demon_> passive, List<_Demon_> passivecopy, ref string battlestr)
        {
        
            //A主动攻击P，对P造成的伤害
            int a_to_p_harm = (int)(active[0].attack * (1 - (4.0 * passive[0].defence) / (8 * passive[0].defence + 50)));
            //P被动反击，对A造成的伤害
            int p_to_a_harm = (int)(passive[0].attack * (1 - (4.0 * active[0].defence) / (8 * active[0].defence + 50)));

            if (passive[0].heart > (uint)a_to_p_harm)
            {
                passive[0].heart = passive[0].heart - (uint)a_to_p_harm;
                passivecopy[0].heart = passivecopy[0].heart-(uint)a_to_p_harm;//维持下一环节
            }
            else
            {
                passive[0].heart = 0;
                passivecopy[0].heart = 0;
            }
            //passive[0].heart -= (uint)a_to_p_harm;
            // passivecopy[0].heart -= (uint)a_to_p_harm;//维持下一环节
            if (active[0].heart > (uint)p_to_a_harm)
            {
                active[0].heart = active[0].heart - (uint)p_to_a_harm;
                activecopy[0].heart = activecopy[0].heart - (uint)p_to_a_harm;
            }
            else
            {
                active[0].heart = 0;
                activecopy[0].heart = 0;
            }
            activeRd.MakeDamage[active[0].id] += (uint)a_to_p_harm;
            activeRd.TakeDamage[active[0].id] += (uint)p_to_a_harm;
            //enemy battle report
            passiveRd.MakeDamage[passive[0].id] += (uint)p_to_a_harm;
            passiveRd.TakeDamage[passive[0].id] += (uint)a_to_p_harm;
           // battlestr += PlayerTroops.name + " " + active[0].name + " 攻击 " + EnemyTroops.name + " " + passive[0].name + " 造成" + a_to_p_harm.ToString() + "伤害 \n";
           // battlestr += EnemyTroops.name + " " + passive[0].name + " 反击 " + PlayerTroops.name + " " + active[0].name + " 造成" + p_to_a_harm.ToString() + "伤害 \n";
            if (passive[0].heart <= 0)
            {//该小兵死亡
                //battlestr += "本次攻击" + EnemyTroops.name + "死亡一个" + passive[0].name + "死亡";
                passive.Remove(passive[0]);
                passivecopy.Remove(passivecopy[0]);

            }
            if (active[0].heart <= 0)
            {//该小兵死亡
                //battlestr += "本次被反击" + PlayerTroops.name + "死亡一个" + active[0].name + "死亡";
                active.Remove(active[0]);
                activecopy.Remove(activecopy[0]);
            }
            else
            {
                if (active[0].spec == true &&  passive.Count>0)
                {
                    System.Random rand = new System.Random();
                    if (rand.Next(1, 100) < active[0].extraDmg)
                    {
                        int id = rand.Next(0, passive.Count - 1);
                        if (passive[id].heart > active[0].extraDmgVal)
                        {
                            passive[id].heart -= active[0].extraDmgVal;
                        }
                        else
                        {
                            passive[id].heart = 0;
                        }
                        //player battle report
                        activeRd.MakeDamage[active[0].id] += active[0].extraDmgVal;
                        //enemy battle report
                        passiveRd.TakeDamage[passive[0].id] += active[0].extraDmgVal;
                        //计算额外伤害
                        activeRd.SpecNum[active[0].id] += 1;
                        //battlestr += PlayerTroops.name + " " + active[0].name + " 对 " + EnemyTroops.name + " " + passive[id].name + " 造成" + active[0].extraDmgVal.ToString() + "额外伤害 \n";
                        if (passive[id].heart <= 0)
                        {//该小兵死亡
                           // battlestr += PlayerTroops.name +"本次额外伤害造成 " + EnemyTroops.name + "死亡一个" + passive[id].name ;
                            passive.Remove(passive[id]);
                            passivecopy.Remove(passivecopy[0]);

                        }
                    }
                }
                active.Clear();
                for (var i = 0; i < activecopy.Count; i++)
                {
                    active.Add(CopyHumanData(activecopy[i]));
                }
                passive.Clear();
                for (var i = 0; i < passivecopy.Count; i++)
                {
                    passive.Add(CopyDemonData(passivecopy[i]));
                }
                if (active[0].spec == true)
                {
                    uint def, atk;
                    System.Random rand = new System.Random();
                    if (rand.Next(1, 100) < active[0].chDef)
                    {
                        def = (uint)(active[0].chDefVal / 100.0 * active[0].defence);
                        active[0].defence = def;
                        //计算防御特效
                        activeRd.SpecNum[active[0].id] += 1;
                    }

                    if (rand.Next(1, 100) < active[0].chAtk)
                    {
                        atk = (uint)(active[0].chAtkVal / 100.0 * active[0].attack);
                        active[0].attack = atk;
                        //计算攻击特效
                        activeRd.SpecNum[active[0].id] += 1;
                    }
                }
            }
           
        }
        static void Demon_A_Human(RoundRecords activeRd, RoundRecords passiveRd, List<_Demon_> active, List<_Demon_> activecopy, List<_Human_> passive, List<_Human_> passivecopy, ref string battlestr)
        {
            //A主动攻击P，对P造成的伤害
            int a_to_p_harm = (int)(active[0].attack * (1 - (4.0 * passive[0].defence) / (8 * passive[0].defence + 50)));
            //P被动反击，对A造成的伤害
            int p_to_a_harm = (int)(passive[0].attack * (1 - (4.0 * active[0].defence) / (8 * active[0].defence + 50)));

            if (passive[0].heart > (uint)a_to_p_harm)
            {
                passive[0].heart = passive[0].heart - (uint)a_to_p_harm;
                passivecopy[0].heart -= (uint)a_to_p_harm;//维持下一环节
            }
            else
            {
                passive[0].heart = 0;
                passivecopy[0].heart = 0;
            }
            //passive[0].heart -= (uint)a_to_p_harm;
            // passivecopy[0].heart -= (uint)a_to_p_harm;//维持下一环节
            if (active[0].heart > (uint)p_to_a_harm)
            {
                active[0].heart -= (uint)p_to_a_harm;
                activecopy[0].heart -= (uint)p_to_a_harm;
            }
            else
            {
                active[0].heart = 0;
                activecopy[0].heart = 0;
            }
            activeRd.MakeDamage[active[0].id] += (uint)a_to_p_harm;
            activeRd.TakeDamage[active[0].id] += (uint)p_to_a_harm;
            //enemy battle report
            passiveRd.MakeDamage[passive[0].id] += (uint)p_to_a_harm;
            passiveRd.TakeDamage[passive[0].id] += (uint)a_to_p_harm;
            //battlestr += PlayerTroops.name + " " + active[0].name + " 攻击 " + EnemyTroops.name + " " + passive[0].name + " 造成" + a_to_p_harm.ToString() + "伤害 \n";
            //battlestr += EnemyTroops.name + " " + passive[0].name + " 反击 " + PlayerTroops.name + " " + active[0].name + " 造成" + p_to_a_harm.ToString() + "伤害 \n";
            if (passive[0].heart <= 0)
            {//该小兵死亡
                //battlestr += "本次攻击" + EnemyTroops.name + "死亡一个" + passive[0].name + "死亡";
                passive.Remove(passive[0]);
                passivecopy.Remove(passivecopy[0]);

            }
            if (active[0].heart <= 0)
            {//该小兵死亡
                //battlestr += "本次被反击" + PlayerTroops.name + "死亡一个" + active[0].name + "死亡";
                active.Remove(active[0]);
                activecopy.Remove(activecopy[0]);
            }
           else
            {
                if (active[0].spec == true && passive.Count > 0)
                {
                    System.Random rand = new System.Random();
                    if (rand.Next(1, 100) < active[0].extraDmg)
                    {
                        int id = rand.Next(0, passive.Count - 1);
                        if (passive[id].heart > active[0].extraDmgVal)
                        {
                            passive[id].heart -= active[0].extraDmgVal;
                        }
                        else
                        {
                            passive[id].heart = 0;
                        }
                        //player battle report
                        activeRd.MakeDamage[active[0].id] += active[0].extraDmgVal;
                        //enemy battle report
                        passiveRd.TakeDamage[passive[0].id] += active[0].extraDmgVal;
                        //计算额外伤害
                        activeRd.SpecNum[active[0].id] += 1;
                        //battlestr += PlayerTroops.name + " " + active[0].name + " 对 " + EnemyTroops.name + " " + passive[id].name + " 造成" + active[0].extraDmgVal.ToString() + "额外伤害 \n";
                        if (passive[id].heart <= 0)
                        {//该小兵死亡
                           // battlestr += PlayerTroops.name + "本次额外伤害造成 " + EnemyTroops.name + "死亡一个" + passive[id].name;
                            passive.Remove(passive[id]);
                            passivecopy.Remove(passivecopy[0]);

                        }
                    }
                }
                active.Clear();
                for (var i = 0; i < activecopy.Count; i++)
                {
                    active.Add(CopyDemonData(activecopy[i]));
                }
                passive.Clear();
                for (var i = 0; i < passivecopy.Count; i++)
                {
                    passive.Add(CopyHumanData(passivecopy[i]));
                }
                if (active[0].spec == true)
                {
                    uint def, atk;
                    System.Random rand = new System.Random();
                    if (rand.Next(1, 100) < active[0].chDef)
                    {
                        def = (uint)(active[0].chDefVal / 100.0 * active[0].defence);
                        active[0].defence = def;
                        //计算防御特效
                        activeRd.SpecNum[active[0].id] += 1;
                    }

                    if (rand.Next(1, 100) < active[0].chAtk)
                    {
                        atk = (uint)(active[0].chAtkVal / 100.0 * active[0].attack);
                        active[0].attack = atk;
                        //计算攻击特效
                        activeRd.SpecNum[active[0].id] += 1;
                    }
                }
            }
        }
        static void Human_A_Human(RoundRecords activeRd, RoundRecords passiveRd, List<_Human_> active, List<_Human_> activecopy, List<_Human_> passive, List<_Human_> passivecopy, ref string battlestr)
        {
            //A主动攻击P，对P造成的伤害
            int a_to_p_harm = (int)(active[0].attack * (1 - (4.0 * passive[0].defence) / (8 * passive[0].defence + 50)));
            //P被动反击，对A造成的伤害
            int p_to_a_harm = (int)(passive[0].attack * (1 - (4.0 * active[0].defence) / (8 * active[0].defence + 50)));
            if (passive[0].heart > (uint)a_to_p_harm)
            {
                passive[0].heart= passive[0].heart- (uint)a_to_p_harm;
                passivecopy[0].heart -= (uint)a_to_p_harm;//维持下一环节
            }
            else
            {
                passive[0].heart = 0;
                passivecopy[0].heart = 0;
            }
            //passive[0].heart -= (uint)a_to_p_harm;
            // passivecopy[0].heart -= (uint)a_to_p_harm;//维持下一环节
            if (active[0].heart > (uint)p_to_a_harm)
            {
                active[0].heart -= (uint)p_to_a_harm;
                activecopy[0].heart -= (uint)p_to_a_harm;
            }
            else
            {
                active[0].heart =0;
                activecopy[0].heart =0;
            }
            //active[0].heart -= (uint)p_to_a_harm;
           // activecopy[0].heart -= (uint)p_to_a_harm;
            //player battle report
            activeRd.MakeDamage[active[0].id] += (uint)a_to_p_harm;
            activeRd.TakeDamage[active[0].id] += (uint)p_to_a_harm;
            //enemy battle report
            passiveRd.MakeDamage[passive[0].id] += (uint)p_to_a_harm;
            passiveRd.TakeDamage[passive[0].id] += (uint)a_to_p_harm;
            /*********
            battlestr += PlayerTroops.name + " " + active[0].name + " 攻击 " + EnemyTroops.name + " " + passive[0].name + " 造成" + a_to_p_harm.ToString() + "伤害 \n";
            battlestr += EnemyTroops.name + " " + passive[0].name + " 反击 " + PlayerTroops.name + " " + active[0].name + " 造成" + p_to_a_harm.ToString() + "伤害 \n";
            /*********/
            if (passive[0].heart <= 0)
            {//该小兵死亡
                //battlestr += "本次攻击" + EnemyTroops.name + "死亡一个" + passive[0].name + "死亡";
                passive.Remove(passive[0]);
                passivecopy.Remove(passivecopy[0]);

            }
            if (active[0].heart <= 0)
            {//该小兵死亡
                //battlestr += "本次被反击" + PlayerTroops.name + "死亡一个" + active[0].name + "死亡";
                active.Remove(active[0]);
                activecopy.Remove(activecopy[0]);
            }
            else
            {
                if (active[0].spec == true && passive.Count > 0)
                {
                    System.Random rand = new System.Random();
                    if (rand.Next(1, 100) < active[0].extraDmg)
                    {
                        int id = rand.Next(0, passive.Count - 1);
                        if(passive[id].heart > active[0].extraDmgVal)
                        {
                            passive[id].heart -= active[0].extraDmgVal;
                        }
                        else
                        {
                            passive[id].heart =0;
                        }
                         //player battle report
                        activeRd.MakeDamage[active[0].id] += active[0].extraDmgVal;
                        //enemy battle report
                        passiveRd.TakeDamage[passive[0].id] += active[0].extraDmgVal;
                        //计算额外伤害
                        activeRd.SpecNum[active[0].id] += 1;

                       // battlestr += PlayerTroops.name + " " + active[0].name + " 对 " + EnemyTroops.name + " " + passive[id].name + " 造成" + active[0].extraDmgVal.ToString() + "额外伤害 \n";
                        if (passive[id].heart <= 0)
                        {//该小兵死亡
                         //   battlestr += PlayerTroops.name + "本次额外伤害造成 " + EnemyTroops.name + "死亡一个" + passive[id].name;
                            passive.Remove(passive[id]);
                            passivecopy.Remove(passivecopy[0]);

                        }
                    }
                }
                active.Clear();
                for (var i = 0; i < activecopy.Count; i++)
                {
                    active.Add(CopyHumanData(activecopy[i]));
                }
                passive.Clear();
                for (var i = 0; i < passivecopy.Count; i++)
                {
                    passive.Add(CopyHumanData(passivecopy[i]));
                }
                if (active[0].spec == true)
                {
                    uint def, atk;
                    System.Random rand = new System.Random();
                    if (rand.Next(1, 100) < active[0].chDef)
                    {
                        def = (uint)(active[0].chDefVal / 100.0 * active[0].defence);
                        active[0].defence = def;
                        //防御特效
                        activeRd.SpecNum[active[0].id] += 1;
                    }

                    if (rand.Next(1, 100) < active[0].chAtk)
                    {
                        atk = (uint)(active[0].chAtkVal / 100.0 * active[0].attack);
                        active[0].attack = atk;
                        //防御特效
                        activeRd.SpecNum[active[0].id] += 1;
                    }
                }
            }
        }
        static void Demon_A_Demon(RoundRecords activeRd, RoundRecords passiveRd, List<_Demon_> active,List<_Demon_>activecopy, List<_Demon_> passive, List<_Demon_> passivecopy, ref string battlestr)
        {
            //A主动攻击P，对P造成的伤害
            int a_to_p_harm = (int)(active[0].attack * (1 - (4.0 * passive[0].defence) / (8 * passive[0].defence + 50)));
            //P被动反击，对A造成的伤害
            int p_to_a_harm = (int)(passive[0].attack * (1 - (4.0 * active[0].defence) / (8 * active[0].defence + 50)));

            if (passive[0].heart > (uint)a_to_p_harm)
            {
                passive[0].heart = passive[0].heart - (uint)a_to_p_harm;
                passivecopy[0].heart -= (uint)a_to_p_harm;//维持下一环节
            }
            else
            {
                passive[0].heart = 0;
                passivecopy[0].heart = 0;
            }
            //passive[0].heart -= (uint)a_to_p_harm;
            // passivecopy[0].heart -= (uint)a_to_p_harm;//维持下一环节
            if (active[0].heart > (uint)p_to_a_harm)
            {
                active[0].heart -= (uint)p_to_a_harm;
                activecopy[0].heart -= (uint)p_to_a_harm;
            }
            else
            {
                active[0].heart = 0;
                activecopy[0].heart = 0;
            }
            activeRd.MakeDamage[active[0].id] += (uint)a_to_p_harm;
            activeRd.TakeDamage[active[0].id] += (uint)p_to_a_harm;
            //enemy battle report
            passiveRd.MakeDamage[passive[0].id] += (uint)p_to_a_harm;
            passiveRd.TakeDamage[passive[0].id] += (uint)a_to_p_harm;
            //battlestr += PlayerTroops.name + " " + active[0].name + " 攻击 " + EnemyTroops.name + " " + passive[0].name + " 造成" + a_to_p_harm.ToString() + "伤害 \n";
            //battlestr += EnemyTroops.name + " " + passive[0].name + " 反击 " + PlayerTroops.name + " " + active[0].name + " 造成" + p_to_a_harm.ToString() + "伤害 \n";
            if (passive[0].heart <= 0)
            {//该小兵死亡
                //battlestr += "本次攻击" + EnemyTroops.name + "死亡一个" + passive[0].name + "死亡";
                passive.Remove(passive[0]);
                passivecopy.Remove(passivecopy[0]);

            }
            if (active[0].heart <= 0)
            {//该小兵死亡
               // battlestr += "本次被反击" + PlayerTroops.name + "死亡一个" + active[0].name + "死亡";
                active.Remove(active[0]);
                activecopy.Remove(activecopy[0]);
            }
           else
            {
                if (active[0].spec == true && passive.Count > 0)
                {
                    System.Random rand = new System.Random();
                    if (rand.Next(1, 100) < active[0].extraDmg)
                    {
                        int id = rand.Next(0, passive.Count - 1);
                        if (passive[id].heart > active[0].extraDmgVal)
                        {
                            passive[id].heart -= active[0].extraDmgVal;
                        }
                        else
                        {
                            passive[id].heart = 0;
                        }
                        //player battle report
                        activeRd.MakeDamage[active[0].id] += active[0].extraDmgVal;
                        //enemy battle report
                        passiveRd.TakeDamage[passive[0].id] += active[0].extraDmgVal;
                        //计算额外伤害
                        activeRd.SpecNum[active[0].id] += 1;
                        // battlestr += PlayerTroops.name + " " + active[0].name + " 对 " + EnemyTroops.name + " " + passive[id].name + " 造成" + active[0].extraDmgVal.ToString() + "额外伤害 \n";
                        if (passive[id].heart <= 0)
                        {//该小兵死亡
                           // battlestr += PlayerTroops.name + "本次额外伤害造成 " + EnemyTroops.name + "死亡一个" + passive[id].name;
                            passive.Remove(passive[id]);
                            passivecopy.Remove(passivecopy[0]);

                        }
                    }
                }
                active.Clear();
                for (var i = 0; i < activecopy.Count; i++)
                {
                    active.Add(CopyDemonData(activecopy[i]));
                }
                passive.Clear();
                for (var i = 0; i < passivecopy.Count; i++)
                {
                    passive.Add(CopyDemonData(passivecopy[i]));
                }
                if (active[0].spec == true)
                {
                    uint def, atk;
                    System.Random rand = new System.Random();
                    if (rand.Next(1, 100) < active[0].chDef)
                    {
                        def = (uint)(active[0].chDefVal / 100.0 * active[0].defence);
                        active[0].defence = def;
                        //计算防御特效
                        activeRd.SpecNum[active[0].id] += 1;
                    }

                    if (rand.Next(1, 100) < active[0].chAtk)
                    {
                        atk = (uint)(active[0].chAtkVal / 100.0 * active[0].attack);
                        active[0].attack = atk;
                        //计算攻击特效
                        activeRd.SpecNum[active[0].id] += 1;
                    }
                }
            }
        }
        
        //human speed sort ：Descending
        public class HU_BattleSpeed_DES : IComparer<_Human_>
        {
            public int Compare(_Human_ x, _Human_ y)
            {
                return y.speed.CompareTo(x.speed);
            }
        }
        //human defence sort ：Ascending
        public class HU_BattleDefence_ASC : IComparer<_Human_>
        {
            public int Compare(_Human_ x, _Human_ y)
            {
                return x.defence.CompareTo(y.defence);
            }
        }
        //demon speed sort ：Descending
        public class DE_BattleSpeed_DES : IComparer<_Demon_>
        {
            public int Compare(_Demon_ x, _Demon_ y)
            {
                return y.speed.CompareTo(x.speed);
            }
        }
        //demon defence sort ：Ascending
        public class DE_BattleDefence_ASC : IComparer<_Demon_>
        {
            public int Compare(_Demon_ x, _Demon_ y)
            {
                return x.defence.CompareTo(y.defence);
            }
        }

        private static bool IsBattleFinish()
        {
            return BattleRds.PlayerWinNum == 2 || BattleRds.EnemyWinNum == 2;
        }

        private static void InitBattleRecord()
        {
            BattleRds.PlayerRecords.Add(BattleRds.RoundNum, new RoundRecords());
            BattleRds.EnemyRecords.Add(BattleRds.RoundNum, new RoundRecords());

            var plkeys = PlayerTroops.troops.Keys;
            foreach (var key in plkeys)
            {
                BattleRds.PlayerRecords[BattleRds.RoundNum].MakeDamage.Add(key, 0);
                BattleRds.PlayerRecords[BattleRds.RoundNum].TakeDamage.Add(key, 0);
                BattleRds.PlayerRecords[BattleRds.RoundNum].SpecNum.Add(key, 0);
            }

            var enkeys = EnemyTroops.troops.Keys;
            foreach (var key in enkeys)
            {
                BattleRds.EnemyRecords[BattleRds.RoundNum].MakeDamage.Add(key, 0);
                BattleRds.EnemyRecords[BattleRds.RoundNum].TakeDamage.Add(key, 0);
                BattleRds.EnemyRecords[BattleRds.RoundNum].SpecNum.Add(key, 0);
            }
        }
    }

    // TODO 在UI 部分写消息发送

}