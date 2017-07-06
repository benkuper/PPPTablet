using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityOSC;

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

    public enum ScoreView{NONE, EQUIPE, VISITE };

    public ScoreView view;

    public static int NUM_PLAYERS = 12;

    public GameObject[] jeux;
    public Text totalText;

    public Text scoreTitre;

    Text[] scoreTexts;
    Image[] scoreBGs;

    public ScoreData[] scores;

    public ScoreSaveData scoreSaveData;

    public Color normalColor;
    public Color focusColor;

    Canvas canvasEquipe;
    Canvas canvasVisite;
    Canvas currentCanvas;

    int[][] visiteScores;

    public static ScoreManager instance;
    
    void Awake()
    {
       
        canvasEquipe = transform.Find("Canvas_Equipe").GetComponent<Canvas>();
        canvasVisite = transform.Find("Canvas_Visite").GetComponent<Canvas>();

        instance = this;

        scoreTexts = new Text[jeux.Length];
        scoreBGs = new Image[jeux.Length];

       
        scores = new ScoreData[jeux.Length];
        scoreSaveData = new ScoreSaveData();
        scoreSaveData.scores = new int[scores.Length];

        visiteScores = new int[12][]; //12 tablettes
        for (int i = 0; i < visiteScores.Length; i++)
        {
            visiteScores[i] = new int[3]; //3 modules
        }

        for (int i=0;i<jeux.Length;i++)
        {
            scoreTexts[i] = jeux[i].transform.Find("Score").GetComponent<Text>();
            scoreBGs[i] = jeux[i].GetComponent<Image>();

            scores[i] = new ScoreData();
            
            scores[i].gameID = jeux[i].name;

        }

        //resetScores();


        setView(ScoreView.NONE);
    }


    [OSCMethod("reset")]
    public void resetScores()
    {
        for(int i=0;i<scores.Length;i++)
        {
            scores[i].score = -1000;
            scoreSaveData.scores[i] = -1000;
        }
        saveToFile();
    }

    [OSCMethod("resetGame")]
    public void resetScoresFor(string gid)
    {
        for (int i = 0; i < scores.Length; i++)
        {
            if(scores[i].gameID == gid)
            {
                scores[i].score = -1000;
                scoreSaveData.scores[i] = -1000;
            }
            
        }
        saveToFile();
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
                saveToFile();
                return;
            }
            index++;
        }
           
    }

    public static void saveToFile()
    {
        string data = JsonUtility.ToJson(instance.scoreSaveData, true);
        AssetManager.writeFileData("scores", getScoreFileName(), data);
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

        AudioPlayer.instance.stop();


        scoreTitre.text = "Score de l'équipe " + TabletIDManager.getTabletID();

        GameMaster.instance.setCurrentGame(null);
        MediaPlayer.instance.stop();

        setView(ScoreView.EQUIPE);
        loadScores();

        int globalScore = 0;
        bool hasFinishedAllGames = true;
        for(int i=0;i<jeux.Length;i++)
        {
            scoreBGs[i].color = (focusJeuID == scores[i].gameID) ? focusColor : normalColor;
            bool hasFinished = scores[i].score != -1000;
            if (hasFinished) scoreTexts[i].text = scores[i].gameID.Contains("reportage") ? "OK" : scores[i].score.ToString();
            else
            {
                scoreTexts[i].text = "-";
                hasFinishedAllGames = false;
            }
            globalScore += scores[i].score;
        }

        totalText.text = hasFinishedAllGames ? globalScore.ToString() : "-";

        for (int i=0;i< canvasEquipe.transform.childCount; i++)
        {
            Transform t = currentCanvas.transform.GetChild(i);
            t.localScale = Vector3.zero;
            t.DOScale(Vector3.one, .3f + i * .01f).SetDelay(i * .03f);
        }

    }

    [OSCMethod("showVisite")]
    public void showScoreVisite()
    {
        setView(ScoreView.VISITE);
        for (int i = 0; i < currentCanvas.transform.childCount; i++)
        {
            Transform t = currentCanvas.transform.GetChild(i);
            t.localScale = Vector3.zero;
            t.DOScale(Vector3.one, .3f + i * .01f).SetDelay(i * .03f);
        }
    }

    [OSCMethod("showEquipe")]
    public void showScoreEquipe()
    {
        setView(ScoreView.EQUIPE);
        for (int i = 0; i < currentCanvas.transform.childCount; i++)
        {
            Transform t = currentCanvas.transform.GetChild(i);
            t.localScale = Vector3.zero;
            t.DOScale(Vector3.one, .3f + i * .01f).SetDelay(i * .03f);
        }
    }

    [OSCMethod("setVisiteScore")]
    public void setVisiteScore(int tabID, int score1, int score2, int score3)
    {
        if (tabID >= visiteScores.Length) return;

        visiteScores[tabID][0] = score1;
        visiteScores[tabID][1] = score2;
        visiteScores[tabID][2] = score3;

        for (int i = 0;i<3;i++)
        {
            Text t = canvasVisite.transform.Find("Module" + (i + 1) + "/Score" + (tabID + 1)).GetComponent<Text>();
            t.text = visiteScores[tabID][i].ToString();
        }

        Text tt = canvasVisite.transform.Find("Total/Score" + (tabID + 1)).GetComponent<Text>();
        tt.text = (score1 + score2 + score3).ToString();

    }

    [OSCMethod("sendVisiteScore")]
    public void sendVisiteScore()
    {
        if (TabletIDManager.getTabletID() > NUM_PLAYERS) return;

        int scoreModule1 = scores[0].score + scores[1].score + scores[2].score + scores[3].score;
        int scoreModule2 = scores[4].score + scores[5].score;
        int scoreModule3 = scores[6].score + scores[7].score + scores[8].score + scores[9].score + scores[10].score;

        OSCMessage m = new OSCMessage("/score");
        m.Append(TabletIDManager.getTabletID());
        m.Append(scoreModule1);
        m.Append(scoreModule2);
        m.Append(scoreModule3);
        OSCMaster.sendScoreMessage(m);

        Debug.Log("Visite score sent for tablet " + TabletIDManager.getTabletID());
    }

    [OSCMethod("hide")]
    public void hideScore()
    {
        if(currentCanvas != null)
        {
            for (int i = 0; i < currentCanvas.transform.childCount; i++)
            {
                Transform t = currentCanvas.transform.GetChild(i);
                t.DOKill();
                t.DOScale(Vector3.zero, .2f + i * .01f).SetDelay(i * .01f);
            }
        }
        Invoke("hideCanvas", 1f);
    }

    public void hideCanvas()
    {
        setView(ScoreView.NONE);
    }

    public static string getScoreFileName()
    {
        return "score_tab" + TabletIDManager.getTabletID() + ".json";
    }

    public bool isShowing()
    {
        return canvasEquipe.isActiveAndEnabled || canvasVisite.isActiveAndEnabled;
    }

    public static void setView(ScoreView newView)
    {
        instance.view = newView;

        switch(instance.view)
        {
            case ScoreView.NONE:
                instance.canvasVisite.gameObject.SetActive(false);
                instance.canvasEquipe.gameObject.SetActive(false);
                instance.currentCanvas = null;
                break;

            case ScoreView.EQUIPE:
                instance.canvasVisite.gameObject.SetActive(false);
                instance.canvasEquipe.gameObject.SetActive(true);
                instance.currentCanvas = instance.canvasEquipe;
                break;

            case ScoreView.VISITE:
                instance.canvasVisite.gameObject.SetActive(true);
                instance.canvasEquipe.gameObject.SetActive(false);
                instance.currentCanvas = instance.canvasVisite;
                break;

        }
    }
}
