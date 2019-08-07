using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // Start is called before the first frame update
    public int life;
    public bool canGetHit;

    void Start()
    {
        canGetHit = true;
        life = 3;
    }

    void SetHitBool()
    {
        canGetHit = true;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy" && canGetHit)
        {
            canGetHit = false;
            life--;
            if(life <= 0)
            {
                Destroy(gameObject);
            }
            Invoke("SetHitBool", 1f);
        }
    }


}