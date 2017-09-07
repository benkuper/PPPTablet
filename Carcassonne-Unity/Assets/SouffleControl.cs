using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SouffleControl : MonoBehaviour {

    public List<Collider> colliders;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnTriggerEnter(Collider c)
    {
        if (colliders.IndexOf(c) == -1) return;
        Debug.Log("trigger");
        transform.position = new Vector3(transform.position.x, transform.position.y, c.transform.position.z);
        trigger();
    }

    public void trigger()
    {
        GetComponent<ParticleSystem>().Play();
    }
}
