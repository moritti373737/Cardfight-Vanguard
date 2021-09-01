using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardCircle
{
    void Add(Card card);

    Transform GetTransform();
    Card GetCard();
}
