using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rearguard : MonoBehaviour, ICardCircle
{
    // Start is called before the first frame update
    public void Add(Card card)
    {
        var localr = card.transform.localRotation;
        card.transform.SetParent(transform);
        card.transform.position = transform.position;
        card.transform.localRotation = localr;
    }
    public Card Pull() => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>();

    public Transform GetTransform() => transform;
    public Card GetCard() => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>();

    /// <summary>
    /// リアガードサークルが同じ縦の列に存在するか調べる
    /// ヴァンガードや相手のサークルの判定はしていない
    /// </summary>
    /// <param name="cardCircle"></param>
    /// <returns></returns>
    public bool IsSameColumn(ICardCircle cardCircle) => transform.name.Substring(transform.name.Length - 1) == cardCircle.GetTransform().name.Substring(transform.name.Length - 1);

}