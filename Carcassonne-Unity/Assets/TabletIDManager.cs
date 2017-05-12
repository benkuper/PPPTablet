using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletIDManager : MonoBehaviour {

    public int tabletID;
    public static TabletIDManager instance;

	// Use this for initialization
	void Awake () {
        instance = this;	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static int getTabletID()
    {
        return instance.tabletID;
    }

    public static void setTabletID(int value)
    {
        instance.tabletID = value;
    }
}
