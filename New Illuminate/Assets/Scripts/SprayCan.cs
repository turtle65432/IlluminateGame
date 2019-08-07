using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayCan : MonoBehaviour
{
    public GameObject effect;
    public GameObject hitBox;
    public float emmissionTime;
    public float timeBetweenEmmission;


    void Spray()
    {
        effect.active = true;
        Invoke("ChangeHitBox", 1f);
        Invoke("ChangeSpray", emmissionTime);
        Invoke("ChangeHitBox", emmissionTime - 1.5f);

        Invoke("Spray", timeBetweenEmmission);
    }
    void ChangeHitBox()
    {
        if(hitBox.active == true)
        {
            hitBox.active = false;
        }
        else if(hitBox.active == false)
        {
            hitBox.active = true;
        }
    }
    void ChangeSpray()
    {
        if (effect.active == true)
        {
            effect.active = false;
        }
        else if (effect.active == false)
        {
            effect.active = true;
        }
    }

    void Start()
    {
        Spray();
    }


}
