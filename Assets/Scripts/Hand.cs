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

    private List<GameObject> EmptyCardList = new List<GameObject>();
    private List<GameObject> EmptyCardRemovedList = new List<GameObject>();
    public GameObject EmptyCardPrefab;

    public int Count { get => cardList.Count; }

    private void Start()
    {
        //cardList.ObserveCountChanged().Subscribe(count => SetPosition());
    }
    public void Add(Card _card)
    {
        var emptyCard = Instantiate(EmptyCardPrefab).FixName();
        //emptyCard.GetComponent<EmptyCard>().card = _card;
        emptyCard.transform.SetParent(transform);
        emptyCard.transform.position = transform.position;
        emptyCard.transform.localPosition = Vector3.zero;
        emptyCard.transform.localScale = new Vector3(1F, 1F, 1F);
        emptyCard.transform.localRotation = Quaternion.identity;

        var localr = _card.transform.localRotation;
        _card.transform.SetParent(emptyCard.transform);
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
        EmptyCardList.Add(emptyCard);
        SetPosition();
    }

    public Card Pull(Card card)
    {
        GameObject empthObject = card.transform.parent.gameObject;
        cardList.Remove(card);
        EmptyCardList.Remove(empthObject);
        EmptyCardRemovedList.Add(empthObject);
        SetPosition();
        DestroyEmpty(card);
        return card;
    }

    private void DestroyEmpty(Card card)
    {
        //foreach (var emptyCard in EmptyCardRemovedList)
        //{
        //    print(emptyCard.transform.childCount);
        //    print(emptyCard.transform.Find("SelectBox"));
        //        //yield return false;
        //    Destroy(emptyCard);
        //}
        EmptyCardRemovedList.ForEach(emptyCard => Destroy(emptyCard));
        EmptyCardRemovedList.Clear();
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
        if (EmptyCardList.Count == 0) return;
        float cardSizeX = EmptyCardList[0].transform.localScale.x;
        int EmptyCardListCount = EmptyCardList.Count;
        if (EmptyCardListCount % 2 == 0)
        {
            for (int i = 1; i <= EmptyCardListCount; i++)
            {
                Vector3 pos = EmptyCardList[i - 1].transform.localPosition;
                if (EmptyCardListCount / 2 - i >= 0)
                {
                    pos.x = -(EmptyCardListCount / 2 - i) * cardSizeX - cardSizeX / 2;
                }
                else
                {
                    pos.x = -(EmptyCardListCount / 2 - i) * cardSizeX - cardSizeX / 2;
                }
                EmptyCardList[i - 1].transform.localPosition = pos;
            }
        }
        else
        {
            for (int i = 1; i <= EmptyCardListCount; i++)
            {
                Vector3 pos = EmptyCardList[i - 1].transform.localPosition;
                if ((EmptyCardListCount + 1) / 2 - i > 0)
                {
                    pos.x = -((EmptyCardListCount + 1) / 2 - i) * cardSizeX;
                }
                else if ((EmptyCardListCount + 1) / 2 - i < 0)
                {
                    pos.x = (i - (EmptyCardListCount + 1) / 2) * cardSizeX;
                }
                else
                {
                    pos.x = 0;
                }
                EmptyCardList[i - 1].transform.localPosition = pos;
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