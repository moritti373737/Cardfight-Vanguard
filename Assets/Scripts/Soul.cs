using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Soul : MonoBehaviour, IMultiCardZone
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
    public bool HasCard() => cardList.ToList().Any();
    public Card GetCard(int index) => cardList[index];
}
