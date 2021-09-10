using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Deck : MonoBehaviour, IMultiCardZone
{
    /// <summary>
    /// 山札に含まれるカード一覧
    /// がデッキトップ
    /// </summary>
    public List<Card> cardList = new List<Card>();

    private void Start()
    {
        //この時点ではcardListは空
        //cardList.Reverse();
        //cardList = cardList.OrderBy(a => Guid.NewGuid()).ToList();
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

    public void Shuffle()
    {

        cardList = cardList.OrderBy(a => Guid.NewGuid()).ToList();
        for (int i = 1; i <= cardList.Count; i++)
        {
            cardList[cardList.Count - i].transform.localPosition = new Vector3(0, 0, (float)(i * -0.9));
            cardList[i - 1].transform.SetAsLastSibling();
        }
    }

    public bool HasCard() => cardList.Any();
    public Card GetCard(int index) => cardList[index];
}