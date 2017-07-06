using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Code2DConsommation : Code2DGame {

    public float mauvaiseReponseIDStart;

   public override bool checkIdIsGood(int id)
    {
        return id < mauvaiseReponseIDStart;
    }
}
