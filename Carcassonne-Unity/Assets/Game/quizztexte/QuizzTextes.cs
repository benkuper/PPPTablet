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

        RectTransform r = explicationsPanel.GetComponent<RectTransform>();
        explicationsPanel.SetActive(true);
        explicationsText.text = currentQuestion.explication;

        r.SetPositionAndRotation(new Vector3(explicationsPanel.transform.position.x, initExplicationsPos.y - 800, 0), Quaternion.identity);

        r.DOAnchorPosY(initExplicationsPos.y, .3f).SetDelay(1);
        r.DOLocalRotateQuaternion(initExplicationsRot, .2f).SetDelay(1);

        r.DOAnchorPosY(initExplicationsPos.y - 800, .3f).SetDelay(1 + tempsExplication - .5f);
        

        base.showExplications();
    }

    [OSCMethod("laserSelect")]
    public void selectAnswerFromLaser(int laserID)
    {
        Debug.Log("Laser select ! " + laserID);
        int groupID = (int)Mathf.Floor(laserID / 3); // 3 lasers par groupe
        int modTabletID = (TabletIDManager.getTabletID() - 1) % 4; //4 groupes

        Debug.Log("Laser select : " + laserID + " / groupID = " + groupID + " / modTabletID = " + modTabletID);

        if (groupID == modTabletID)
        {
            int targetAnswer = laserID % 3;
            if (targetAnswer < 0 || targetAnswer > answers.Length) return;
            answerSelected(answers[targetAnswer]);
        }
    }
}
