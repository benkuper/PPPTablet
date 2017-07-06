using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machine : MonoBehaviour
{
    public Transform[] dechets;
    
    Transform transformer;

    private void Awake()
    {
        setDechets(false);

    }


    public void loadMachineSettings(MachineData data)
    {

        transformer = transform.Find("transformer");
        Debug.Log("Transformer " + (transformer != null) + "/" + (data != null));
        Debug.Log(" > " +data.nom);
        transformer.localRotation = Quaternion.Euler(data.rotationX, data.rotationY, 0);
        transformer.localPosition = new Vector3(data.positionX, data.positionY, data.positionZ);
        transformer.localScale = Vector3.one * data.scale;
    }

    public void setDechets(bool value)
    {
        foreach(Transform d in dechets)
        {
            d.gameObject.SetActive(value);
        }
    }
   
}
