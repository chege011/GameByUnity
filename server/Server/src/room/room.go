// wangzongbao Room handling and control
// 2018-1-22
// room project room.go
package room

import (
	"base/glog"
	"base/gonet"
	"common"
	"usercmd"
)

//log 开管，开：true。关：false
const OnOff bool = true

type EchoTask struct {
	gonet.TcpTask
	msgHandlerMap MsgHandlerMap
}

//房间结构体
type RooM struct {
	ROOM_ID    int               //房间ID
	isALL      bool              //房间是否满员,且在战斗
	player_num int               //房间玩家个数
	my_link    map[int]*EchoTask //房间中玩家的IP
	player_A   string            //玩家1
	player_B   string            //玩家2
}

//用户结构体
type player struct {
	useNmae      string //玩家名字
	ROOM_ID      int    //房间ID  初始值为100
	action       bool   //玩家状态是：匹配状态，
	login        bool   //true 登陆状态，false未登录状态
	choose_heros int    //默认为0。返回1：通知客户端开始选择指挥官和种族。接收为2：英雄等选择完毕
}

//房间最大数
const RoomMax int32 = 40

//房间列表
var my_room [40]RooM

func init() {
	glog.Error("room init")
	init_myroom()
}

//登录验证
func (uselogin *EchoTask) Loginverify(data []byte, flag byte) bool {
	if OnOff {
		glog.Error("Loginverify----验证开始")
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

func (this *EchoTask) Init() {
	this.msgHandlerMap.MpaInit()
	this.RegCmds()
}

//网络注册
func (this *EchoTask) RegCmds() {
	this.msgHandlerMap.RegisterHandler(usercmd.MsgTypeWzb_login_REQ, this.Loginverify)
	this.msgHandlerMap.RegisterHandler(usercmd.MsgTypeWzb_match_REQ, this.MatchRoom)
	this.msgHandlerMap.RegisterHandler(usercmd.MsgTypeWzb_action, this.toEnemy)
	this.msgHandlerMap.RegisterHandler(usercmd.MsgTypeWzb_deactiv_REQ, this.Deactivation)
	this.msgHandlerMap.RegisterHandler(usercmd.MsgTypeWzb_reserved1, this.HeartBag)
	this.msgHandlerMap.RegisterHandler(usercmd.MsgTypeWzb_reserved2, this.toEnemy)
	this.msgHandlerMap.RegisterHandler(usercmd.MsgTypeWzb_reserved3, this.toEnemy)
}

func (this *EchoTask) ParseMsg(data []byte, flag byte) bool {
	if OnOff {
		glog.Error("ParseMsg----解析数据\n")
	}
	if this.IsVerified() == false {
		this.Verify()
	}
	this.Init()
	cmd := usercmd.MsgTypeWzb(common.GetCmd(data))
	this.msgHandlerMap.Call(cmd, data, flag)
	return true
}
func (yselink *EchoTask) HeartBag(data []byte, flag byte) bool {
	if OnOff {
		glog.Error("--心跳包")
	}
	return true
}

//房间匹配
func (uselink *EchoTask) MatchRoom(data []byte, flag byte) bool {
	if OnOff {
		glog.Error("房间匹配开始")
	}
	//用户不在任何房间
	for id := 0; id < int(RoomMax); id++ {
		if my_room[id].my_link[0] == uselink || my_room[id].my_link[1] == uselink {
			glog.Error("房间已经有该玩家，正在匹配")
			break
		}
		switch my_room[id].player_num {
		case 0:
			glog.Error("进入房间，正在匹配")
			my_room[id].player_num = 1 //1 为等待匹配
			wzb1 := make(map[int]*EchoTask)
			wzb1[0] = &EchoTask{
				TcpTask: uselink.TcpTask,
			}
			my_room[id].my_link = make(map[int]*EchoTask)
			my_room[id].my_link[0] = wzb1[0]
			glog.Error(my_room[id].my_link[0])
			//返回消息等待匹配等待
			return true
		case 1:
			glog.Error("匹配成功")
			my_room[id].player_num = 2
			wzb2 := make(map[int]*EchoTask)
			wzb2[0] = &EchoTask{
				TcpTask: uselink.TcpTask,
			}
			my_room[id].my_link[1] = wzb2[0]
			for i := 0; i < 2; i++ {
				glog.Error(my_room[id].my_link[i])
			}
			uselink.BoradcastMatch(data, flag)
			//返回消息匹配成功，向双方发送消息
			return true
		case 2:

			glog.Error("该房间已经满员")
		default:
			glog.Error("房间人数不能超出2个人，分配失败")
			return false
		}
	}
	return true

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
func (uselink *EchoTask) toEnemy(data []byte, flag byte) bool {
	if OnOff {
		glog.Error("数据发送给敌方")
	}

	for _, c := range my_room[uselink.GetRoomID()].my_link {
		if c.Conn != uselink.Conn {
			glog.Error(c.Conn)
			glog.Error(uselink.Conn)
			c.AsyncSend(data, flag)
		}
	}
	return true
}

//解码报错函数
func (this *EchoTask) RetErrorMsg(ecode int) {
	//报错发送待完善
	/*****
	retCmd := &usercmd.RetErrorMsgCmd{
		RetCode: uint32(ecode),
	}
	/*****/
	if OnOff {
		glog.Error("解码报错")
	}
	//this.SendCmd(usercmd.MsgTypeCmd_ErrorMsg, retCmd)
}

//广播发送数据
func (uselink *EchoTask) Boradcastmsg(data []byte, flag byte) bool {
	if OnOff {
		glog.Error("广播发送数据")
		return false
	}

	for _, v := range my_room[uselink.GetRoomID()].my_link {
		v.AsyncSend(data, flag)
		glog.Error(v.Conn)
	}
	return true
}

//获取房间ID
func (this *EchoTask) GetRoomID() int {
	if OnOff {
		glog.Error("获取房间ID----1")
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
	glog.Error("用户不在任何房间里面")
	return 100
}

//游戏结束，或者客户端强制关闭游戏，清理房间存储信息
func (this *EchoTask) Deactivation(data []byte, flag byte) bool {
	if OnOff {
		glog.Error("Deactivation开始")
	}
	var ID int = this.GetRoomID()
	if ID >= int(RoomMax) {
		glog.Error("已经被提出来了")
		return false
	}

	/*****预留部分，客户端不处理，暂时不发送--begin***/
	if my_room[ID].player_num == 2 {
		cmd := usercmd.MsgTypeWzb_deactiv_CNF
		touser := &usercmd.GamePlayer{}
		sdata, sflag, err := common.EncodeCmd(uint16(cmd), touser)
		if err != nil {
			glog.Error("编码出错")
			return false
		}
		this.Boradcastmsg(sdata, sflag)
	}
	/*****预留部分，客户端不处理，暂时不发送--end**/
	ClearRoom := make(map[int]*EchoTask)
	ClearRoom[0] = &EchoTask{}
	my_room[ID].player_num = 0
	my_room[ID].player_A = ""
	my_room[ID].player_B = ""
	my_room[ID].isALL = false
	my_room[ID].my_link[0] = ClearRoom[0]
	my_room[ID].my_link[1] = ClearRoom[0]
	return true
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

func (this *EchoTask) OnClose() {
	//处理玩家异常退出
	var data []byte
	var flag byte
	if this.GetRoomID() < int(RoomMax) {
		this.Deactivation(data, flag)
	}
	glog.Error("OnClose--链接关闭\n")
}

// 网络消息处理器--begin
type MsgHandler func(data []byte, flag byte) bool

type MsgHandlerMap struct {
	handlerMap map[usercmd.MsgTypeWzb]MsgHandler
}

func (this *MsgHandlerMap) MpaInit() {
	this.handlerMap = make(map[usercmd.MsgTypeWzb]MsgHandler)
}

func (this *MsgHandlerMap) RegisterHandler(cmd usercmd.MsgTypeWzb, cb MsgHandler) {
	this.handlerMap[cmd] = cb
}

func (this *MsgHandlerMap) Call(cmd usercmd.MsgTypeWzb, data []byte, flag byte) {
	glog.Error(cmd)
	cb, ok := this.handlerMap[cmd]
	if ok {
		cb(data, flag)
	} else {
		glog.Error("MsgHandlerMap.Call: unknow cmd,", cmd)
	}
}

// 网络消息处理器--end
