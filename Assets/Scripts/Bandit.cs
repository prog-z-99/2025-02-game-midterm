using UnityEngine;
using System.Collections;

public class Bandit : MonoBehaviour
{

    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Collider2D mCollider;
    private Sensor_Bandit m_groundSensor;
    private bool m_grounded = false;
    private bool m_combatIdle = false;
    public GameObject m_attackSensorL;
    private bool m_isDead = false;
    public int HP = 3;

    int direction = 1;

    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        mCollider = GetComponent<Collider2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();
    }

    // Update is called once per frame
    void Update()
    {
        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {

            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }
        // -- Handle input and movement --
        // float inputX = Input.GetAxis("Horizontal");

        // Swap direction of sprite depending on walk direction
        if (direction != 0)
            transform.localScale = new Vector3(transform.localScale.x * direction, transform.localScale.y, transform.localScale.z);

        // Move
        // m_body2d.velocity = new Vector2(direction * m_speed, m_body2d.velocity.y);

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeed", m_body2d.velocity.y);

        // -- Handle Animations --

        //Change between idle and combat idle
        // if (Input.GetKeyDown("f"))
        //     m_combatIdle = !m_combatIdle;

        //Jump
        // else if (m_grounded)
        // {
        //     m_animator.SetTrigger("Jump");
        //     m_grounded = false;
        //     m_animator.SetBool("Grounded", m_grounded);
        //     m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
        //     m_groundSensor.Disable(0.2f);
        // }

        //Run
        // else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        //     m_animator.SetInteger("AnimState", 2);

        //Combat Idle
        if (m_combatIdle)
            m_animator.SetInteger("AnimState", 1);

        //Idle
        else
            m_animator.SetInteger("AnimState", 0);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!m_isDead)
        {


            switch (collision.gameObject.tag)
            {
                case "attack":
                    HP--;

                    m_animator.SetTrigger("Hurt");
                    if (HP == 0)
                    {
                        m_isDead = true;
                        Destroy(m_body2d);
                        Destroy(mCollider);
                        m_animator.SetTrigger("Death");

                    }
                    break;
                case "Player":
                    if (collision.gameObject.name == "PresenceSensor")
                        m_combatIdle = true;

                    else Invoke("banditAttack", 0.5f);
                    break;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Player":
                if (collision.gameObject.name == "PresenceSensor")
                    m_combatIdle = false;

                break;
        }
    }

    void banditAttack()
    {
        if (!m_isDead)
        {
            m_animator.SetTrigger("Attack");
            Invoke("attackOn", 0.4f);
        }

    }
    void attackOff()
    {
        m_attackSensorL.SetActive(false);

    }
    void attackOn()
    {
        m_attackSensorL.SetActive(true);
        Invoke("attackOff", 0.1f);

    }
}
