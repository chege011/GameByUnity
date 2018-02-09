using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GameBox.Framework;
using GameBox.Service.AssetManager;
using GameBox.Service.NetworkManager;
using Network;
using UnityEngine;
using UI;
using Assets;
using Battle;

public class Program : MonoBehaviour
{
    private bool _isDataOk;
    private bool _isServiceOk;
    private bool _isInit;

    // Use this for initialization
    void Start ()
	{
        _isDataOk = false;
        _isServiceOk = false;
        _isInit = false;

        InitData();
	    InitService();
	}

    void Update()
    {
        if (!_isInit)
        {
            InitGame();
        }
    }

    private void InitData()
    {
        StartCoroutine(ExcelManager.LoadAll_Enum(progress =>
        {
            if (progress == 1.0f)
            {
                _isDataOk = true;
                InitGame();
            }
        }));
        _isDataOk = true;
    }

    private void InitService()
    {
        new ServiceTask(new[]
        {
            typeof(IAssetManager),
            typeof(INetworkManager)
        }).Start().Continue(t =>
        {
            MsgHandler.Init();
            AssetsManager.Init();
            _isServiceOk = true;
            return null;
        });
    }

    private void InitGame()
    {
        if (_isDataOk && _isServiceOk && AssetsManager.IsAssetManagerOk())
        {
            Debuger.EnableOnScreen(false);
            Debuger.EnableOnText(false);
            UIManager.Init();
            GameObject.FindGameObjectWithTag("MsgReceiver").GetComponent<MsgReceiver>().Init();
            BattleManager.Init();
            UIManager.SwitchUI(UIManager.UIState.Login);
            _isInit = true;
        }
    }
}
