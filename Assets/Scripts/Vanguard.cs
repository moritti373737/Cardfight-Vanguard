using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Vanguard : MonoBehaviour, ICardCircle
{
    public int ID { get; } = 12;
    public bool V { get; } = true;
    public bool R { get; } = false;
    public bool Front { get; } = true;
    public bool Back { get; } = false;
    public Card Card { get => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>(); }

    private void Start()
    {
        transform.ObserveEveryValueChanged(x => x.childCount)
                 .Skip(1)
                 .Where(_ => Card == null)
                 .Subscribe(_ => TextManager.Instance.DestroyStatusText(this));
    }

    public void Add(Card card)
    {
        //var localr = card.transform.localRotation;
        card.transform.SetParent(transform);
        card.transform.localPosition = new Vector3(0, 0, -1);
        //card.transform.localRotation = localr;

    }
    public Card Pull() => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>();

    public bool IsSameColumn(ICardCircle cardCircle) => ID % 10 == cardCircle.ID % 10;
    public bool HasCard() => Card != null;
}
