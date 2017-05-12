using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class QuizzTextes : 
    QuizzGame
{
    public override void showExplications()
    {
        int index = 0;
        foreach (QuizzAnswer a in answers)
        {
            if (a.isGood)
            {
                a.GetComponent<RectTransform>().DOAnchorPosY(answerPos.y, .3f).SetDelay(1);
            }
            else
            {
                a.GetComponent<RectTransform>().DOAnchorPosX(400, .3f).SetDelay(index * .1f);
            }
            index++;
        }

        explicationsPanel.SetActive(true);
        explicationsText.text = currentQuestion.explication;
        explicationsPanel.GetComponent<RectTransform>().SetPositionAndRotation(new Vector3(explicationsPanel.transform.position.x, initExplicationsPos.y - 800, 0), Quaternion.identity);
        explicationsPanel.GetComponent<RectTransform>().DOAnchorPosY(initExplicationsPos.y, .3f).SetDelay(1);
        explicationsPanel.GetComponent<RectTransform>().DOAnchorPosY(initExplicationsPos.y - 800, .3f).SetDelay(1 + tempsExplication - .5f);


        base.showExplications();
    }


}
