using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class QuizzImages : 
    QuizzGame {

    public override void showExplications()
    {
        int index = 0;
        foreach (QuizzAnswer a in answers)
        {
            if (a.isGood)
            {
                a.GetComponent<RectTransform>().DOAnchorPosX(answerPos.x, .3f).SetDelay(1);
            }
            else
            {
                a.GetComponent<RectTransform>().DOAnchorPosY(answerPos.y-400, .3f).SetDelay(index * .1f);
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
}
