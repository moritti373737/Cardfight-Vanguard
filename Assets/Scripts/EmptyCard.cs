using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyCard : MonoBehaviour, ICardZone
{
    public Card card;

    public int ID => throw new System.NotImplementedException();

    public void Add(Card card)
    {
        throw new System.NotImplementedException();
    }

    public Card GetCard() => card;

    public Transform GetTransform()
    {
        throw new System.NotImplementedException();
    }

    public Card Pull()
    {
        throw new System.NotImplementedException();
    }

    //public new Transform transform { get => card.transform; }


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
