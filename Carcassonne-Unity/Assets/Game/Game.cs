using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Game : OSCControllable {

    public string id;

    public int score;

    public bool autoLaunch;

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
        if (autoLaunch) launchGame();
	}

    public virtual void launchGame()
    {
        score = 0;
        loadSettings();
        gameObject.SetActive(true);
        AudioPlayer.instance.stop();

        if (_canvas != null)
        {
            for (int i = 0; i < _canvas.transform.childCount; i++)
            {
                Transform t = _canvas.transform.GetChild(i);
                //Vector3 targetScale = t.localScale;
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

        AudioPlayer.instance.play("end");

        Invoke("dispatchEnd", 1f);
    }

    public void dispatchEnd()
    {
        if (gameEndEvent != null) gameEndEvent(this);
    }

    public virtual void killGame()
    {
        AudioPlayer.instance.stop();

        gameObject.SetActive(false);
    }
    
    public void loadSettings()
    {
        string gameConfig = AssetManager.getGameConfig(id);
        JsonUtility.FromJsonOverwrite(gameConfig, this);
    }
	

    public void triggerAnswer(string answerID, bool isGood)
    {
        string file = isGood ? "yes" : "wrong";
        AudioPlayer.instance.play(file);

        UnityOSC.OSCMessage m = new UnityOSC.OSCMessage(OSCMaster.getBaseAddress()+id+"/"+(isGood?"yes":"wrong"));
        m.Append<float>(1);
        OSCMaster.sendMessage(m);
    }

	// Update is called once per frame
	public virtual void Update () {
        if (Input.GetKeyDown("e")) endGame();
	}


}
