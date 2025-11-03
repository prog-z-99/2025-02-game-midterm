using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Player : MonoBehaviour
{
    public float speed = 5;
    public float jumpPower = 7;
    public Rigidbody2D rb;
    public int MaxJump = 2;
    int jumpCount;
    public int HP = 3;
    public GameObject Hit_Prefab;
    public GameObject Health_Prefab;
    public GameObject gameOver;
    public GameObject gameClear;
    Animator anim;
    public Animator TramAnim;
    public Text HP_Text;
    bool isOver = false;
    public Slider slider;
    int MaxHP;
    bool DashRight = true;
    bool dashing = false;
    public float dashSpeed = 3;
    public float dashDuration = 0.5f;
    public float dashTimer = 0;
    public int maxDash = 1;
    int dashCount;


    // Start is called before the first frame update
    void Start()
    {
        // transform.position = new Vector3(-13, 2, 0);
        rb = GetComponent<Rigidbody2D>();

        anim = GetComponent<Animator>();
        HP_Text.text = "HP: " + HP.ToString();

        MaxHP = HP;
        slider.value = (float)HP / MaxHP;

        jumpCount = MaxJump;
        dashCount = maxDash;

    }

    // Update is called once per frame
    void Update()
    {
        slider.value = (float)HP / MaxHP;
        if (isOver)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (rb.velocity.y < -0.5)
        {
            ChangeAnim("isFall");
        }
        else if (rb.velocity.y < 0.5)
        {
            if (rb.velocity.x == 0)
            {
                ChangeAnim("isIdle");
            }
            else ChangeAnim("isRun");
        }

        if ((Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow)) && dashTimer == 0)
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                rb.velocity = new Vector2(speed, rb.velocity.y);
                GetComponent<SpriteRenderer>().flipX = false;
                DashRight = true;
            }
            else
            {
                rb.velocity = new Vector2(-speed, rb.velocity.y);
                GetComponent<SpriteRenderer>().flipX = true;
                DashRight = false;
            }
        }
        else
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (dashCount > 0)
            {
                dashing = true;
                anim.SetTrigger("dash");
                dashCount--;
            }
        }
        if (dashing)
        {
            if (dashTimer >= dashDuration)
            {
                dashTimer = 0;
                dashing = false;
                rb.velocity = new Vector2(0, 0);
            }
            else
            {
                dashTimer += Time.deltaTime;
                if (DashRight)
                {
                    rb.velocity = new Vector2(dashSpeed, 0);
                }
                else
                {
                    rb.velocity = new Vector2(-dashSpeed, 0);
                }
            }
        }



        if (Input.GetKeyDown(KeyCode.Space) && jumpCount > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            ChangeAnim("isJump");
            jumpCount--;
            if (jumpCount == 0)
            {
                anim.SetTrigger("doubleJump");
            }
        }


    }

    private void ChangeAnim(string temp)
    {
        anim.SetBool("isIdle", false);
        anim.SetBool("isRun", false);
        anim.SetBool("isJump", false);
        anim.SetBool("isFall", false);
        anim.SetBool(temp, true);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Obstacle":

                HP--;
                GameObject go = Instantiate(Hit_Prefab, transform.position, Quaternion.identity);
                Destroy(go, 1.0f);
                anim.SetTrigger("isHit");

                HP_Text.text = "HP: " + HP.ToString();

                if (HP == 0)
                {
                    goto case "InstantDeath";
                }
                break;

            case "InstantDeath":
                gameOver.SetActive(true);
                isOver = true;
                break;

            case "Environment":
                jumpCount = MaxJump;
                dashCount = maxDash;
                anim.SetBool("isFall", false);
                break;


        }

    }
    public void OnCollisionStay2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Environment":
                dashCount = maxDash;
                anim.SetBool("isFall", false);
                break;

        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {

        switch (collision.gameObject.tag)
        {
            case "Trampoline":

                TramAnim.SetTrigger("isActive");
                ChangeAnim("isJump");
                rb.velocity = new Vector2(rb.velocity.x, jumpPower * 1.5f);
                break;

            case "End":
                gameClear.SetActive(true);
                isOver = true;
                break;

            case "Health":
                Destroy(collision.gameObject);
                HP++;
                GameObject hpup = Instantiate(Health_Prefab, transform.position, Quaternion.identity);
                Destroy(hpup, 1.0f);
                HP_Text.text = "HP: " + HP.ToString();
                break;

        }



    }

}
