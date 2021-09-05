using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardCircle : ICardZone
{
    bool V { get; }
    bool R { get; }
    bool Front { get; }
    bool Back { get; }
    public bool IsSameColumn(ICardCircle cardCircle);
}
