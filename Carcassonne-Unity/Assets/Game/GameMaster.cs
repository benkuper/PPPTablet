using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    public GameObject[] gamePrefabs;

    public int startGameAtLoad;

    Game currentGame;
    Game currentGameInstance;

	// Use this for initialization
	void Start () {
        setCurrentGame(startGameAtLoad);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Keypad0)) setCurrentGame(0);
        if (Input.GetKeyDown(KeyCode.Keypad1)) setCurrentGame(1);
        if (Input.GetKeyDown(KeyCode.Keypad2)) setCurrentGame(2);
        if (Input.GetKeyDown(KeyCode.Keypad3)) setCurrentGame(3);
        if (Input.GetKeyDown(KeyCode.Keypad4)) setCurrentGame(4);
        if (Input.GetKeyDown(KeyCode.Keypad5)) setCurrentGame(5);
        if (Input.GetKeyDown(KeyCode.Keypad6)) setCurrentGame(6);
	}

    public void setCurrentGame(int gameIndex)
    {
        if (gameIndex < 0 || gameIndex >= gamePrefabs.Length)
        {
            setCurrentGame((Game)null);
            return;
        }

        setCurrentGame(gamePrefabs[gameIndex].GetComponent<Game>());
    }

    public void setCurrentGame(string gameID)
    {

        setCurrentGame(getGameForID(gameID));
    }

    public void setCurrentGame(Game g)
    {
        if (g == currentGame) return;

        if(currentGame != null)
        {
            currentGameInstance.killGame();
            Destroy(currentGameInstance.gameObject);
        }

        currentGame = g;

        if(currentGame != null)
        {
            currentGameInstance = Instantiate(g.gameObject).GetComponent<Game>();
            currentGameInstance.launchGame();
        }
    }

    Game getGameForID(string gameID)
    {
        foreach (GameObject go in gamePrefabs)
        {
            Game g = go.GetComponent<Game>();
            if (g.id == gameID) return g;
        }

        return null;
    }
}
