using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class CandyDechet : MonoBehaviour
//IPointerClickHandler
{


    //public delegate void CandyTouchDownEvent(CandyDechet c, Vector2 pos);
    //public CandyTouchDownEvent candyTouchDown;


    public string id;
    Color color;

    RawImage image;

    public int x;
    public int y;

    RectTransform r;
    RectTransform parentRect;
    float tw;
    float th;
    float gap;

	// Use this for initialization
	void Awake () {
        image = GetComponentInChildren<RawImage>();
        r = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
       
    }

    /*    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (candyTouchDown != null) candyTouchDown(this,eventData.pressPosition);
    }
    */

    public void setData(DechetData d, Texture t)
    {
        id = d.id;
        ColorUtility.TryParseHtmlString(d.couleur,out color);
        //image.color = color;
        setTexture(t);
        color = GetComponent<Image>().color;
    }

    public void setTexture(Texture t)
    {
        image.texture = t;
    }

    public void setBlock(int _x, int _y, float tailleGrille, float _gap)
    {
        x = _x;
        y = _y;
        //GetComponentInChildren<Text>().text = x + "," + y;
        gap = _gap;

        parentRect = transform.parent.GetComponentInParent<RectTransform>();
        tw = (parentRect.rect.width - gap * tailleGrille) / tailleGrille;
        th = (parentRect.rect.height - gap * tailleGrille) / tailleGrille;

        r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tw);
        r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,th);

        Vector3 ts = transform.localScale;
        transform.localScale = Vector3.zero;
        transform.DOScale(ts, .3f);

        Vector3 pos = new Vector3(x * (tw + gap) - parentRect.rect.width / 2 + r.rect.width/2, parentRect.rect.height/2 - (y+1) * (th + gap) + r.rect.height/2);
        transform.localPosition = pos;
    }    

    public void moveToBlock(int tx, int ty, float time)
    {
        x = tx;
        y = ty;
        //GetComponentInChildren<Text>().text = x + "," + y;
        Vector3 pos = new Vector3(x * (tw + gap) - parentRect.rect.width / 2 + r.rect.width / 2, parentRect.rect.height / 2 - (y + 1) * (th + gap) + r.rect.height/2);
        transform.DOLocalMove(pos,time);
    }

    public void setSelected(bool value)
    {
       GetComponent<Image>().color = value ? Color.yellow : color;
    }


    public void remove()
    {
        transform.DOScale(Vector3.zero, .3f).OnComplete(removeComplete);
    }

    public void removeComplete()
    {
        Destroy(gameObject);
    }
    
}
