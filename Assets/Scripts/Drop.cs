using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class Drop : MonoBehaviour
{
    public ReactiveCollection<Card> cardList = new ReactiveCollection<Card>();

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
