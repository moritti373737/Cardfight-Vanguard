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
    /// ���A�K�[�h�T�[�N���������c�̗�ɑ��݂��邩���ׂ�
    /// ���@���K�[�h�⑊��̃T�[�N���̔���͂��Ă��Ȃ�
    /// </summary>
    /// <param name="cardCircle"></param>
    /// <returns></returns>
    public bool IsSameColumn(ICardCircle cardCircle) => transform.name.Substring(transform.name.Length - 1) == cardCircle.GetTransform().name.Substring(transform.name.Length - 1);

}