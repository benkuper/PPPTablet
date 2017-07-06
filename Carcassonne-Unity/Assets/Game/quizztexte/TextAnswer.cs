using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextAnswer : QuizzAnswer
{

    Text answerText;

    public override void Awake()
    {
        base.Awake();
        answerImages.Add(transform.Find("AnswerTextPanel").GetComponent<Image>());
        answerImages.Add(transform.Find("AnswerNumPanel").GetComponent<Image>());

        answerText = transform.Find("AnswerTextPanel/AnswerText").GetComponent<Text>();
    }

    public override void setData(string gameID, int questionID, string answer, bool isGood)
    {
        base.setData(gameID, questionID, answer, isGood);
        answerText.text = answer;
    }
}
