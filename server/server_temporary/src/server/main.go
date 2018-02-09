package main

import (
	"base/glog"
	"base/gonet"
	"common"
	"fmt"
	"net"
	//"plr"
	//"sync"
	"time"
	"usercmd"
)

//log 开管，开：true。关：false
const OnOff bool = true

//var lock sync.RWMutex
var timer1 int64 = 0
var timer2 int64 = 0
var flag bool = true

type EchoTask struct {
	gonet.TcpTask
}

/******************
房间结构体
/******************/
type RooM struct {
	ROOM_ID    int               //房间ID
	isALL      bool              //房间是否满员,且在战斗
	player_num int               //房间玩家个数
	my_link    map[int]*EchoTask //房间中玩家的IP
	player_A   string            //玩家1
	player_B   string            //玩家2
	timeA      int64
	timeB      int64
}

/*********************
用户结构体
/*********************/
type player struct {
	useNmae      string //玩家名字
	ROOM_ID      int    //房间ID初始值为100
	action       bool   //玩家状态是：匹配状态，
	login        bool   //true 登陆状态，false未登录状态
	choose_heros int    //默认为0。返回1：通知客户端开始选择指挥官和种族。接收为2：英雄等选择完毕
}

//房间最大数
const RoomMax int32 = 40

//房间列表
var my_room [40]RooM

//登录验证
func (uselogin *EchoTask) Loginverify() bool {
	if OnOff {
		//glog.Error("Loginverify----验证开始")
	}
	cmd := usercmd.MsgTypeWzb_login_CNF
	touser := &usercmd.GamePlayer{}
	sdata, sflag, err := common.EncodeCmd(uint16(cmd), touser)

	if err != nil {
		glog.Error("编码出错")
		return false
	}
	uselogin.AsyncSend(sdata, sflag)
	return true

}

//房间匹配
func (uselink *EchoTask) MatchRoom(data []byte, flag byte) {
	if OnOff {
		glog.Error("房间匹配开始")
	}
	//用户不在任何房间
	for id := 0; id < int(RoomMax); id++ {
		if my_room[id].my_link[0] == uselink || my_room[id].my_link[1] == uselink {
			if OnOff {
				glog.Error("房间已经有该玩家，正在匹配")
			}
			break
		}
		switch my_room[id].player_num {
		case 0:
			if OnOff {
				glog.Error("进入房间，正在匹配")
			}
			//use.ROOM_ID = int32(id)
			my_room[id].player_num = 1 //1 为等待匹配
			//my_room[id].player_A = use.Usename
			wzb1 := make(map[int]*EchoTask)
			wzb1[0] = &EchoTask{
				TcpTask: uselink.TcpTask,
			}
			my_room[id].my_link = make(map[int]*EchoTask)
			//			lock.Lock()
			my_room[id].my_link[0] = wzb1[0]
			//			lock.Unlock()
			if OnOff {
				glog.Error(my_room[id].my_link[0].Conn.RemoteAddr().String())
			}
			//返回消息等待匹配等待
			return
		case 1:
			if OnOff {
				glog.Error("匹配成功")
			}
			//use.ROOM_ID = int32(id)
			my_room[id].player_num = 2
			//my_room[id].player_B = use.Usename
			wzb2 := make(map[int]*EchoTask)
			wzb2[0] = &EchoTask{
				TcpTask: uselink.TcpTask,
			}
			//			lock.Lock()
			my_room[id].my_link[1] = wzb2[0]
			//			lock.Unlock()
			if OnOff {
				for i := 0; i < 2; i++ {
					glog.Error(my_room[id].my_link[i].Conn.RemoteAddr().String())
				}
			}
			uselink.BoradcastMatch(data, flag)
			//返回消息匹配成功，向双方发送消息
			return
		case 2:
			//该房间已经满员
			if OnOff {
				glog.Error("该房间已经满员")
			}
			//return
		default:
			glog.Error("房间人数不能超出2个人，分配失败")
			return
		}
	}

}

//发送匹配信息
func (uselink *EchoTask) BoradcastMatch(data []byte, flag byte) {
	cmd := usercmd.MsgTypeWzb_match_CNF
	//发送该房间第二个玩家，计算数据计算
	touserA := &usercmd.ChooseCal{
		IsCal: true,
	}
	sdataA, sflagA, errA := common.EncodeCmd(uint16(cmd), touserA)
	if errA != nil {
		glog.Error("编码出错")
		return
	}
	uselink.AsyncSend(sdataA, sflagA)
	//发送该房间第一个玩家
	touserB := &usercmd.ChooseCal{
		IsCal: false,
	}
	sdataB, sflagB, errB := common.EncodeCmd(uint16(cmd), touserB)
	if errB != nil {
		glog.Error("编码出错")
		return
	}
	my_room[uselink.GetRoomID()].my_link[0].AsyncSend(sdataB, sflagB)

}

//分发玩家战斗数据
func (uselink *EchoTask) toEnemy(data []byte, flag byte, ID int) {
	if OnOff {
		glog.Error("toEnemy begin--------------------------")
		defer glog.Error("toEnemy end--------------------------")
	}

	fmt.Printf("玩家IP：" + uselink.Conn.RemoteAddr().String() + "\n")
	for _, c := range my_room[ID].my_link {

		//		glog.Error(c.Conn.RemoteAddr().String())
		if c.Conn != uselink.Conn {
			fmt.Printf(" 接受IP：" + c.Conn.RemoteAddr().String() + "\n")
			var xx EchoTask
			xx = *c
			xx.AsyncSend(data, flag)
			//c.toplayer()
		}
	}

}
func (this *EchoTask) toplayer() {
	cmd := usercmd.MsgTypeWzb_reserved1
	//发送该房间第二个玩家，计算数据计算
	fmt.Printf("reserved1---开始" + "\n")
	touserA := &usercmd.GamePlayer{}
	dataA, flagA, errA := common.EncodeCmd(uint16(cmd), touserA)
	if errA != nil {
		glog.Error("编码出错")
		return
	}
	for i := 0; i < 5; i++ {
		this.AsyncSend(dataA, flagA)
	}
	fmt.Printf("reserved1---结束" + "\n")
}
func NewEchoTask(conn net.Conn) *EchoTask {
	glog.Error("NewEchoTask\n")
	if OnOff {
		glog.Error(conn.RemoteAddr().String())
	}
	s := &EchoTask{
		TcpTask: *gonet.NewTcpTask(conn),
	}
	s.Derived = s
	return s
}

//解码报错函数
func (this *EchoTask) RetErrorMsg(ecode int) {

	/*****
	retCmd := &usercmd.RetErrorMsgCmd{
		RetCode: uint32(ecode),
	}
	/*****/
	if OnOff {
		glog.Error("解码报错")
	}
	//this.SendCmd(usercmd.MsgTypeCmd_ErrorMsg, retCmd)
	//报错发送待完善
}

/**wzb---------------------begin-----**/
func (uselink *EchoTask) Boradcastmsg(data []byte, flag byte) bool {
	if OnOff {
		glog.Error("广播发送数据")
	}

	for _, v := range my_room[uselink.GetRoomID()].my_link {
		v.AsyncSend(data, flag)
		glog.Error(v.Conn)
	}
	return true
}

func (this *EchoTask) ParseMsg(data []byte, flag byte) bool {
	if OnOff {
		//glog.Error("ParseMsg----解析数据\n")
	}
	if this.IsVerified() == false {
		this.Verify()
	}
	//glog.Error(flag)
	cmd := usercmd.MsgTypeWzb(common.GetCmd(data))
	//fmt.Println(cmd)
	if cmd == usercmd.MsgTypeWzb_login_REQ {
		//登录验证
		/****/
		if !this.Loginverify() {
			glog.Error("验证失败")
		}
		/****/
	}
	if cmd == usercmd.MsgTypeWzb_match_REQ {
		//处理匹配请求
		this.MatchRoom(data, flag)
	}
	if cmd == usercmd.MsgTypeWzb_action {
		//战斗状态
		//glog.Error("战斗状态")
		ID := this.GetRoomID()
		this.toEnemy(data, flag, ID)
	}

	if cmd == usercmd.MsgTypeWzb_deactiv_REQ {
		//玩家离开游戏
		if OnOff {
			glog.Error("战斗结束，玩家退出:" + this.Conn.RemoteAddr().String())
		}
		ID := this.GetRoomID()
		this.Deactivation(ID)
	}
	if cmd == usercmd.MsgTypeWzb_reserved1 {
		if OnOff {
			//glog.Error("------心跳包开始")
		}
		//this.toplayer()
		//this.TimeClock()
		//glog.Error("------心跳包结束")
	}
	if cmd == usercmd.MsgTypeWzb_reserved2 {
		//向对方发送玩家名字和种族
		glog.Error("向对方发送玩家名字和种族，MsgTypeWzb_reserved2")
		ID := this.GetRoomID()
		this.toEnemy(data, flag, ID)
	}
	if cmd == usercmd.MsgTypeWzb_reserved3 {
		//向对方发战报
		/****/
		if false {
			userw, _ := common.DecodeCmd(data, flag, &usercmd.BattleResult{}).(*usercmd.BattleResult)
			glog.Error(userw.MakeDamage)
			glog.Error(userw.TakeDamage)
		}
		/****/
		if OnOff {
			glog.Error("------向对方发战报")
		}
		ID := this.GetRoomID()

		this.toEnemy(data, flag, ID)
	}
	return true
}

//获取房间ID
func (this *EchoTask) GetRoomID() int {
	glog.Error("GetRoomID start------------------------")
	if OnOff {
		glog.Error(this.Conn.RemoteAddr().String() + "获取房间ID")
	}
	wzb3 := make(map[int]*EchoTask)
	wzb3[0] = &EchoTask{
		TcpTask: this.TcpTask,
	}
	for ID := 0; ID < (int)(RoomMax); ID++ {
		for i := 0; i < 2; i++ {
			if my_room[ID].my_link[i].Conn == wzb3[0].Conn {
				if OnOff {
					glog.Error("寻找房间成功")
				}
				return ID

			} else {
				//glog.Error("房间不是", ID)
			}
		}
	}
	glog.Error("用户不在任何房间里面，导致出错")
	defer glog.Error("GetRoomID finish------------------------")
	return 100

}

//游戏结束，或者客户端强制关闭游戏，清理房间存储信息
func (this *EchoTask) Deactivation(ID int) {
	if OnOff {
		glog.Error("Deactivation开始---------------------")
	}
	//var ID int = this.GetRoomID()
	if ID >= int(RoomMax) {
		glog.Error("已经被提出来了---------------------------")
		return
	}

	/*****客户端不处理，可以不发送***/
	if my_room[ID].player_num == 2 {
		cmd := usercmd.MsgTypeWzb_deactiv_CNF
		touser := &usercmd.GamePlayer{}
		sdata, sflag, err := common.EncodeCmd(uint16(cmd), touser)
		if err != nil {
			glog.Error("编码出错")
			return
		}
		glog.Error("对方发送离开消息")
		this.toEnemy(sdata, sflag, ID)
	}
	/*******/
	//user.ROOM_ID
	ClearRoom := make(map[int]*EchoTask)
	ClearRoom[0] = &EchoTask{}
	my_room[ID].player_num = 0
	my_room[ID].player_A = ""
	my_room[ID].player_B = ""
	my_room[ID].isALL = false
	//	lock.Lock()
	my_room[ID].my_link[0] = ClearRoom[0]
	my_room[ID].my_link[1] = ClearRoom[0]
	//	lock.Unlock()
	/****/
}
func (this *EchoTask) OnClose() {
	//处理玩家异常退出
	glog.Error("OnClose--链接关闭 -------------------------------begin")
	ID := this.GetRoomID()
	if ID < int(RoomMax) {
		this.Deactivation(ID)
	}

	glog.Error("OnClose--链接关闭 " + this.Conn.RemoteAddr().String())
	glog.Error("OnClose--链接关闭 -------------------------------end")
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
	//	this.RegCmds()
	init_myroom()
	return true
}

func (this *EchoServer) MainLoop() {
	if OnOff {
		//glog.Error("MainLoop\n")
	}
	conn, err := this.tcpser.Accept()
	if err != nil {
		return
	}
	NewEchoTask(conn).Start()
}

func (this *EchoServer) Reload() {
	glog.Error("Reload\n")
}

func (this *EchoServer) Final() bool {
	glog.Error("Final\n")
	this.tcpser.Close()
	return true
}

func main() {
	glog.Error("main\n")

	EchoServer_GetMe().Main()

}

//初始化所有房间
func init_myroom() {
	ClearRoom := make(map[int]*EchoTask)
	ClearRoom[0] = &EchoTask{}
	for id := 0; id < int(RoomMax); id++ {
		my_room[id].my_link = make(map[int]*EchoTask)
		my_room[id].my_link[0] = ClearRoom[0]
		my_room[id].my_link[1] = ClearRoom[0]
	}
}
func (this *EchoTask) time_my() {
	timer1 = time.Now().Unix()
	if flag {
		go timerClock()
		flag = false
	}

}

/******/
func timerClock() {
	time.Sleep(time.Second * 19)
	//timer2 := time.Now().Unix()
}

/***********/
func (this *EchoTask) TimeClock() {
	c := make(chan int64, 4)
	glog.Error("------定时器开始")
	go func() {
		c <- time.Now().Unix()
		fmt.Println(time.Now().Unix())
		time.Sleep(17 * time.Second)

		glog.Error("------子线程")
		close(c)
	}()
	time_test := time.Now().Unix()
	fmt.Println(time_test)
	for readme := range c {
		if (time_test - readme) > 10 {
			glog.Error("客户端链接正常")
		}
	}
	glog.Error("------TimeClock结束")
}

/********/
