using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardZone
{
    Transform transform { get; }

    bool HasCard();
}
