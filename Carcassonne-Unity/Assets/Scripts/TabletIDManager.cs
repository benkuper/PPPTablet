using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabletIDManager : MonoBehaviour {

    public int tabletID;
    public bool isAdmin;

    public static TabletIDManager instance;


    public bool isDown;
    public float downTime;

    public Canvas canvas;
    public Text idLabel;
    public Slider idSlider;

	// Use this for initialization
	void Awake () {
        instance = this;
        canvas = transform.GetComponentInChildren<Canvas>();
        canvas.gameObject.SetActive(false);

        tabletID = PlayerPrefs.GetInt("tabletID");
        idLabel.text = tabletID.ToString();
        idSlider.value = tabletID;
	}
	

    bool checkPos1(Vector2 pos)
    {
        return pos.x > Screen.width - 100 && pos.y > Screen.height - 100;
    }

    bool checkPos2(Vector2 pos)
    {
        return pos.x > Screen.width - 100 && pos.y < 100;
    }

	// Update is called once per frame
	void Update () {

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (checkPos1(Input.mousePosition))
            {
                downTime = Time.time;
                isDown = true;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDown = false;
        }
#endif
        if(Input.touchCount >= 2)
        {
            
            Vector2 t0 = Input.GetTouch(0).position;
            Vector2 t1 = Input.GetTouch(1).position;
            
            if ((checkPos1(t0) && checkPos2(t1)) || (checkPos1(t1) && checkPos2(t0)))
            {
                if(!isDown) 
                {
                    Debug.Log("DOWN DOUBLE !");
                    downTime = Time.time;
                    isDown = true;
                }
            }
            else
            {
                Debug.Log("Not valid :\n"+t0 + "\n" + t1);
                isDown = false;
            }
        }
        else
        {
            isDown = false;
        }

        if (isDown && Time.time > downTime + 2) //3 seconds to enter admin
        {
            show();
        }
	}

    public void close()
    {
        hide();
    }

    public void show()
    {

        canvas.gameObject.SetActive(true);
    }

    public void hide()
    {

        canvas.gameObject.SetActive(false);
    }

    public static int getTabletID()
    {
        if (instance == null) return -1;
        return instance.tabletID;
    }

    public static void setTabletID(int value)
    {
        instance.tabletID = value;
    }

    public void setTabletID(float value)
    {
        tabletID = (int)value;
        idLabel.text = tabletID.ToString();
        PlayerPrefs.SetInt("tabletID", tabletID);
        PlayerPrefs.Save();
    }
    
    public void setTabletAdmin(bool value)
    {
        isAdmin = value;
    }
}
