using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Code2DConsignes : Code2DGame
{
    //from config
    public float poubelleSwitchTime;
    public int poubelleJauneStartID;
    public int poubelleVerteStartID;
    public int poubelleGriseStartID;

    float lastSwitchTime;

    public int poubelleOffset;
    public RectTransform[] poubelles; 

    public Vector2[] poubelleScales;
    public Vector3[] poubellePositions;

    string[] consignes = new string[] { "jaune", "verte", "grise" };

    public override void Awake()
    {
        base.Awake();


        poubellePositions = new Vector3[3];
        poubelleScales = new Vector2[3];

        for (int i = 0; i < 3; i++)
        {
            poubellePositions[i] = poubelles[i].position;
            poubelleScales[i] = new Vector2(poubelles[i].rect.width, poubelles[i].rect.height);
        }

        
    }

    public override void launchGame()
    {
        base.launchGame();
        switchPoubelles();
        lastSwitchTime = Time.time;
    }


    public override void Update()
    {
        base.Update();
        if(Time.time > lastSwitchTime + poubelleSwitchTime)
        {
            switchPoubelles();
        }
    }


    [OSCMethod("switch")]
    public void switchPoubelles()
    {
        AudioPlayer.instance.play("turn.mp3");

        poubelleOffset = (poubelleOffset + 1) % 3;
        for(int i=0;i<3;i++)
        {
            int index = (i + 3 - poubelleOffset) % 3;
            poubelles[i].DOMove(poubellePositions[index], .5f);
            poubelles[i].DOSizeDelta(poubelleScales[index], .5f);
        }
        lastSwitchTime = Time.time;

        consigneText.text = initConsigneText+"vont dans la poubelle " + consignes[poubelleOffset] + " !";
    }

    public override bool checkIdIsGood(int id)
    {
        int group = -1;
        if (id >= poubelleGriseStartID) group = 2;
        else if (id >= poubelleVerteStartID) group = 1;
        else if (id >= poubelleJauneStartID) group = 0;

        Debug.Log("Check id " + id + " : " + group + " / " + poubelleOffset);
        if(group >= 0)
        {
            return group == poubelleOffset;
        }
        else
        {
            return false;
        }
    }

    public override void resetCode()
    {
        base.resetCode();
        consigneText.text = initConsigneText+"vont dans la poubelle " + consignes[poubelleOffset] + " !";
    }
}
