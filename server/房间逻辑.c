/*************

王宗宝
2018-1-22
房间逻辑条理分析

/************/

链接之后
判断若为链接消息，则返回链接成功

会开始匹配
接收到开始匹配的消息后，进入房间管理，为玩家分配房间

RoomMgr()
{
1>>用户不在任何房间（新玩家）
   >>>>AddRooM()//用户加入房间

2>>玩家已经拥有房间
   >>>>my_room[RooM_ID].boradcastmsg(data,flag)//boradcastmsg()将该玩家的数据发送给房间内的其他玩家
   //对boradcastmsg()修改，只将data数据发送给房间内的其他玩家，而不发送给自己。
}

//用户加入房间
AddRooM(){
	//判断房间中是否有人，有人则开始，没人返回等待
	//有一个人等待，两个人则开始。通知两个人开始
	
}



玩家离开房间
发送退出消息
DeleteRoom()
{
	//假如该玩家所在的房间还在，则退出房间并将房间的数据置空
	if(my_room[RooM_ID].isALL != false)
	{
		my_room[RooM_ID].isALL = true
		my_room[RooM_ID].playernum = 0
		my_room[RooM_ID].my_link[0] //置成0.0.0.0：0000
		my_room[RooM_ID].my_link[1] //置成0.0.0.0：0000
	}
	//假如该房间已经不再，则直接返回

}
