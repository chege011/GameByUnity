//****************************************************************************
// Description:tcp通信逻辑
// Author: chege011
// Email: 598054306@qq.com
//****************************************************************************

using System;
using GameBox.Framework;
using GameBox.Service.ByteStorage;
using GameBox.Service.NetworkManager;
using UnityEngine;
using UI;

namespace Network
{
    public class TcpConnection : IProtocolHost
    {

        // Debug 开关
        private bool _isDebug = true;
        
        // 包的基础属性
        private const int MaxPackageLength = 65535;
        private const int PackageHeadLength = 6;
        private const int CommandHeadLength = 2;

        // Unpack过程中的过程标识开关
        private bool _headReaded;
        private uint _packBodyLen;

        // 在Unpack过程中用到的缓冲区
        private readonly ByteArray _receiveBuffer = new ByteArray();
        private readonly ByteArray _bodyBuffer = new ByteArray();

        // Tcp底层连接状态
        public enum ConState
        {
            Connect,
            Connecting,
            Disconnect
        }
        public ConState State { get; private set; }

        // Tcp连接管理 : GameBox内的类
        private INetworkManager _iNetworkManager;
        private ISocketChannel _iSocketChannel;


        // 初始化资源 使用 GameBox ServiceTask 进行异步加载
        public TcpConnection()
        {
            new ServiceTask(new[]
            {
                typeof(IByteStorage),
                typeof(INetworkManager)
            }).Start().Continue(t =>
            {
                _iNetworkManager = ServiceCenter.GetService<INetworkManager>();
                return null;
            });

            _headReaded = false;
            _packBodyLen = 0;
            State = ConState.Disconnect;
        }

        // 建立连接
        public void Connect(string ip, int port)
        {
            if (_iNetworkManager == null)
            {
                Debug.Log("INetworkManager is not exist");
                return;
            }

            _iSocketChannel = _iNetworkManager.Create("tcp") as ISocketChannel;
            if (_iSocketChannel == null)
            {
                Debug.Log("Creat tcp socket channel failed");
                return;
            }

            _iSocketChannel.Setup(this);
            _iSocketChannel.OnChannelStateChange = OnServerStateChange;
            _iSocketChannel.Connect(ip, port);
        }

        // 断开连接
        public void Disconnect()
        {
            if (_iSocketChannel == null) return;
            _iSocketChannel.Disconnect();
            _iSocketChannel.Dispose();
            _iSocketChannel = null;
        }

        // 发送数据包
        public void Send(Package package)
        {
            if (State != ConState.Connect)
            {
                Debug.Log("Please connect a host first");
                return;
            }
            _iSocketChannel.Send(package);
        }

        // 接收数据包
        public void Receive(Action<object> handler)
        {
            var flag = _iSocketChannel.Receive(handler);
            while (flag)
            {
                flag = _iSocketChannel.Receive(handler);
            }
        }

        // GameBox 发送包时调用
        public void Pack(IObjectReader reader, IByteArray writer)
        {
            var package = reader.ReadOne() as Package;
            if (package == null) return;
            var bytes = package.ToBytes();

            uint packBodyLen = 2;
            if (package.Body != null)
                packBodyLen = (uint)package.Body.Length + 2;
            byte len1 = (byte)packBodyLen;
            byte len2 = (byte)(packBodyLen >> 8);
            byte len3 = (byte)(packBodyLen >> 16);
            writer.WriteByte(len1);
            writer.WriteByte(len2);
            writer.WriteByte(len3);
            writer.WriteByte(0);

            writer.WriteBytes(bytes, 0, bytes.Length);
        }

        // GameBox 接收到包时调用
        public void Unpack(IByteArray reader, IObjectWriter writer)
        {
            _receiveBuffer.WriteBytes(reader.ReadBytes());
            while (true)
            { 
                if (!_headReaded)
                {
                    if (!ReadHead()) return;
                    if (!ReadBody()) return;
                }
                else
                {
                    if (!ReadBody()) return;
                }
                writer.WriteOne(Package.BytesToPackage(_bodyBuffer.Bytes));
                _headReaded = false;
                _packBodyLen = 0;
                _bodyBuffer.Clear();
            }
        }

        private bool ReadHead()
        {
            if (_receiveBuffer.Length < PackageHeadLength) return false;

            var len1 = _receiveBuffer.ReadUnsignedByte();
            var len2 = _receiveBuffer.ReadUnsignedByte();
            var len3 = _receiveBuffer.ReadUnsignedByte();
            var compress = _receiveBuffer.ReadUnsignedByte(); // 读取压缩位置

            _packBodyLen = len1 | (len2 << 8) | (len3 << 16);

            // buffer error
            if (_packBodyLen < CommandHeadLength || _packBodyLen > MaxPackageLength)
            {
                _receiveBuffer.Clear();
                _packBodyLen = 0;
                return false;
            }

            _headReaded = true;
            return true;
        }

        private bool ReadBody()
        {
            if (_receiveBuffer.Length < _packBodyLen) return false;
            _receiveBuffer.ReadBytes(_bodyBuffer, (int)_packBodyLen);
            return true;
        }

        // GameBox 底层在连接状态发生改变时会被调用
        private void OnServerStateChange(string state)
        {
            switch (state)
            {
                case ChannelState.CONNECTED:
                    State = ConState.Connect;
                    Debug.Log("OnServerState" + ChannelState.CONNECTED);
                    Debuger.Log("OnServerState" + ChannelState.CONNECTED);
                    break;
                case ChannelState.CONNECTING:
                    State = ConState.Connecting;
                    Debug.Log("OnServerState" + ChannelState.CONNECTING);
                    Debuger.Log("OnServerState" + ChannelState.CONNECTING);
                    break;
                case ChannelState.DISCONNECTED:
                    State = ConState.Disconnect;
                    Debug.Log("OnServerState" + ChannelState.DISCONNECTED);
                    Debuger.Log("OnServerState" + ChannelState.DISCONNECTED);
                    Disconnect();
                    Application.Quit();
                    UIManager.SwitchUI(UIManager.UIState.Login);
                    break;
            }
        }
    }
}
