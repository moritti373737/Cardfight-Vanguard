using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardCircle : ICardZone
{
    int ID { get; }
    bool V { get; }
    bool R { get; }
    bool Front { get; }
    bool Back { get; }
    bool IsSameColumn(ICardCircle cardCircle);
}
