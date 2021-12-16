using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Cysharp.Threading.Tasks;
using System.Linq;



public class Hand : MonoBehaviour, ICardZone, IMultiCardZone
{
    /// <summary>
    /// 手札に含まれるカード一覧
    /// 0番目がデッキ左端
    /// </summary>
    //private List<Card> cardList = new List<Card>();
    public ReactiveCollection<Card> cardList = new ReactiveCollection<Card>();

    public int Count { get => cardList.Count; }

    private void Start()
    {
        //cardList.ObserveCountChanged().Subscribe(count => SetPosition());
    }
    public void Add(Card _card)
    {

        var localr = _card.transform.localRotation;
        _card.transform.SetParent(transform);
        //_card.transform.parent = transform;
        _card.transform.position = transform.position;
        _card.transform.localRotation = localr;
        _card.transform.localPosition = Vector3.zero;
        _card.transform.localScale = new Vector3(1F, 1F, 1F);
        /*Debug.Log(transform.position);
        Debug.Log(transform.GetComponent<RectTransform>().transform.position);
        Debug.Log(transform.GetComponent<RectTransform>().sizeDelta);
        Debug.Log(_card.transform.localScale);
        Debug.Log(_card.transform.position);
        Debug.Log(_card.transform.localPosition);
        */
        cardList.Add(_card);
        SetPosition();
    }

    public Card Pull(Card card)
    {
        cardList.Remove(card);
        SetPosition();
        return card;
    }


    //public Card Pull(int _position)
    //{
    //    Card card = transform.GetChild(_position).GetComponent<Card>();
    //    cardList.Remove(card);
    //    SetPosition();
    //    return card;
    //}

    public Card GetCard(int index) => cardList[index];

    private void SetPosition()
    {
        if (cardList.Count == 0) return;
        float cardSizeX = cardList[0].transform.localScale.x;
        int CardListCount = cardList.Count;
        if (CardListCount % 2 == 0)
        {
            for (int i = 1; i <= CardListCount; i++)
            {
                Vector3 pos = cardList[i - 1].transform.localPosition;
                if (CardListCount / 2 - i >= 0)
                {
                    pos.x = -(CardListCount / 2 - i) * cardSizeX - cardSizeX / 2;
                }
                else
                {
                    pos.x = -(CardListCount / 2 - i) * cardSizeX - cardSizeX / 2;
                }
                cardList[i - 1].transform.localPosition = pos;
            }
        }
        else
        {
            for (int i = 1; i <= CardListCount; i++)
            {
                Vector3 pos = cardList[i - 1].transform.localPosition;
                if ((CardListCount + 1) / 2 - i > 0)
                {
                    pos.x = -((CardListCount + 1) / 2 - i) * cardSizeX;
                }
                else if ((CardListCount + 1) / 2 - i < 0)
                {
                    pos.x = (i - (CardListCount + 1) / 2) * cardSizeX;
                }
                else
                {
                    pos.x = 0;
                }
                cardList[i - 1].transform.localPosition = pos;
            }
        }
    }

    public Card Pull(int index)
    {
        throw new NotImplementedException();
    }
    public bool HasCard() => cardList.ToList().Any();

}

/*
 * 全体1000 1枚100
 *
 * 左端座標
 * 450
 * 400 500
 * 350 450 550
 * 300 400 500 600
 * 250 350 450 550 650
 * 200 300 400 500 600 700
 */