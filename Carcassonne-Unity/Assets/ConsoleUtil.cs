using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleUtil : MonoBehaviour {

	public static ConsoleUtil instance;
	public Text text;
	public int maxLines;

	List<string> lines;
	// Use this for initialization
	void Start () {
		text = GetComponent<Text>();
		instance = this;
		lines = new List<string>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static void log(string t)
	{
		if(instance == null || instance.text == null) return;

		instance.lines.Add(t);
		while(instance.lines.Count > instance.maxLines) instance.lines.RemoveAt(0);
		string s = "";
		foreach(string l in instance.lines) s+= l+"\n";
		instance.text.text = s;
	}
}
