//****************************************************************************
// Description:消息注册发送及处理
// Author: chege011
// Email: 598054306@qq.com
//****************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public class MsgHandler
    {
        private static Dictionary<int, Action<IProto>> _dic = new Dictionary<int, Action<IProto>>();

        private static TcpConnection _tcpConnection;

        private const string Ip = "192.168.198.30";
        private const int Port = 8000;

        // 初始化MessageHander
        public static void Init()
        {
            if (_tcpConnection == null)
            {
                _tcpConnection = new TcpConnection();
            }
        }

        public static void StartConnetToServer(string ip = Ip, int port = Port)
        {
            if (_tcpConnection.IsNetworkManagerOk())
            {
                _tcpConnection.Connect(ip, port);
            }
            else
            {
                Debug.Log("tcp NetworkManager is not ok");
            }
        }

        public static bool IsConnect()
        {
            return _tcpConnection != null && _tcpConnection.IsConnect();
        }

        // 注册收到消息之后的动作
        public static void Regist(int id, Action<IProto> action){
            if (_dic.ContainsKey((int)id))
            {
                Debug.LogWarning("dont need regist again");
                return;
            }
            _dic.Add((int)id, action);
        }

        // 注销收到消息之后的动作
        public static void UnRegist(int id)
        {
            if (_dic.ContainsKey((int)id))
            {
                _dic.Remove((int)id);
                return;
            }
            Debug.Log("do not have key:" + id);
        }

        // 外部函数调用的发送信息的接口
        public static void SendMessage(int id, object obj = null){
            if(obj == null){
                SendPackage(id);
            } else {
                IProto proto = new Proto(obj);
                SendPackage(id, proto.ToBytes());
            }
        }

        // 外部函数调用进行消息处理
        public static void ProcessReceiveMessage(Action<object> handler)
        {
            _tcpConnection.Receive(handler);
        }

        // 内部使用将消息对象封装成数据包
        private static void SendPackage(int id, byte[] bytes=null){
            var package = new Package(id, bytes);
            if (_tcpConnection == null){
                Debug.Log("should init socket first");
                return;
            }
            _tcpConnection.Send(package);
        }

        // MsgReceiver使用将消息分发并执行消息处理 分发的函数是根据id值来将Proto.bytes 转成 对应的Proto.obj 的
        public static void Dispatch(int id, byte[] bytes)
        {
            if (!_dic.ContainsKey(id))
            {
                Debug.LogWarning("should regeist first:" + id);
                return;
            }
            IProto iProto = new Proto(bytes);
            _dic[id](iProto);
        }

        public static void Clear()
        {
            _dic.Clear();
        }

        // 关闭TCP 连接
        public static void Disconnect()
        {
            _tcpConnection.Disconnect();
        }

        // 关闭MsgHandler
        public static void Close()
        {
            Disconnect();
            Clear();
        }
    }
}
