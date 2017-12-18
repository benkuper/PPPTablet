using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GameData
{
    public GameObject prefab;
    public string id;
}

public class GameMaster : OSCControllable {

    public static GameMaster instance;

    public GameData[] gameData;

    public int startGameAtLoad;

    GameData currentGameData;
    Game currentGameInstance;

    [OSCMethod("go")]
    public void go(string gid)
    {
        Debug.Log("GameMaster GO " + gid);
        setCurrentGame(getGameForID(gid)); //gameIndex 1-7 => 0-6
    }

    private void Awake()
    {
        instance = this;

    }

	// Use this for initialization
	void Start () {
        if(startGameAtLoad > 0) setCurrentGame(gameData[startGameAtLoad]);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Keypad0)) setCurrentGame(gameData[0]);
        if (Input.GetKeyDown(KeyCode.Keypad1)) setCurrentGame(gameData[1]);
        if (Input.GetKeyDown(KeyCode.Keypad2)) setCurrentGame(gameData[2]);
        if (Input.GetKeyDown(KeyCode.Keypad3)) setCurrentGame(gameData[3]);
        if (Input.GetKeyDown(KeyCode.Keypad4)) setCurrentGame(gameData[4]);
        if (Input.GetKeyDown(KeyCode.Keypad5)) setCurrentGame(gameData[5]);
        if (Input.GetKeyDown(KeyCode.Keypad6)) setCurrentGame(gameData[6]);
        if (Input.GetKeyDown(KeyCode.Keypad7)) setCurrentGame(gameData[7]);
        if (Input.GetKeyDown(KeyCode.Keypad8)) setCurrentGame(gameData[8]);
        if (Input.GetKeyDown(KeyCode.Keypad9)) setCurrentGame(gameData[9]);
        if (Input.GetKeyDown(KeyCode.KeypadMultiply)) setCurrentGame(gameData[11]);
        if (Input.GetKeyDown(KeyCode.KeypadPeriod))
        {
            setCurrentGame(null);
            if (Input.GetKey(KeyCode.LeftControl)) ScoreManager.instance.showScore();
        }
    }

    public void setCurrentGameID(string gameID)
    {
        setCurrentGame(getGameForID(gameID));
    }

    public void setCurrentGame(GameData gd)
    {
        if (gd == currentGameData) return;

        MediaPlayer.hide();

        if(currentGameData != null)
        {
            currentGameInstance.gameEndEvent -= gameEnded;
            currentGameInstance.killGame();
            Destroy(currentGameInstance.gameObject);
        }

        currentGameData = gd;

        if (currentGameData != null)
        {
            currentGameInstance = Instantiate(currentGameData.prefab).GetComponent<Game>();
            currentGameInstance.setID(currentGameData.id);
            currentGameInstance.gameEndEvent += gameEnded;
            currentGameInstance.launchGame();
            if (ScoreManager.instance != null) ScoreManager.instance.hideScore();
        }

    }

    GameData getGameForID(string gameID)
    {
        foreach (GameData gd in gameData)
        {
            if (gd.id == gameID) return gd;
        }

        return null;
    }

    public void gameEnded(Game g)
    {
        if(g == currentGameInstance)
        {
            setCurrentGame(null);
            //ScoreManager.instance.showScore(g.id);
        }
    }

    public bool gameIsPlaying()
    {
        return currentGameInstance != null;
    }
}
