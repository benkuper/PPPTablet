using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

    public string id;
    
    // Use this for initialization
    public virtual void Start () {
        loadSettings();
	}

    public virtual void launchGame()
    {
        loadSettings();
        gameObject.SetActive(true);
    }


    public virtual void endGame()
    {

    }

    public virtual void killGame()
    {
        gameObject.SetActive(false);
    }
    
    public void loadSettings()
    {
        string gameConfig = AssetManager.getGameConfig(id);
        Debug.Log("Game Config : " + gameConfig);
        JsonUtility.FromJsonOverwrite(gameConfig, this);
    }
	
	// Update is called once per frame
	public virtual void Update () {
		
	}


}
