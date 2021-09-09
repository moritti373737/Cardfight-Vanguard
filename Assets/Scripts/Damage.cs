using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class Damage : MonoBehaviour
{
    public ReactiveCollection<Card> cardList = new ReactiveCollection<Card>();
    void Start()
    {
        cardList.ObserveCountChanged().Subscribe(_ => ChangeCount());
    }

    private void ChangeCount()
    {
        for (int i = 0; i < cardList.Count; i++)
        {
            cardList[i].transform.localPosition = new Vector3(i * 0.27F, 0, (i + 1) * -0.9F);
        }
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
