using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Rearguard : MonoBehaviour, ICardCircle
{
    public int ID { get; private set; }
    public string Name { get => transform.name; }
    public bool V { get; } = false;
    public bool R { get; } = true;
    public bool Front { get; private set; } = false;
    public bool Back { get; private set; } = false;
    public int Count { get => Card == null ? 0 : 1; }
    public Card Card { get => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>(); }

    private void Start()
    {
        ID = int.Parse(transform.name.Substring(transform.name.Length - 2));
        Front = (ID - ID % 10) / 10  == 1;
        Back = !Front;
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
        card.transform.localRotation = Quaternion.Euler(0, 180, 0);
        //card.transform.localRotation = localr;
    }
    public Card Pull() => transform.FindWithChildTag(Tag.Card)?.GetComponent<Card>();

    /// <summary>
    /// リアガードサークルが同じ縦の列に存在するか調べる
    /// 相手のサークルの判定はしていない
    /// </summary>
    /// <param name="cardCircle"></param>
    /// <returns></returns>
    public bool IsSameColumn(ICardCircle cardCircle) => ID % 10 == cardCircle.ID % 10;

    public bool HasCard() => Card != null;
}