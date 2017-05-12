using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;



[Serializable]
public class ScoreSaveData
{
    public int[] scores;
}

public class ScoreData
{
    public string gameID;
    public int score;
}

public class ScoreManager : OSCControllable {

    public GameObject[] jeux;
    public Text totalText;

    Text[] scoreTexts;
    Image[] scoreBGs;

    public ScoreData[] scores;

    public ScoreSaveData scoreSaveData;

    public Color normalColor;
    public Color focusColor;

    Canvas canvas;

    public static ScoreManager instance;
    
    void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();

        instance = this;

        scoreTexts = new Text[jeux.Length];
        scoreBGs = new Image[jeux.Length];

       
        scores = new ScoreData[jeux.Length];
        scoreSaveData = new ScoreSaveData();
        scoreSaveData.scores = new int[scores.Length];

        for (int i=0;i<jeux.Length;i++)
        {
            scoreTexts[i] = jeux[i].transform.FindChild("Score").GetComponent<Text>();
            scoreBGs[i] = jeux[i].GetComponent<Image>();

            scores[i] = new ScoreData();
            
            scores[i].gameID = jeux[i].name;

        }

        resetScores();


        canvas.gameObject.SetActive(false);
    }
    

    [OSCMethod("reset")]
    void resetScores()
    {
        for(int i=0;i<scores.Length;i++)
        {
            scores[i].score = -1;
            scoreSaveData.scores[i] = -1;
        }
    }

	// Use this for initialization
	void Start () {
        loadScores();
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    public static void saveScore(Game g)
    {
        int index = 0;
        foreach (ScoreData sd in instance.scores)
        {
            if (sd.gameID == g.id)
            {
                instance.scoreSaveData.scores[index] = g.score;
                string data = JsonUtility.ToJson(instance.scoreSaveData,true);
                AssetManager.writeFileData("scores", getScoreFileName(), data);
                return;
            }
            index++;
        }
           
    }

    public void loadScores()
    {   
        string data = AssetManager.getFileData("scores/"+getScoreFileName());
        if(data.Length == 0)
        {
            Debug.Log("Score file is empty");
            return;
        }
        Debug.Log("Load data :" + data);
        JsonUtility.FromJsonOverwrite(data, scoreSaveData);
        for(int i=0;i<scores.Length;i++)
        {
            scores[i].score = scoreSaveData.scores[i];
        }
             
    }

    [OSCMethod("show")]
    public void showScore(string focusJeuID = "")
    {
        CancelInvoke("hideCanvas");

        GameMaster.instance.setCurrentGame(null);

        canvas.gameObject.SetActive(true);
        loadScores();

        int globalScore = 0;
        bool hasFinishedAllGames = true;
        for(int i=0;i<jeux.Length;i++)
        {
            scoreBGs[i].color = (focusJeuID == scores[i].gameID) ? focusColor : normalColor;
            bool hasFinished = scores[i].score >= 0;
            if (hasFinished) scoreTexts[i].text = scores[i].gameID.Contains("reportage") ? "OK" : scores[i].score.ToString();
            else
            {
                scoreTexts[i].text = "-";
                hasFinishedAllGames = false;
            }
            globalScore += scores[i].score;
        }

        totalText.text = hasFinishedAllGames ? globalScore.ToString() : "-";

        for (int i=0;i< canvas.transform.childCount; i++)
        {
            Transform t = canvas.transform.GetChild(i);
            t.localScale = Vector3.zero;
            t.DOScale(Vector3.one, .3f + i * .01f).SetDelay(i * .03f);
        }

    }

    [OSCMethod("hide")]
    public void hideScore()
    {
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            Transform t = canvas.transform.GetChild(i);
            t.DOScale(Vector3.zero, .2f + i * .01f).SetDelay(i * .01f);
        }

        Invoke("hideCanvas", 1f);
    }

    public void hideCanvas()
    {
        instance.canvas.gameObject.SetActive(false);
    }

    public static string getScoreFileName()
    {
        return "score_tab" + TabletIDManager.getTabletID() + ".json";
    }

    public bool isShowing()
    {
        return canvas.isActiveAndEnabled;
    }
}
