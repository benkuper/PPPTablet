using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

public class QuizzAnswer : MonoBehaviour
     , IPointerClickHandler // 2
{

    public delegate void AnswerSelected(QuizzAnswer a);
    public AnswerSelected answerSelectedHandler;

    public bool isSelected;

    public bool isGood;

    Image bgImage;
   
    Vector2 initPos;
    bool canBeClicked;

    public int answerID;

    // Use this for initialization
    public virtual void Awake()
    {
        bgImage = GetComponent<Image>();
       
        initPos = GetComponent<RectTransform>().anchoredPosition;
    }

    // Update is called once per frame
    public virtual void Update()
    {

    }


    public void setSelected(bool value)
    {
        isSelected = value;
        bgImage.DOColor(isSelected ? new Color(1, .5f, 0, 1) : Color.grey, .3f);
    }

    public virtual void setData(string gameID, int questionID, string answer, bool isGood)
    {
        setSelected(false);
        this.isGood = isGood;
        canBeClicked = true;
        GetComponent<RectTransform>().DOAnchorPos(initPos, .3f);
    }

    public bool showAnswer()
    {
        if (isGood || isSelected) bgImage.DOColor(isGood ? Color.green : Color.red, .3f);
        canBeClicked = false;
        return isGood && isSelected;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canBeClicked) return;
        if (answerSelectedHandler != null) answerSelectedHandler(this);
    }

}
