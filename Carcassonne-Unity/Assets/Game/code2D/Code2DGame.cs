using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vexpot.Arcolib;
using DG.Tweening;

public class Code2DGame : Game
{


    //config.json
    public float tempsSelection;
    public float tempsJeu;
    public float tempsAffichageReponse;
    public string texteBonneReponse;
    public string texteMauvaiseReponse;

    float timeAtLaunch;

    public float maxDetectionDistance;

    public Color selectColor;
    public Color goodColor;
    public Color badColor;

    public RawImage camImage;
    public Text scoreText;
    public Text countDown;
    
    public Transform scoreBG;
    Vector3 initScoreBGPos;
    public int pixelsScoreParPoint = 10;

    public Image circle;
    public Text consigneText;
    protected string initConsigneText;

    public Image mireImage;
    public Color mireColor;

    BarcodeManager bm;

    int currentSymbol;
    bool symbolIsOutside;
    int lastValidatedSymbol;

    public override void Awake()
    {
        base.Awake();

        camImage = _canvas.transform.Find("CamImage").GetComponent<RawImage>();
        scoreText = _canvas.transform.Find("ScoreBG/Score").GetComponent<Text>();
        scoreBG = _canvas.transform.Find("ScoreBG");
        countDown = _canvas.transform.Find("Countdown").GetComponent<Text>();
        circle = _canvas.transform.Find("CamImage/Circle").GetComponent<Image>();
        mireImage = _canvas.transform.Find("Mire").GetComponent<Image>();
        consigneText = _canvas.transform.Find("ConsigneText").GetComponent<Text>();
        initConsigneText = consigneText.text;

        initScoreBGPos = scoreBG.position;

        circle.enabled = false;

        mireColor = mireImage.color;

        scoreText.text = "0";


        bm = GetComponent<BarcodeManager>();
        bm.codeDetected += codeDetected;
        bm.codeUndetected += codeUndetected;
        bm.codeUpdated += codeUpdated;

        currentSymbol = -1;
    }

    public override void launchGame()
    {
        base.launchGame();

        bm.init();
        camImage.texture = bm.camInput.texture;

        timeAtLaunch = Time.time;
    }

    public override void Update()
    {
        base.Update();

        if (!isPlaying) return;

        if (Input.GetKeyDown("g")) selectId(1);
        if (Input.GetKeyDown("h")) selectId(25);
        if (Input.GetKeyDown("j")) selectId(35);
        if (Input.GetKeyDown("k")) selectId(60);

        float timeLeft = timeAtLaunch + tempsJeu - Time.time;

        //float relTime = timeLeft / tempsJeu;

        if (timeLeft <= 0) endGame();
        
        countDown.text = StringUtil.timeToCountdownString(timeLeft);

        if (currentSymbol > -1 && currentSymbol != lastValidatedSymbol)
        {

            if (symbolIsOutside) circle.fillAmount = 0;
            else circle.fillAmount += Time.deltaTime / tempsSelection;

            if (circle.fillAmount >= 1) selectId(currentSymbol);
        }

       
    }

    public void codeDetected(Symbol s)
    {
        Debug.Log("Detected : " + s.id + " /" + s.center);
        circle.rectTransform.DOAnchorPos(getPosForSymbol(s), .01f);

        //DOTween.To(() => circle.fillAmount, x => circle.fillAmount = x, 1, tempsSelection).SetEase(Ease.Linear).SetId("circle").OnComplete(() => selectId(s.id));
    }

    public void codeUndetected(Symbol s)
    {
        Debug.Log("Undetected : " + s.id);
        //DOTween.Kill("circle");
        unfocusSymbol(s);
    }

    public void codeUpdated(Symbol s)
    {
        Vector2 targetPos = getPosForSymbol(s);

        float dist = Vector2.Distance(targetPos, new Vector2(camImage.rectTransform.rect.width/2, camImage.rectTransform.rect.height/2));
        symbolIsOutside = dist > maxDetectionDistance;

        if (!symbolIsOutside)
        {
            if (currentSymbol != s.id && lastValidatedSymbol != s.id) //Identify here
            {
                focusSymbol(s);
            }
        }
        else
        {
            if (currentSymbol == s.id)
            {
                unfocusSymbol(s);
            }
        }

        circle.rectTransform.DOAnchorPos(targetPos, .3f);
    }

    public void focusSymbol(Symbol s)
    {
        Debug.Log("SYMBOL FOCUS");
        AudioPlayer.instance.play("bip.mp3");

        circle.color = selectColor;

        circle.enabled = true;
        circle.fillAmount = 0;

        currentSymbol = s.id;
    }

    public void unfocusSymbol(Symbol s)
    {
        Debug.Log("Symbol UNFOCUS");
        circle.fillAmount = -.1f;
        circle.enabled = false;
        currentSymbol = -1;
        lastValidatedSymbol = -1;
    }

    public Vector2 getPosForSymbol(Symbol s)
    {
        Vector2 relPos = new Vector2(s.center.x / 1280.0f, 1 - (s.center.y / 720.0f));
        return new Vector2(relPos.x * camImage.rectTransform.rect.width, relPos.y * camImage.rectTransform.rect.height);
    }

    public void selectId(int id)
    {
        Debug.Log("select Id : " + id);

        bool isGood = checkIdIsGood(id);

        if (isGood)
        {
            //good
            circle.color = goodColor;
            consigneText.text = texteBonneReponse;
            score++;
        }
        else
        {
            circle.color = badColor;
            consigneText.text = texteMauvaiseReponse;
            score--;
        }

        if (score > 0) scoreText.text = "+" + score;
        else scoreText.text = score.ToString();
        mireImage.color = circle.color;

        //positiveScoreImage.transform.localScale = new Vector3(Mathf.Max(score, 0)*1f / maxScoreBar, 1, 1);
        //negativeScoreImage.transform.localScale = new Vector3(Mathf.Abs(Mathf.Min(score, 0))*1f / maxScoreBar, 1, 1);
        scoreBG.position = initScoreBGPos + Vector3.right * score * pixelsScoreParPoint;

        Invoke("resetCode", tempsAffichageReponse);

        lastValidatedSymbol = currentSymbol;
        currentSymbol = -1;
        
        triggerAnswer(id.ToString(), isGood);
    }

    virtual public void resetCode()
    {
        consigneText.text = initConsigneText;
        mireImage.color = mireColor;
        circle.fillAmount = 0;
        currentSymbol = -1;
    }

    virtual public bool checkIdIsGood(int id)
    {
        //to override
        return true;
    }
}
