using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBattle : MonoBehaviour {

    public void OnMove(Vector2 axie)
    {
        Debug.Log("x:" + axie.x.ToString() + "  y:" + axie.y.ToString());
    }

    public void OnDown()
    {
        Debug.Log("OnDown");
    }
}
