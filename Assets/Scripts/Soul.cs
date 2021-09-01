using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soul : MonoBehaviour
{
    public List<Card> cardList = new List<Card>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Add(Card _card)
    {
        _card.transform.SetParent(transform, false);
        cardList.Add(_card);
    }

    public Card Pull(int _position)
    {
        Card card = cardList[_position];
        cardList.Remove(card);
        return card;
    }
}
