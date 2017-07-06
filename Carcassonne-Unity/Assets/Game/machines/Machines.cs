using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vexpot.Arcolib;
using DG.Tweening;

public class Machines : Game {

    public float tempsJeu;

    float timeAtLaunch = 0;
    BarcodeManager bm;

    RawImage camImage;
    Text countDown;

    public Machine[] machines;
    Machine activeMachine;

    public Toggle outside;
    public Toggle dechetsActive;

    public Material[] outsideMats;
    public Material metalMat;
    public Color insideColor;

    public override void Awake()
    {
        base.Awake();
        camImage = _canvas.transform.Find("CamImage").GetComponent<RawImage>();
        countDown = _canvas.transform.Find("Countdown").GetComponent<Text>();

        bm = GetComponent<BarcodeManager>();
        bm.codeDetected += codeDetected;
        bm.codeUndetected += codeUndetected;
        bm.codeUpdated += codeUpdated;

        outside.isOn = true;
        dechetsActive.isOn = false;

        foreach (Machine m in machines)
        {
            m.gameObject.SetActive(false);
           
        }
    }

    public override void launchGame()
    {
        base.launchGame();
        timeAtLaunch = Time.time;

        bm.init();
        camImage.texture = bm.camInput.texture;
    }

    public override void Update()
    {
        base.Update();

        if (!isPlaying) return;

        float timeLeft = timeAtLaunch + tempsJeu - Time.time;

        //float relTime = timeLeft / tempsJeu;

        if (timeLeft <= 0) endGame();
        
        countDown.text = StringUtil.timeToCountdownString(timeLeft);
    }

    public void codeDetected(Symbol s)
    {
        AudioPlayer.instance.play("bip.mp3");
        int targetID = s.id % machines.Length;
        setActiveMachine(machines[targetID]);
        
    }

    public void codeUndetected(Symbol s)
    {
        Debug.Log("Undetected : " + s.id);
        setActiveMachine(null);
    }

    public void codeUpdated(Symbol s)
    {
        int targetID = s.id % machines.Length;
        if (machines[targetID] == activeMachine)
        {
            s.ApplyTransformMatrixTo(activeMachine.transform);
            activeMachine.loadMachineSettings();
        }
    }

    public void setActiveMachine(Machine m)
    {
        if(activeMachine != null)
        {
            activeMachine.gameObject.SetActive(false);
        }

        activeMachine = m;

        if(activeMachine != null)
        {
            activeMachine.gameObject.SetActive(true);
            activeMachine.setDechets(dechetsActive.isOn);
            //activeMachine.setOutside(outside.isOn);
        }
    }

    public void setOutside(bool value)
    {
        foreach (Material m in outsideMats) m.DOFloat(value ?0:1f, "_Cutoff", 1);
        metalMat.DOColor(value ?Color.black:insideColor, "_EmissionColor", 1);
        //if (activeMachine != null) activeMachine.setOutside(outside.isOn);
    }

    public void setDechets(bool value)
    {
        Debug.Log("Set dechets :" + value + ", machine is active ? " + (activeMachine != null));
        if (activeMachine != null) activeMachine.setDechets(dechetsActive.isOn);
    }
}
