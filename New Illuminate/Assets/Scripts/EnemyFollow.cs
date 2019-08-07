using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public PlayerMove owo;
    private Rigidbody2D m_Rigidbody2D;
    public float speed;
    private Transform target;
    public float space;
    //public 

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(transform.position, target.position) < space)
        {
            if (owo.isFlash == true)
            {
                transform.position = Vector2.MoveTowards(transform.position, target.position, -3 * speed * Time.deltaTime);
            }
            //transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }

        



    }
}
