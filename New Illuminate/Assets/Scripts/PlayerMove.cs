using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public CharacterMovement2D controller;
    public Animator animator;

    public float runSpeed = 40f;

    float horizontalMove = 0f;


    public bool jump;

    private float vertical;

    Vector3 player;

    public float timeLeft = 5f;

    public float newRunSpeed = 80f;

    public bool startTimer;

    public bool isFlash = false;


    public LayerMask groundLayer;

    private Rigidbody2D m_Rigidbody2D;
    [SerializeField] private float m_JumpForce = 400f;
    public ParticleSystem flashParticles;



    private void Start()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        player = gameObject.transform.position;
        Debug.Log(timeLeft -= Time.deltaTime);

    }

    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed* Time.deltaTime;

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        //if (Input.GetButtonDown("Jump"))
        //{
        //    //jump = true;
        //}
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z), Vector2.down, Color.red, 1f);

        vertical = Input.GetAxis("Vertical") * (runSpeed / 4);
        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));


        if (Input.GetButtonDown("Jump") && IsGrounded())
        {

            jump = true;
            Debug.Log(jump = true);
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            animator.SetBool("isJumping", true);
        }
      
        if (startTimer)
        {
            timeLeft -= Time.deltaTime;

            if (timeLeft <= 0)
            {
                isFlash = false;
                timeLeft = 5;
                startTimer = false;
            }
        }
        }

  private  void FixedUpdate()
    {
        controller.Move(horizontalMove, false, jump);
        jump = false;

        if (Input.GetButtonDown("Fire1"))
        {
            isFlash = true;
            startTimer = true;
            flashParticles.Play();

        }
    }

        bool IsGrounded()
        {
            if (Physics2D.Raycast(new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z), Vector2.down, 1f, groundLayer.value))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }

