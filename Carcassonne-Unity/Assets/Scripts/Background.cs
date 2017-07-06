using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Background : MonoBehaviour, ITextureReceiver {

    Canvas canvas;
    RawImage image;
    Text infosText;

    public bool isEnabled;

	// Use this for initialization
	void Start () {
        canvas = GetComponent<Canvas>();
        image = GetComponentInChildren<RawImage>();
        infosText = GetComponentInChildren<Text>();
        StartCoroutine(AssetManager.loadTexture("fond.png", "bg", this));

	}
	
	// Update is called once per frame
	void Update () {
        infosText.text = "ID tablette : "+TabletIDManager.getTabletID()+"\nApp version : "+Application.version;
        setEnabled(!GameMaster.instance.gameIsPlaying() && !ScoreManager.instance.isShowing() && !MediaPlayer.instance.mediaIsPlaying());
	}

    void setEnabled(bool value)
    {
        if (isEnabled == value) return;
        isEnabled = value;
        if(isEnabled)
        {
            image.color = Color.black;
            image.DOColor(Color.white, .5f);
        }

        canvas.enabled = value;
    }

    void ITextureReceiver.textureReady(string textureID, Texture2D tex)
    {
        if(textureID == "bg")
        {
            image.texture = tex;
        }
    }
}
