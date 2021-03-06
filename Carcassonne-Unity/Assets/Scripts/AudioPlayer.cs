﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour, IAudioReceiver {

    public static AudioPlayer instance;

    AudioSource[] sources;
    AudioSource fxSource;
    AudioSource bgSource;
    
    public enum SourceType { FX, BG, ALL};

    public string extension;
    
    private void Awake()
    {
        instance = this;
        sources = GetComponents<AudioSource>();
        fxSource = sources[0];
        bgSource = sources[1];
    }
    
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void play(string fileName, SourceType type = SourceType.FX)
    {
        //string mediaPath = AssetManager.getMediaPath(fileName);

        StartCoroutine(AssetManager.loadAudio(fileName+"."+extension, type == SourceType.FX?"fx":"bg", this));
    }

    public void stop(SourceType type = SourceType.ALL)
    {
        Debug.Log("Audio stop !");
        if (type == SourceType.ALL) foreach (AudioSource s in sources) s.Stop();
        else if (type == SourceType.FX)
        {
            fxSource.Stop();
            if (fxSource.clip != null)
            {
                fxSource.clip.UnloadAudioData();
                AudioClip.Destroy(fxSource.clip);
                fxSource.clip = null;
            }
        }
        else if (type == SourceType.BG)
        {
            bgSource.Stop();
            if (fxSource.clip != null)
            {
                bgSource.clip.UnloadAudioData();
                AudioClip.Destroy(bgSource.clip);
                bgSource.clip = null;
            }
            
        }


        GC.Collect();
    }

    public void audioReady(string audioID, AudioClip clip)
    {
        if (audioID != "fx" && audioID != "bg") return;

        if(clip != null)
        {
            //Debug.Log("Audio ready : play");
            AudioSource source = audioID == "fx" ? fxSource : bgSource;
            if (source.clip != null)
            {
                source.clip.UnloadAudioData();
                AudioClip.Destroy(source.clip);
            }
            source.clip = clip;
            source.Play();
        }
    }
}
