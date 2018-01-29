using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class MainConfig : OSCControllable {

    public static MainConfig instance;
    public string remoteIP;

    private void Awake()
    {
        instance = this;

        string config = AssetManager.getConfig();
        Debug.Log("Config : " + config);
        JsonUtility.FromJsonOverwrite(config, this);

        if (OSCMaster.instance != null) OSCMaster.instance.setupClient();
    }
    
    [OSCMethod("quit")]
    public void quitApp()
    {
        Debug.Log("Quit app !");
        Application.Quit();
    }

    [OSCMethod("show")]
    public void showConfig()
    {
        TabletIDManager.instance.show();
    }

    [OSCMethod("hide")]
    public void hideConfig()
    {
        TabletIDManager.instance.hide();
    }

	// Use this for initialization
	void Start () {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static string getRemoteIP()
    {
        if (instance == null) return "";
        return instance.remoteIP;
    }

    public void sendScoreVisite()
    {
        OSCMessage m = new OSCMessage("/all/score/showVisite");
        OSCMaster.sendMessageToRouter(m);
    }
}
