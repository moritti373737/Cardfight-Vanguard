using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MouseController : MonoBehaviour
{
    public int ClickedObject = 0;

    public enum ObjectName
    {
        NONE,
        DECK,
    };

    //ObjectName objectName;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DeckClicked()
    {
        //objectName = ObjectName.DECK;
    }
}
