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

        //Vector3 posInParent = transform.parent.InverseTransformPoint(transform.position);

        //Vector3 colPosInParent = transform.parent.InverseTransformPoint(c.transform.position);
        //Vector3 tPos = transform.InverseTransformPoint(transform.parent.TransformPoint(posInParent.x, posInParent.y, colPosInParent.z));
        //transform.localPosition = posInParent;
        trigger();
    }

    public void trigger()
    {
        GetComponent<ParticleSystem>().Play();
    }
}
