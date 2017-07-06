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

    protected List<Image> answerImages;
   
    Vector2 initPos;
    bool canBeClicked;

    public int answerID;

    Color selectColor;
    Color goodColor;
    Color badColor;

    // Use this for initialization
    public virtual void Awake()
    {

        selectColor = new Color(.1f,.8f,1);
        goodColor = new Color(.1f,.9f,.2f);
        badColor = new Color(.9f,.2f,0);

        initPos = GetComponent<RectTransform>().anchoredPosition;
        answerImages = new List<Image>();
    }

    // Update is called once per frame
    public virtual void Update()
    {

    }


    public void setSelected(bool value)
    {
        isSelected = value;
        if(isSelected) AudioPlayer.instance.play("bip.mp3");

        foreach (Image ai in answerImages) ai.DOColor(isSelected ? selectColor : Color.white, .3f);
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
        if (isGood || isSelected) foreach (Image ai in answerImages) ai.DOColor(isGood ? goodColor : badColor, .3f);
        if (isGood) transform.DOScale(1.2f, 1).SetEase(Ease.OutElastic);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, isGood? .1f : 0);

        canBeClicked = false;
        return isGood && isSelected;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canBeClicked) return;
        if (answerSelectedHandler != null) answerSelectedHandler(this);
    }

}
