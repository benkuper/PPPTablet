using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Reportage : Game {

    WebCamTexture camTex;
    Texture2D feedbackTex;

    WebCamDevice device;
    CamCapture camCap;

    RawImage camImage;
    public Material camPlaneMat;
    bool isCapturing;
    public GameObject camFeedback;

    public int tempsPreparation;
    public int tempsEnregistrement;
    public string textePreparation;
    public string texteEnregistrement;

    int prepaCountDown;
    int recCountDown;

    GameObject prepaPanel;
    //Text prepaText;

    
    GameObject recPanel;
    Text recText;

    Text titreText;
    //Text Instructions;


    public override void Awake()
    {
        base.Awake();

        camImage = transform.Find("Canvas/Rec/CamFeedback").GetComponent<RawImage>();

        
        prepaPanel = _canvas.transform.Find("Prepa").gameObject;
        recPanel = _canvas.transform.Find("Rec").gameObject;
       

        //prepaText = prepaPanel.transform.Find("Countdown").GetComponent<Text>();
        recText = recPanel.transform.Find("Countdown").GetComponent<Text>();

        //Instructions = _canvas.transform.Find("Instructions").GetComponent<Text>();
        titreText = _canvas.transform.Find("TitreText").GetComponent<Text>();

        //prepaText.text = tempsPreparation.ToString();
        recText.text = tempsEnregistrement.ToString();

        foreach (WebCamDevice d in WebCamTexture.devices)
        {
#if UNITY_EDITOR
            device = d;
            break;
#else
            if(d.isFrontFacing)
            {
                device = d;
                break;
            }
#endif
        }


        camCap = GetComponentInChildren<CamCapture>();
        camCap.videoDir = AssetManager.getFolderPath("reportages/tab" + TabletIDManager.getTabletID()+"/");

    }


    public override void launchGame()
    {
        base.launchGame();

        if (prepaPanel == null) return;

        prepaPanel.SetActive(true);
        recPanel.SetActive(false);

        //Instructions.text = textePreparation;

        titreText.text = textePreparation;
        AudioPlayer.instance.play("wait",AudioPlayer.SourceType.BG);
        MediaPlayer.hide(); 

        //init cam without showing, delayed for sanity if coming after a media player
        StartCoroutine(initCam());

        prepaCountDown = tempsPreparation;

        prepaPanel.SetActive(true);
        recPanel.SetActive(false);

        //Debug.Log("prepa countdown : " + prepaCountDown);

        DOTween.Kill("cd2");
        DOTween.To(() => prepaCountDown, x => prepaCountDown = x, 0, prepaCountDown).SetId("cd2").SetEase(Ease.Linear).OnComplete(startCapture);//.OnUpdate(updatePrepaText);
    }


    public IEnumerator initCam()
    {
        Debug.Log("Init cam");
        yield return new WaitForSeconds(prepaCountDown / 2.0f);

        camTex = new WebCamTexture(device.name);
        camCap.fileName = TabletIDManager.getTabletID() + "-" + id + "_";
        camImage.color = Color.black;
        camImage.DOColor(Color.white, 1f);


        yield return new WaitForSeconds(.5f);
        camPlaneMat.mainTexture = camTex;
    }

    /*
    public void updatePrepaText()
    {
        prepaText.text = Mathf.Min(prepaCountDown + 1,tempsPreparation).ToString();
    }
    */

    public void updateRecText()
    {
        if (recText == null) return;
        recText.text = StringUtil.timeToCountdownString(Mathf.Min(recCountDown + 1, tempsEnregistrement));
    }

    public void startCapture()
    {
        Debug.Log("Start Capturing");

        if (prepaPanel == null) return;


        prepaPanel.SetActive(false);
        recPanel.SetActive(true);

        AudioPlayer.instance.stop();
        //Instructions.text = texteEnregistrement;
        titreText.text = texteEnregistrement;

        recCountDown = tempsEnregistrement;


        camImage.texture = camTex;
        camTex.Play();
       
        camCap.StartCapturing();
        isCapturing = true;

        DOTween.Kill("countdown", false); 
        DOTween.To(() => recCountDown, x => recCountDown = x, 0, recCountDown).SetId("countdown").SetEase(Ease.Linear).OnUpdate(updateRecText).OnComplete(stopCapture);
    }

    public void stopCapture()
    {
        camCap.StopCapturing();
        camTex.Stop();

        isCapturing = false;

        //Delayed ?
        endGame();

        Debug.Log("Stop Capturing");
        if (camFeedback != null && camFeedback.GetComponent<Renderer>() != null)
        {
            camFeedback.GetComponent<Renderer>().enabled = false;
        }
    }

    public override void endGame()
    {
        camImage.DOColor(Color.black, 1);
        base.endGame();
    }

    private void OnDestroy()
    {
        if (isCapturing)
        {
            stopCapture();
        }
        if (camTex != null)
        {

            camTex.Stop();
        }
        else
        {
            Debug.LogWarning("CamTex null !");
        }
    }

    // Use this for initialization
    public override void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	public override void Update () {
        base.Update();
	}

    
}
