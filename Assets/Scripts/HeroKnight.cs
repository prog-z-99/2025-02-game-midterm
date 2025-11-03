using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class HeroKnight : MonoBehaviour
{

    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    // [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;
    private Sensor_HeroKnight m_wallSensorR1;
    private Sensor_HeroKnight m_wallSensorR2;
    private Sensor_HeroKnight m_wallSensorL1;
    private Sensor_HeroKnight m_wallSensorL2;
    public GameObject m_attackSensorR;
    public GameObject m_attackSensorL;
    public GameObject gameOver;
    public GameObject gameClear;
    private bool m_isWallSliding = false;
    private bool m_grounded = false;
    private bool m_rolling = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    public float m_rollDuration = 0.2f;
    int MaxHP;
    public int HP = 3;
    public Slider slider;
    public Text HP_Text;
    int jumpCount;
    public int maxJump = 2;
    int dashCount;
    public int maxDash = 1;
    bool isDead = false;
    public Text dialogueText;
    public GameObject dialogue;
    [TextArea]
    public string dialogueString;
    public AudioSource audioSource;

    public AudioClip[] sfx;
    bool isOver = false;

    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
        HP_Text.text = "HP: " + HP.ToString();

        MaxHP = HP;
        slider.value = (float)HP / MaxHP;

        jumpCount = maxJump;
        dashCount = maxDash;
        audioSource.Play();

    }


    void stopRoll()
    {
        m_rolling = false;
        m_body2d.velocity = new Vector2(0, 0);
    }
    // Update is called once per frame
    void Update()
    {
        //Set HP slider value
        slider.value = (float)HP / MaxHP;
        while (isOver)
        { return; }
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Horizontal");

        // Increase timer that checks roll duration
        if (m_rolling)
        {
            m_body2d.velocity = new Vector2(m_rollForce * m_facingDirection, 0);
        }


        // Disable rolling if timer extends duration
        // if (m_rollCurrentTime > m_rollDuration)
        // {
        //     m_rolling = false;
        //     m_rollCurrentTime = 0;
        //     m_body2d.velocity = new Vector2(0, 0);
        // }


        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            jumpCount = maxJump;
            dashCount = maxDash;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }



        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }

        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // Move
        if (!m_rolling)
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);


        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        // -- Handle Animations --
        //Wall Slide
        if ((m_wallSensorR1.State() && m_wallSensorR2.State()) || (m_wallSensorL1.State() && m_wallSensorL2.State()))
        {
            m_isWallSliding = true;
            // AE_SlideDust();
        }
        else
        {
            m_isWallSliding = false;
        }

        m_animator.SetBool("WallSlide", m_isWallSliding);



        //Attack
        if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);

            // Reset timer
            m_timeSinceAttack = 0.0f;

            //Attack hitbox
            if (m_facingDirection > 0)
            {
                m_attackSensorR.SetActive(true);
            }
            else
                m_attackSensorL.SetActive(true);

            Invoke("attackOff", 0.1f);
            //Play sound
            audioSource.PlayOneShot(sfx[1]);
        }

        // Block
        else if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }

        else if (Input.GetMouseButtonUp(1))
            m_animator.SetBool("IdleBlock", false);

        // Roll
        else if (Input.GetKeyDown("left shift") && !m_rolling && !m_isWallSliding && dashCount > 0)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, 0);
            Invoke("stopRoll", m_rollDuration);
            if (!m_grounded)
            {
                dashCount--;
            }
        }


        //Jump
        else if (Input.GetKeyDown("space") && !m_rolling && jumpCount > 0)
        {
            if (m_grounded)
            {
                m_grounded = false;
                m_animator.SetBool("Grounded", m_grounded);
                m_groundSensor.Disable(0.2f);

            }
            m_animator.SetTrigger("Jump");
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            jumpCount--;
        }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }
    }

    // Animation Events
    // Called in slide animation.
    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "dialogueTrigger":
                dialogue.SetActive(true);
                switch (collision.gameObject.name)
                {
                    case "dt1":
                        dialogueString = "I have to deal with the bandits in the nearby forest. I better go check them out. \n Press A or D to move";
                        break;
                    case "dt2":
                        dialogueString = "Hmm, the road to the forest seems to have been eroded away... \n Press Space to jump";
                        break;
                    case "dt3":
                        dialogueString = "That's quite the climb... Well, nothing my boots of agility can't deal with! \n Press space mid-air to double jump";
                        break;
                    case "dt4":
                        dialogueString = "Was there another cliff like this? This seems dug up... too deep and too wide for mere bandits... \n Press shift to dash";
                        break;
                    case "dt5":
                        dialogueString = "That's the food from the village! I wonder what else they've stolen from them... \n Press Mouse 1 to attack";
                        break;
                    case "dt6":
                        dialogueString = "Bandits! Taste my blade! \n Press Mouse 1 to attack";
                        break;
                }
                dialogueText.text = dialogueString;

                break;
            case "health":
                Destroy(collision.gameObject);
                HP++;
                HP_Text.text = "HP: " + HP.ToString();
                break;
            case "damage":
                m_animator.SetTrigger("Hurt");
                HP--;

                HP_Text.text = "HP: " + HP.ToString();

                if (HP <= 0)
                {
                    goto case "death";
                }

                break;
            case "death":
                m_animator.SetTrigger("Death");
                gameOver.SetActive(true);
                isOver = true;

                break;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "dialogueTrigger":
                dialogue.SetActive(false);

                break;
        }
    }

    void attackOff()
    {
        m_attackSensorR.SetActive(false);
        m_attackSensorL.SetActive(false);

    }

}
