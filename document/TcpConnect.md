代码模板在template 中

## 包的结构（题主定义）

|-------head-------|---------------body------------|

head 中存储整个包的基本信息 前3个字节是包长度 使用 小端格式 第4个字节是压缩为，可以根据情况删除

body 分为如下

|----command----|----obj----|

command 为 protobuf 定义的enum值字段，用来区分该数据包的种类

obj 为此数据包应该带的结构体数据，可以为空

代码中
```csharp
private const int MaxPackageLength = 65535;
private const int PackageHeadLength = 6;
private const int CommandHeadLength = 2;
```
    
规定了包的基础性 可以根据不同需要修改

## 可自定义函数
### Send
`Send` 中的 Package 参数 可根据自己定义的包的需要进行修改 并在 `Pack` 函数中写入对应的打包过程，将Package 对象转换成二进制数据

### OnServerStateChange
`OnServerStateChange`中的内容可以根据需要写入对应的状态下面，在状态转换过程中进行处理

### Pack 和 Unpack
如果要自定义包结构，需要需改此函数

## 用法
首先需要在你想建立连接的函数中创建 TcpConnect 对象
```csharp
public class Msg{
    public TcpConnection tcpConnection;

    public Msg(){
        tcpConnection = new TcpConnection();
    }
}
```

然后在使用Connect与服务端建立连接
```csharp
public class Msg{
    public TcpConnection tcpConnection;

    public Msg(){
        tcpConnection = new TcpConnection();
        tcpConnection.Connect(ip, port);
    }
}
```
### 发送
```csharp
public class Msg{
    public TcpConnection tcpConnection;

    public Msg(){
        tcpConnection = new TcpConnection();
        tcpConnection.Connect(ip, port);
        var package = new Package();
        tcpConnection.Send(package);
    }
}
```
### 接收
```csharp
public class Msg : MonoBehaviour{
    public TcpConnection tcpConnection;

    public Msg(){
        tcpConnection = new TcpConnection();
        tcpConnection.Connect(ip, port);
        tcpConnection.Receive(Receiver);
    }

    void Update(){
        tcpConnection.Receive(Receiver);
    }

    public Receiver(Object obj){
        // Do something
    }
}
```
在 Update 中调用Receive 会处理在缓冲队列中的所有包，但这时调用，并不是注册函数，所以需要定期使用`tcpConnection.Receive(Receiver);`来处理包。可以建议用一些其他函数来包裹此调用，进一步控制每帧的数据处理的开销。







