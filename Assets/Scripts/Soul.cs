using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Soul : MonoBehaviour, IMultiCardZone
{
    public List<Card> cardList = new List<Card>();
    public int Count { get => cardList.Count; }


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
