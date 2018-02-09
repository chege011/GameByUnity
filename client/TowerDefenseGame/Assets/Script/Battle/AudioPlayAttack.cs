using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;

public class AudioPlayAttack : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayAudioAttack()
    {
        UIManager.audioManager.PlayAudioAttack();
    }
}
