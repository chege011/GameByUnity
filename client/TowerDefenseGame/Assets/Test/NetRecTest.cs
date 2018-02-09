using System.Collections;
using System.Collections.Generic;
using Network;
using UnityEngine;
using usercmd;

public class NetRecTest : MonoBehaviour {
    private TcpConnection xx;
    private bool flag;
    private float timer;

    // Use this for initialization
    void Start () {
	    xx = new TcpConnection();
	    flag = true;
        timer = 0;
    }
	
	// Update is called once per frame
	void Update () {
	    if (xx.IsNetworkManagerOk() && flag == true)
	    {
	        print("Network start ok");
	        xx.Connect("192.168.242.21", 8000);
	        xx.Send(new Package((int)MsgType_wzb.match_REQ, null));
	        flag = false;
	    }

	    if (xx.IsNetworkManagerOk() && timer > 5)
	    {
	        xx.Send(new Package((int)MsgType_wzb.match_REQ, null));
	        timer = 0;
	    }

	    timer += Time.deltaTime;
	    xx.Receive(XXOnReceive);
	}

    public void XXOnReceive(object obj)
    {
        var pack = obj as Package;
        if (pack.Id == (int)MsgType_wzb.login_CNF)
        {
            Debug.Log("Login");
        }
    }

    class ReceiverXX
    {
        public void OnReceive(object obj)
        {
            var pack = obj as Package;
            if (pack.Id == (int) MsgType_wzb.login_CNF)
            {
                Debug.Log("Login");
            }
        }
    }
    // Use this for initialization

}
