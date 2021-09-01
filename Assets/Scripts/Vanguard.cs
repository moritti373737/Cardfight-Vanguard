using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vanguard : MonoBehaviour, ICardCircle
{
    public void Add(Card card)
    {
        var localr = card.transform.localRotation;
        card.transform.SetParent(transform);
        card.transform.position = transform.position;
        card.transform.localRotation = localr;
        card.transform.localRotation = localr;
    }

    public Transform GetTransform() => transform;

    public Card GetCard() => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>();
}
