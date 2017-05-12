using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reportage : Game {

    WebCamTexture camTex;
    WebCamDevice device;
    CamCapture camCap;

    Material camImage;
    bool isCapturing;

    public float tempsPreparation;
    public float tempsEnregistrement;

    public override void Awake()
    {
        base.Awake();

        camImage = transform.FindChild("CamFeedback").GetComponent<Renderer>().material;

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

        camTex = new WebCamTexture(device.name);
        camImage.mainTexture = camTex;

        camCap.fileName = TabletIDManager.getTabletID() + "-" + id + "_";

        Invoke("camPlayDelayed", .5f);
        Invoke("startCapture", tempsPreparation);

       
    }

    public void camPlayDelayed()
    {
        Debug.Log("Cam play delayed");
        camTex.Play();
    }

    public override void endGame()
    {
       
        stopCapture();
        
        camTex.Stop();
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

    [OSCMethod("startCapture")]
    public void startCapture()
    {
        Debug.Log("Start Capturing");
        
        camCap.StartCapturing();
        isCapturing = true;

        Invoke("endGame", tempsEnregistrement);
    }

    [OSCMethod("stopCapture")]
    public void stopCapture()
    {
        Debug.Log("Stop Capturing");
        camCap.StopCapturing();
        isCapturing = false;
    }
}
