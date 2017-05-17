using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Game : OSCControllable {

    public string id;

    public int score;

    public delegate void GameEndEvent(Game g);
    public GameEndEvent gameEndEvent;

    protected Canvas _canvas;

    public bool isPlaying;

    public void setID(string _id)
    {
        id = _id;
        oscName = id;
    }

    public virtual void Awake()
    {
        _canvas = GetComponentInChildren<Canvas>();
    }

    // Use this for initialization
    public virtual void Start () {
        loadSettings();
	}

    public virtual void launchGame()
    {
        score = 0;
        loadSettings();
        gameObject.SetActive(true);

        if(_canvas != null)
        {
            for (int i = 0; i < _canvas.transform.childCount; i++)
            {
                Transform t = _canvas.transform.GetChild(i);
                t.localScale = Vector3.zero;
                t.DOScale(Vector3.one, .3f + i * .01f).SetDelay(i * .03f);
            }
        }

        Invoke("startGame", 1f);
    }

    public virtual void startGame()
    {

        //start really here
        isPlaying = true;
    }

    public virtual void endGame()
    {
        isPlaying = false;

        DOTween.KillAll();

        ScoreManager.saveScore(this);

        if (_canvas != null)
        {
            for (int i = 0; i < _canvas.transform.childCount; i++)
            {
                Transform t = _canvas.transform.GetChild(i);
                t.DOScale(Vector3.zero, .3f + i * .01f).SetDelay(i * .03f);
            }
        }

        Invoke("dispatchEnd", 1f);
    }

    public void dispatchEnd()
    {
        if (gameEndEvent != null) gameEndEvent(this);
    }

    public virtual void killGame()
    {
        gameObject.SetActive(false);
    }
    
    public void loadSettings()
    {
        string gameConfig = AssetManager.getGameConfig(id);
        JsonUtility.FromJsonOverwrite(gameConfig, this);
    }
	
	// Update is called once per frame
	public virtual void Update () {
		
	}


}
