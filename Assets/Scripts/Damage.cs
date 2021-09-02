using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class Damage : MonoBehaviour
{
    public ReactiveCollection<Card> cardList = new ReactiveCollection<Card>();
    void Start()
    {
        //cardList.ObserveCountChanged().Subscribe(_ => ChangeCount());
    }

    private void ChangeCount()
    {
        for (int i = 0; i < cardList.Count; i++)
        {
            cardList[i].transform.localPosition = new Vector3(0, 0, (float)(i * -0.9));
        }
    }

    public void Add(Card _card)
    {
        _card.transform.SetParent(transform, false);
        _card.transform.localPosition = new Vector3(cardList.Count * 0.27F, 0, (cardList.Count + 1) * -0.9F);
        cardList.Add(_card);

    }

    public Card Pull(int _position)
    {
        Card card = cardList[_position];
        cardList.Remove(card);
        return card;
    }
}
