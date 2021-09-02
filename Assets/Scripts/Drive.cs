using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour
{
    public void Add(Card card)
    {
        var localr = card.transform.localRotation;
        var locals = card.transform.localScale;
        card.transform.SetParent(transform);
        card.transform.position = transform.position;
        card.transform.localRotation = localr;
        card.transform.localScale = locals;
    }
    public Card Pull() => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>();

    public Transform GetTransform() => transform;
    public Card GetCard() => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>();
}
