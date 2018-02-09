using GameBox.Framework;
using System.Collections;
using System.Collections.Generic;
using Assets;
using GameBox.Service.AssetManager;
using UnityEngine;
using Battle;

namespace UI
{
    public static class UIManager
    {
        private static string rootPath = "UI";
        public static UIState State;

        public static AudioManager audioManager;

        public enum UIState
        {
            Login,
            MainTitle,
            RaceChose,
            FootManChoose,
            Battle,
            LoadingLoM,
            LoadingWData,
            BattleResult
        }

        public static GameObject UITotalBg { get; private set; }
        public static GameObject UILogin { get; private set; }
        public static GameObject UIMainTitle { get; private set; }
        public static GameObject UIRaceChose { get; private set; }
        public static GameObject UIFootManChoose { get; private set; }
        public static GameObject UIBattle { get; private set; }
        public static GameObject UILoading { get; private set; }
        public static GameObject UIBattleResult { get; private set; }

        public static void Init()
        {
            UITotalBg = GameObject.FindGameObjectWithTag("TotalBg");
            UILogin = AssetsManager.LoadAsset(rootPath + "\\UILogin", false);
            UIMainTitle = AssetsManager.LoadAsset(rootPath + "\\UIMainTitle", false);
            UIRaceChose = AssetsManager.LoadAsset(rootPath + "\\UIRaceChose", false);
            UIFootManChoose = AssetsManager.LoadAsset(rootPath + "\\UIFootManChoose", false);
            UIBattle = AssetsManager.LoadAsset(rootPath + "\\UIBattle", false);
            UILoading = AssetsManager.LoadAsset(rootPath + "\\UILoading", false);
            UIBattleResult = AssetsManager.LoadAsset(rootPath + "\\UIBattleResult", false);

            UIFootManChoose.GetComponent<UIFootManChoose>().Init();
            UIBattleResult.GetComponent<UIBattleResult>().Init();

            audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        }

        public static void SwitchUI(UIState state)
        {
            SetAllDeactive();
            State = state;
            switch (state)
            {
                case UIState.Login: UILogin.SetActive(true); break;
                case UIState.MainTitle:
                    UIMainTitle.SetActive(true);
                    audioManager.PlayAudioMain();
                    break;
                case UIState.RaceChose: UIRaceChose.SetActive(true); break;
                case UIState.FootManChoose:
                    audioManager.PlayAudioBattle();
                    UIFootManChoose.GetComponent<UIFootManChoose>().Clear();
                    UIFootManChoose.SetActive(true);
                    break;
                case UIState.Battle: UIBattle.SetActive(true); break;
                case UIState.LoadingLoM:UILoading.SetActive(true);break;
                case UIState.LoadingWData:
                    UILoading.GetComponent<UILoading>().SetFtToBattle();
                    UILoading.SetActive(true); break;
                case UIState.BattleResult:
                    UIBattleResult.GetComponent<UIBattleResult>().UpdateShow();
                    UIBattleResult.SetActive(true);
                    if (BattleManager.IsWin)
                        audioManager.PlayAudioWin();
                    else
                        audioManager.PlayAudioLose();
                    break;
                    // TODO Win Lose BattleFinish
            }
        }

        private static void SetAllDeactive()
        {
            if (UITotalBg.activeSelf)
            {
                UITotalBg.SetActive(false);
            }
            if (UILogin.activeSelf)
            {
                UILogin.SetActive(false);
            }

            if (UIMainTitle.activeSelf)
            {
                UIMainTitle.SetActive(false);
            }

            if (UIRaceChose.activeSelf)
            {
                UIRaceChose.SetActive(false);
            }

            if (UIFootManChoose.activeSelf)
            {
                UIFootManChoose.SetActive(false);
            }

            if (UIBattle.activeSelf)
            {
                UIBattle.SetActive(false);
            }

            if (UILoading.activeSelf)
            {
                UILoading.SetActive(false);
            }

            if (UIBattleResult.activeSelf)
            {
                UIBattleResult.SetActive(false);
            }
        }
    }
}

