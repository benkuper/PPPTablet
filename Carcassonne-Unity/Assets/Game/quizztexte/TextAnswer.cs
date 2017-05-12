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
        answerText = transform.FindChild("AnswerTextPanel/AnswerText").GetComponent<Text>();
    }

    public override void setData(string gameID, int questionID, string answer, bool isGood)
    {
        base.setData(gameID, questionID, answer, isGood);
        answerText.text = answer;
    }
}
