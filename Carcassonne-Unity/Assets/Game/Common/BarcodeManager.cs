using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vexpot.Arcolib;
using Vexpot.Arcolib.Integration;
using System;

[Serializable]
public class SymbolDetection
{
    public Symbol symbol;
    public float lastTimeDetected;

    public void update(Symbol s)
    {
        symbol = s;
        lastTimeDetected = Time.time;
    }
}

public class BarcodeManager : MonoBehaviour
{

    public delegate void CodeEvent(Symbol s);

    public CodeEvent codeDetected;
    public CodeEvent codeUndetected;
    public CodeEvent codeUpdated;

    const int maxDetections = 1;

    [SerializeField]
    public List<SymbolDetection> detectedMarkers;

    public float safeUndetectTime; //time at which the detection event will be dispatched, to avoid noise-detection

    //AR
    public WebcamTextureInput camInput;
    public BarcodeTracker tracker;
    public DeviceCalibration calib;
    public TrackerOptions options;
    public SymbolFrame frame;


    // Use this for initialization
    void Awake()
    {
        detectedMarkers = new List<SymbolDetection>();
        
        calib = new DeviceCalibration(1280, 720);
        options = new TrackerOptions(maxDetections);
        tracker = new BarcodeTracker(calib, options);

        
    }

    public void init()
    {
        camInput = new WebcamTextureInput(CameraPosition.Back, 1280, 720, 30);
        camInput.GrabFrame();

        tracker.input = camInput;
        tracker.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (camInput == null) return;

        camInput.GrabFrame();

        bool result = tracker.QueryFrame(ref frame);

        if (result)
        {
            for (int i = 0; i < frame.symbolCount; i++)
            {
                Symbol s = frame[i];

                SymbolDetection sd = getSD(s.id);
                if(sd == null && detectedMarkers.Count < maxDetections)
                {
                    sd = new SymbolDetection();
                    detectedMarkers.Add(sd);
                    codeDetected(s);
                }

                if(sd != null)
                {
                    sd.update(s);
                    codeUpdated(sd.symbol);
                }
               
            }
        }

        List<SymbolDetection> symbolsToRemove = new List<SymbolDetection>();

        for (int i = 0; i < detectedMarkers.Count; i++)
        {
            float undetectedTime = Time.time - detectedMarkers[i].lastTimeDetected;
            if (undetectedTime > safeUndetectTime)
            {
                symbolsToRemove.Add(detectedMarkers[i]);
            }
        }

        for(int i=0;i<symbolsToRemove.Count;i++)
        {
            detectedMarkers.Remove(symbolsToRemove[i]);
            codeUndetected(symbolsToRemove[i].symbol);
        }

    }

    public SymbolDetection getSD(int id)
    {
        foreach(SymbolDetection sd in detectedMarkers)
        {
            if (sd.symbol.id == id) return sd;
        }

        return null;
    }

    private void OnDestroy()
    {
        camInput.Close();
        tracker.Stop();
    }

}
