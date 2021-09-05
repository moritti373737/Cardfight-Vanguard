using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rearguard : MonoBehaviour, ICardCircle
{
    public int ID { get; private set; }
    public bool V { get; } = false;
    public bool R { get; } = true;
    public bool Front { get; private set; } = false;
    public bool Back { get; private set; } = false;

    private void Start()
    {
        ID = int.Parse(transform.name.Substring(transform.name.Length - 2));
        Front = (ID - ID % 10) / 10  == 1;
        Back = !Front;
    }

    public void Add(Card card)
    {
        var localr = card.transform.localRotation;
        card.transform.SetParent(transform);
        card.transform.position = transform.position;
        card.transform.localRotation = localr;
    }
    public Card Pull() => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>();

    public Transform GetTransform() => transform;
    public Card GetCard() => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>();

    /// <summary>
    /// リアガードサークルが同じ縦の列に存在するか調べる
    /// 相手のサークルの判定はしていない
    /// </summary>
    /// <param name="cardCircle"></param>
    /// <returns></returns>
    public bool IsSameColumn(ICardCircle cardCircle) => ID % 10 == cardCircle.ID % 10;

}