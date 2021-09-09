using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardCircle : ISingleCardZone
{
    int ID { get; }
    bool V { get; }
    bool R { get; }
    bool Front { get; }
    bool Back { get; }
    bool IsSameColumn(ICardCircle cardCircle);
}
