using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyCard : MonoBehaviour, ICardZone
{
    public Card Card { get => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>(); }

    public void Add(Card card)
    {
        throw new System.NotImplementedException();
    }

    public Transform GetTransform()
    {
        throw new System.NotImplementedException();
    }

    public Card Pull()
    {
        throw new System.NotImplementedException();
    }
}
