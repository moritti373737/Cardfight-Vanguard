using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;



public class Drop : MonoBehaviour, IMultiCardZone
{
    public ReactiveCollection<Card> cardList = new ReactiveCollection<Card>();
    public int Count { get => cardList.Count; }

    // Start is called before the first frame update
    void Start()
    {
        cardList.ObserveCountChanged().Subscribe(_ => ChangeCount());
    }

    private void ChangeCount()
    {
        for (int i = 0; i < cardList.Count; i++)
        {
            cardList[i].transform.localPosition = new Vector3(0, 0, (float)(i * -0.9));
        }
    }

    public void Add(Card card)
    {
        card.transform.SetParent(transform, false);
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale = Vector3.one;
        card.transform.localRotation = Quaternion.Euler(0, 180, 0);

        cardList.Add(card);

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
