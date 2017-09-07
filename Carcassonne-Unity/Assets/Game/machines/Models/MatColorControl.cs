using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[ExecuteInEditMode]
public class MatColorControl : MonoBehaviour {

    public string property;

    public Color start;
    public Color end;

    [Range(0, 1)]
    public float weight;
    float _lastWeight;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(weight != _lastWeight)
        {
            _lastWeight = weight;
            GetComponent<Renderer>().sharedMaterial.SetColor(property, Color.Lerp(start, end, weight));
        }
	}

    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision");
        trigger();
    }

    public void OnTriggerEnter(Collider c)
    {
        Debug.Log("trigger");
        trigger();
    }

    public void trigger()
    {
        DOTween.Kill("flash");
        weight = 1;
        DOTween.To(() => weight, x => weight = x, 0, .3f).SetId("flash");
    }
}
