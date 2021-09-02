using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardCircle
{
    void Add(Card card);
    Card Pull();

    Transform GetTransform();
    Card GetCard();
}
