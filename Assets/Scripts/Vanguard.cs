using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vanguard : MonoBehaviour, ICardCircle
{
    public int ID { get; } = 12;
    public bool Front { get; } = true;
    public bool Back { get; } = false;
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
    public bool IsSameColumn(ICardCircle cardCircle) => ID % 10 == cardCircle.ID % 10;
}
