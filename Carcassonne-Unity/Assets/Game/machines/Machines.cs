using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vexpot.Arcolib;
using DG.Tweening;
using System;


[Serializable]
public class MachineData
{
    public string nom;
    public float positionX;
    public float positionY;
    public float positionZ;
    public float rotationX;
    public float rotationY;
    public float scale;
}

public class Machines : Game {

    public float tempsJeu;

    float timeAtLaunch = 0;
    BarcodeManager bm;

    RawImage camImage;
    Text countDown;

    public Machine[] machinePrefabs;
    public MachineData[] machines;

    Machine activeMachine;

    public Toggle outside;
    public Toggle dechetsActive;

    public Material[] outsideMats;
    public Material metalMat;
    public Color insideColor;
    public Color dechetsColor;

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
        updateMatColor();


        foreach (Machine m in machinePrefabs)
        {
            m.gameObject.SetActive(false);
           
        }
    }

    public override void Start()
    {
        base.Start();
        
    }

    public override void launchGame()
    {
        base.launchGame();
        timeAtLaunch = Time.time;

        bm.init();
        camImage.texture = bm.camInput.texture;

        for (int i = 0; i < machinePrefabs.Length; i++) machinePrefabs[i].loadMachineSettings(machines[i]);
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
        int targetID = s.id % machinePrefabs.Length;
        Debug.Log("Id");
        setActiveMachine(machinePrefabs[targetID]);
    }

    public void codeUndetected(Symbol s)
    {
        Debug.Log("Undetected : " + s.id);
        setActiveMachine(null);
    }

    public void codeUpdated(Symbol s)
    {
        int targetID = s.id % machinePrefabs.Length;
        if (machinePrefabs[targetID] == activeMachine)
        {
            s.ApplyTransformMatrixTo(activeMachine.transform);
            //activeMachine.loadMachineSettings();
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
            activeMachine.animate();
            activeMachine.setDechets(dechetsActive.isOn);
            //activeMachine.setOutside(dechetsActive.isOn || outside.isOn);
        }
    }

    public void setOutside(bool value)
    {
        foreach (Material m in outsideMats) m.DOFloat(value ?0:1f, "_Cutoff", 1);

        updateMatColor();
    }

    public void setDechets(bool value)
    {
        Debug.Log("Set dechets :" + value + ", machine is active ? " + (activeMachine != null));
       foreach(Machine m in machinePrefabs) m.setDechets(dechetsActive.isOn);

        updateMatColor();
    }

    public void updateMatColor()
    {
        Color tc = Color.black;
        if(!outside.isOn || dechetsActive.isOn)
        {
            if (dechetsActive.isOn) tc = dechetsColor;
            else tc = insideColor;            
        }

        metalMat.DOColor(tc, "_EmissionColor", 1);
    }
}
