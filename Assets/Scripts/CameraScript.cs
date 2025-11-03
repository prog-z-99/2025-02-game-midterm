using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScrolling : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject parallaxBG1;
    public GameObject parallaxBG2;
    public GameObject parallaxBG3;
    public GameObject parallaxBG4;
    public GameObject parallaxBG5;
    public float BGSpriteSize = 12f;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ParallaxList();

    }

    void ParallaxList()
    {
        Parallax(parallaxBG1);
        Parallax(parallaxBG2);
        Parallax(parallaxBG3);
        Parallax(parallaxBG4);
        Parallax(parallaxBG5);
    }

    void Parallax(GameObject background)
    {
        background.transform.position = new Vector3(transform.position.x + ((transform.position.x / (background.transform.position.z + 4)) % BGSpriteSize), background.transform.position.y, background.transform.position.z);
    }
}
