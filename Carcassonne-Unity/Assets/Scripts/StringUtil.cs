using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringUtil : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static string timeToCountdownString(float time)
    {
        string secondsLeft = ((int)time % 60).ToString();
        string minutesLeft = (Mathf.FloorToInt(time / 60)).ToString();

        if (secondsLeft.Length <= 1) secondsLeft = "0" + secondsLeft;
        if (minutesLeft.Length <= 1) minutesLeft = "0" + minutesLeft;
        return minutesLeft + ":" + secondsLeft;
    }
}
