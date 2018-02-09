// wangzongbao tcplink
// 2018-1-22
// room package main.go
package main

import (
	"base/glog"
	"base/gonet"
	"fmt"
	"net"
	"room"
)

//log 开管，开：true。关：false
const OnOff bool = true

type WzbEchoTask struct {
	room.EchoTask
}

func NewEchoTask(conn net.Conn) *WzbEchoTask {
	glog.Error("NewEchoTask\n")
	glog.Error(conn.LocalAddr())

	w := room.EchoTask{
		TcpTask: *gonet.NewTcpTask(conn),
	}
	s := &WzbEchoTask{
		EchoTask: w,
	}
	s.Derived = s
	return s
}

type EchoServer struct {
	gonet.Service
	tcpser *gonet.TcpServer
}

var serverm *EchoServer

func EchoServer_GetMe() *EchoServer {
	glog.Error("EchoServer_GetMe\n")
	if serverm == nil {
		serverm = &EchoServer{
			tcpser: &gonet.TcpServer{},
		}
		serverm.Derived = serverm
	}
	return serverm
}

func (this *EchoServer) Init() bool {
	glog.Error("Init\n")
	err := this.tcpser.Bind(":8000")
	if err != nil {
		fmt.Println("绑定端口失败")
		return false
	}
	return true
}

func (this *EchoServer) MainLoop() {
	if OnOff {
		glog.Error("MainLoop\n")
	}
	conn, err := this.tcpser.Accept()
	if err != nil {
		return
	}
	NewEchoTask(conn).Start()
}

func (this *EchoServer) Reload() {
	if OnOff {
		glog.Error("Reload\n")
	}

}

func (this *EchoServer) Final() bool {
	if OnOff {
		glog.Error("Final\n")
	}
	this.tcpser.Close()
	return true
}

func main() {
	if OnOff {
		glog.Error("main\n")
	}
	EchoServer_GetMe().Main()

}
