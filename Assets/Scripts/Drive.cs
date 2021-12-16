using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Drive : MonoBehaviour, ISingleCardZone
{
    public Card Card { get => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>(); }

    public void Add(Card card)
    {
        card.transform.SetParent(transform);
        card.transform.position = transform.position;
        card.transform.localPosition = new Vector3(0, 0, -1);
        card.transform.localRotation = Quaternion.identity;
        card.transform.localScale = Vector3.one;
    }
    public Card Pull() => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>();

    public Transform GetTransform() => transform;
    public Card GetCard() => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>();

    public bool HasCard() => Card != null;
}
