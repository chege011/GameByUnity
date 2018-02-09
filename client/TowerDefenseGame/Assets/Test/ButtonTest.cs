using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTest : MonoBehaviour {

    public SpriteState sprState = new SpriteState();
    public Button btnMain;

    // Use this for initialization
    void Start ()
    {
        sprState.highlightedSprite = Resources.Load("Image/Button/Press/Demon/1", typeof(Sprite)) as Sprite;
        btnMain.spriteState = sprState;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
