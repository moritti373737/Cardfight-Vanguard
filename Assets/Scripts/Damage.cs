using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class Damage : MonoBehaviour, ICardZone
{
    public ReactiveCollection<Card> cardList = new ReactiveCollection<Card>();

    public Card Card => throw new System.NotImplementedException();

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
        _card.transform.localRotation = Quaternion.Euler(0, 180, 0);
        cardList.Add(_card);

    }

    public Card Pull(Card card)
    {
        cardList.Remove(card);
        return card;
    }
    public int Count() => cardList.Count;

    public Card GetCard(int index) => cardList[index];

    public Card Pull()
    {
        throw new System.NotImplementedException();
    }

    public Transform GetTransform() => transform;

}
