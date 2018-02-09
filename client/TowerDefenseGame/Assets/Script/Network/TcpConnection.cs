//****************************************************************************
// Description:tcp通信逻辑
// Author: chege011
// Email: 598054306@qq.com
//****************************************************************************

using System;
using Battle;
using GameBox.Framework;
using GameBox.Service.ByteStorage;
using GameBox.Service.NetworkManager;
using usercmd;
using UnityEngine;
using UI;

namespace Network
{
    public class TcpConnection : IProtocolHost
    {
        // comman connetion settings
        // TODO 这下面的可以放到 message handler 里面

        private const int MaxPackageLength = 65535;
        private const int PackageHeadLength = 6;
        private const int CommandHeadLength = 2;

        // buffer
        private readonly ByteArray _receiveBuffer = new ByteArray();
        private readonly ByteArray _bodyBuffer = new ByteArray();

        // tcp connetion state
        private const string Connected = "connected";
        private const string Connecting = "connecting";
        private const string Disconnected = "disconnected";
        private bool _isConnect;

        // tcp manger settings
        private INetworkManager _iNetworkManager;
        private ISocketChannel _iSocketChannel;

        // package used
        private bool _headReaded;
        private bool _noBody;
        private uint _packBodyLen;

        public TcpConnection()
        {
            //// TODO: 考虑如何在游戏退出时断开连接

            new ServiceTask(new[]
            {
                typeof(IByteStorage),
                typeof(INetworkManager)
            }).Start().Continue(t =>
            {
                _iNetworkManager = ServiceCenter.GetService<INetworkManager>();
                _headReaded = false;
                _packBodyLen = 0;
                _isConnect = false;
                return null;
            });
        }

        // 因为iNetworkManager 是异步加载的，在使用前需要检查iNetworkManager的状态
        public bool IsNetworkManagerOk()
        {
            return _iNetworkManager != null;
        }

        public void Connect(string ip, int port)
        {
            _iSocketChannel = _iNetworkManager.Create("tcp") as ISocketChannel;
            if (_iSocketChannel == null)
            {
                Debug.Log("TcpConnection 58 Connect failed");
                return;
            }
            _iSocketChannel.Setup(this);
            _iSocketChannel.OnChannelStateChange = OnServerStateChange;
            _iSocketChannel.Connect(ip, port);
            _isConnect = true;
            Debug.Log("conneted");
            Debuger.Log("connected");
        }

        public bool IsConnect()
        {
            return _isConnect;
        }

        public void Disconnect()
        {
            _isConnect = false;
            if (_iSocketChannel == null) return;
            _iSocketChannel.Disconnect();
            _iSocketChannel.Dispose();
            _iSocketChannel = null;
        }

        public void Send(Package package)
        {
            _iSocketChannel.Send(package);
        }

        // TODO 当数据包包过多时可以在此做控制
        public void Receive(Action<object> handler)
        {
            var flag = _iSocketChannel.Receive(handler);
            while (flag)
            {
                flag = _iSocketChannel.Receive(handler);
            }
            
        }

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

        private void OnServerStateChange(string state)
        {
            switch (state)
            {
                case Connected:
                    Debug.Log("OnServerState" + Connected);
                    Debuger.Log("OnServerState" + Connected);
                    break;
                case Connecting:
                    Debug.Log("OnServerState" + Connecting);
                    Debuger.Log("OnServerState" + Connecting);
                    break;
                case Disconnected:
                    Debug.Log("OnServerState" + Disconnected);
                    Debuger.Log("OnServerState" + Disconnected);
                    Disconnect();
                    Application.Quit();
                    UIManager.SwitchUI(UIManager.UIState.Login);
                    break;
            }
        }
    }
}
