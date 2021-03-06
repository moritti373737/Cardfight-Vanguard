using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;



public class Guardian : MonoBehaviour, IMultiCardZone
{
    /// <summary>
    /// 手札に含まれるカード一覧
    /// </summary>
    //private List<Card> cardList = new List<Card>();
    public List<Card> cardList = new List<Card>();

    private readonly List<GameObject> EmptyCardList = new List<GameObject>();
    public GameObject EmptyCardPrefab;
    public int Count { get => cardList.Count; }

    public int Shield { get => cardList.Sum(card => card.Shield); }

    public void Add(Card card)
    {
        var emptyCard = Instantiate(EmptyCardPrefab).FixName();
        //emptyCard.GetComponent<EmptyCard>().Card = card;
        emptyCard.transform.SetParent(transform);
        emptyCard.transform.position = transform.position;
        emptyCard.transform.localPosition = Vector3.zero;
        emptyCard.transform.localScale = new Vector3(1F, 1F, 1F);
        emptyCard.transform.localRotation = Quaternion.identity;
        Vector3 defaultScale = emptyCard.transform.lossyScale;
        Vector3 lossyScale, localScale;
        emptyCard.transform.localRotation = Quaternion.Euler(0, 0, 0);
        lossyScale = emptyCard.transform.lossyScale;
        localScale = emptyCard.transform.localScale;
        emptyCard.transform.localScale = new Vector3(
            localScale.x / lossyScale.x * defaultScale.x,
            localScale.y / lossyScale.y * defaultScale.y,
            localScale.z / lossyScale.z * defaultScale.z
        );

        card.transform.SetParent(emptyCard.transform);
        //_card.transform.parent = transform;
        card.transform.position = transform.position;
        card.transform.localScale = new Vector3(1F, 1F, 1F);

        cardList.Add(card);
        EmptyCardList.Add(emptyCard);
        SetPosition();
    }
    public Card Pull(Card card)
    {
        GameObject empthObject = card.transform.parent.gameObject;
        card.transform.parent = null;
        cardList.Remove(card);
        EmptyCardList.Remove(empthObject);
        SetPosition();
        Destroy(empthObject);
        return card;
    }

    public List<Card> Clear()
    {
        var retList = new List<Card>(cardList);
        cardList.ForEach(card => card.transform.parent = null);
        cardList.Clear();
        EmptyCardList.ForEach(emptyCard => Destroy(emptyCard));
        EmptyCardList.Clear();
        return retList;
    }

    //private void DestroyEmpty(GameObject empthObject)
    //{
    //    EmptyCardRemovedList.ForEach(emptyCard => Destroy(emptyCard));
    //    EmptyCardRemovedList.Clear();
    //}


    public Transform GetTransform() => transform;

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
                    pos.y = -(EmptyCardListCount / 2 - i) * cardSizeX - cardSizeX / 2;
                }
                else
                {
                    pos.y = -(EmptyCardListCount / 2 - i) * cardSizeX - cardSizeX / 2;
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
                    pos.y = -((EmptyCardListCount + 1) / 2 - i) * cardSizeX;
                }
                else if ((EmptyCardListCount + 1) / 2 - i < 0)
                {
                    pos.y = (i - (EmptyCardListCount + 1) / 2) * cardSizeX;
                }
                else
                {
                    pos.y = 0;
                }
                EmptyCardList[i - 1].transform.localPosition = pos;
            }
        }
    }

    public Card Pull(int index)
    {
        throw new System.NotImplementedException();
    }
    public bool HasCard() => cardList.Any();
    public Card GetCard(int index) => cardList[index];
}
