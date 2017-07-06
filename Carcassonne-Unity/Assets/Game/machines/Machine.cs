using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machine : MonoBehaviour
{
    public Transform[] outsideObjects;
    public Transform[] dechets;


    public float rotationX;
    public float rotationY;
    public float positionX;
    public float positionY;
    public float positionZ;
    public float scale;

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void loadMachineSettings()
    {
        transform.Find("transformer").localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        transform.Find("transformer").localPosition = new Vector3(positionX, positionY, positionZ);
        transform.Find("transformer").localScale = Vector3.one * scale;
    }

    public void setDechets(bool value)
    {
        foreach(Transform d in dechets)
        {
            d.gameObject.SetActive(value);
        }
    }

    public void setOutside(bool value)
    {
        foreach(Transform d in outsideObjects)
        {
            d.gameObject.SetActive(value);
        }
    }
}
