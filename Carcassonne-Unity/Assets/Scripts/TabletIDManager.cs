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
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(Input.mousePosition+"/"+Screen.width);
            if (Input.mousePosition.x > Screen.width - 100 && Input.mousePosition.y > Screen.height - 100)
            {
                downTime = Time.time;
                isDown = true;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDown = false;
        }

        if (isDown && Time.time > downTime + 3) //3 seconds to enter admin
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
