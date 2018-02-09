using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _footMan_client_;

public class ExcelManagerTest : MonoBehaviour
{

    private bool _isDataOk;
    private bool _isGetOne;

    // Use this for initialization
    void Start ()
    {
        _isDataOk = false;
        _isGetOne = false;
        InitData();
    }
	
	// Update is called once per frame
	void Update () {
	    if (_isDataOk && !_isGetOne)
	    {
            _Human_ footMan;
	        var keys = ExcelManager.footMan_Human.Keys;
	        foreach (var key in keys)
	        {
	            ExcelManager.footMan_Human.TryGetValue(key, out footMan);
	            Debug.Log("FootMan" + footMan.id + ":" + key + " " + footMan.attack);
	        }
            _isGetOne = true;
        }
	}

    private void InitData()
    {
        StartCoroutine(ExcelManager.LoadAll_Enum(progress =>
        {
            if (progress == 1.0f)
            {
                _isDataOk = true;
            }
        }));
        _isDataOk = true;
    }
}
