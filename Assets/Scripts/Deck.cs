using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;



public class Deck : MonoBehaviour, IMultiCardZone
{
    /// <summary>
    /// 山札に含まれるカード一覧
    /// がデッキトップ
    /// </summary>
    public List<Card> cardList = new List<Card>();
    public int Count { get => cardList.Count; }

    private void Start()
    {
        //この時点ではcardListは空
        //cardList.Reverse();
        //cardList = cardList.OrderBy(a => Guid.NewGuid()).ToList();
    }

    public void Add(Card card)
    {
        card.transform.SetParent(transform, false);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity;
        cardList.Add(card);

        for (int i = 1; i <= cardList.Count; i++)
        {
            cardList[cardList.Count - i].transform.localPosition = new Vector3(0, 0, (float)(i * -0.9));
            cardList[i - 1].transform.SetAsLastSibling();
        }
    }

    public Card Pull(int _position)
    {
        Card card = cardList[_position];
        cardList.Remove(card);
        return card;
    }

    public Card Pull(Card card)
    {
        cardList.Remove(card);
        return card;
    }

    public async UniTask Shuffle()
    {
        float offset = 0.9F;
        //cardList = cardList.OrderBy(a => Guid.NewGuid()).ToList();
        //System.Random rnd = new System.Random(DateTime.Now.Millisecond + cardList[0].ID);
        System.Random rnd = new System.Random(cardList[0].ID);
        cardList = cardList.OrderBy(item => rnd.Next()).ToList();

        for (int i = 1; i <= cardList.Count; i++)
        {
            cardList[cardList.Count - i].transform.localPosition = new Vector3(0, 0, -i * offset);
            cardList[i - 1].transform.SetAsLastSibling();
        }

        await AnimationManager.Instance.DeckShuffle(cardList, offset);
    }

    public bool HasCard() => cardList.Any();
    public Card GetCard(int index) => cardList[index];

}