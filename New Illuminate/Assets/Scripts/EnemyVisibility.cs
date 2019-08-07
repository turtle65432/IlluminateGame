using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVisibility : MonoBehaviour
{
    public EnemyFollow follower;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject);
        if(collision.gameObject.tag == "Player")
        {
            follower.enabled = true;
        }
        else
        {
            follower.enabled = false;
        }

    }
}
