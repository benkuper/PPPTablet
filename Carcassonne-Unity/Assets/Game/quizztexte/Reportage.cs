using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Reportage : Game {

    WebCamTexture camTex;
    WebCamDevice device;
    CamCapture camCap;

    Material camImage;
    bool isCapturing;

    public int tempsPreparation;
    public int tempsEnregistrement;
    public string textePreparation;
    public string texteEnregistrement;

    int prepaCountDown;
    int recCountDown;

    GameObject prepaPanel;
    Text prepaText;

    GameObject recPanel;
    Text recText;

    Text Instructions;

    public override void Awake()
    {
        base.Awake();

        camImage = transform.FindChild("CamFeedback").GetComponent<Renderer>().material;

        prepaPanel = _canvas.transform.FindChild("PrepaCountdownPanel").gameObject;
        recPanel = _canvas.transform.FindChild("RecCountdownPanel").gameObject;
        prepaText = prepaPanel.transform.FindChild("Countdown").GetComponent<Text>();
        recText = recPanel.transform.FindChild("Countdown").GetComponent<Text>();

        Instructions = _canvas.transform.FindChild("Instructions").GetComponent<Text>();

        prepaText.text = tempsPreparation.ToString();
        recText.text = tempsEnregistrement.ToString();

        foreach(WebCamDevice d in WebCamTexture.devices)
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

        Instructions.text = textePreparation;

        //init cam without showing
        camTex = new WebCamTexture(device.name);
        camCap.fileName = TabletIDManager.getTabletID() + "-" + id + "_";
        camImage.color = Color.black;
        camImage.DOColor(Color.white, 1f);

        prepaCountDown = tempsPreparation;

        prepaPanel.SetActive(true);
        recPanel.SetActive(false);

        DOTween.To(() => prepaCountDown, x => prepaCountDown = x, 0, prepaCountDown).SetEase(Ease.Linear).OnUpdate(updatePrepaText).OnComplete(startCapture);
    }

    public void updatePrepaText()
    {
        prepaText.text = Mathf.Min(prepaCountDown + 1,tempsPreparation).ToString();
    }

    public void updateRecText()
    {
        recText.text = Mathf.Min(recCountDown + 1, tempsEnregistrement).ToString();
    }

    public void startCapture()
    {
        Debug.Log("Start Capturing");

        Instructions.text = texteEnregistrement;

        recCountDown = tempsEnregistrement;

        prepaPanel.SetActive(false);
        recPanel.SetActive(true);

        camImage.mainTexture = camTex;
        camTex.Play();
        camCap.StartCapturing();
        isCapturing = true;

        DOTween.To(() => recCountDown, x => recCountDown = x, 0, recCountDown).SetEase(Ease.Linear).OnUpdate(updateRecText).OnComplete(stopCapture);
    }

    public void stopCapture()
    {

        Debug.Log("Stop Capturing");
        camCap.StopCapturing();
        camTex.Stop();
        isCapturing = false;

        //Delayed ?
        endGame();
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
        camTex.Stop();
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
