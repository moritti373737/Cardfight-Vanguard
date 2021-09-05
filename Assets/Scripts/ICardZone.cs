using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardZone
{
    int ID { get; }
    void Add(Card card);
    Card Pull();

    Transform GetTransform();
    Card GetCard();
}
