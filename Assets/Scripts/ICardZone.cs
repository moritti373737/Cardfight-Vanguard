using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardZone
{
    void Add(Card card);
    Card Pull();

    Transform GetTransform();
    Card Card { get; }
}
