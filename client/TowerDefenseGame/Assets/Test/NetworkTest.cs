using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network;

public class NetworkTest : MonoBehaviour
{
    private TcpConnection xx;
    private bool flag;
	// Use this for initialization
	void Start () {
	    xx = new TcpConnection();
        flag = true;
    }
	
	// Update is called once per frame
	void Update () {
	    if (xx.IsNetworkManagerOk() && flag == true)
	    {
	        print("Network start ok");
	        xx.Connect("192.168.242.21", 8000);
	        var bytes = new byte[5];
	        bytes[1] = 0x1;
	        bytes[2] = 0x2;
	        bytes[3] = 0x3;
	        bytes[4] = 0x4;
	        xx.Send(new Package(1, bytes));
            flag = false;
	    }
	}
}
