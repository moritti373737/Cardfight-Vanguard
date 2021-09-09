using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISingleCardZone: ICardZone
{
    void Add(Card card);
    Card Pull();

    Card Card { get; }
}
