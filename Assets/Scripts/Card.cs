using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    //public int id = -1;

    // Start is called before the first frame update
    void Start()
    {
        //id = int.Parse(transform.name.Substring(4));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TurnOver() => transform.Rotate(0, 180, 0);

    public Texture GetTexture() => transform.Find("Face").GetComponent<Renderer>().material.mainTexture;
}
