using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMultiCardZone: ICardZone
{
    int Count { get; }
    void Add(Card card);
    Card Pull(int index);
    public Card GetCard(int index);
}
