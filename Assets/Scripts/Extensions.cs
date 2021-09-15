using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// �v���W�F�N�g�Ŏg�p����g�����\�b�h
/// </summary>
public static class Extensions
{
    // ����X�ʒu���������X�������ǉ��ňړ�
    //public static void AddPosX(this Transform transform, float x)
    //{
    //    var pos = transform.position;
    //    pos.x += x;
    //    transform.position = pos;
    //}

    /// <summary>
    /// �w�肵���^�O�����݂��邩���ׂ�i������v�j
    /// </summary>
    /// <param name="transform">�����Ώ�</param>
    /// <param name="tag">�����������^�O</param>
    /// <returns>���݂��邩</returns>
    public static bool ExistTag(this Transform transform, Tag tag) => transform.tag.Contains(tag.ToString());

    //public static Transform FindWithChildTag(this Transform transform, string tag)
    //{
    //    foreach (Transform childTransform in transform)
    //    {
    //        if (childTransform.tag.Contains(tag))
    //            return childTransform;
    //    }
    //    return null;
    //}

    /// <summary>
    /// �w�肵���^�O�����܂ގq��transform��T���i������v�j
    /// </summary>
    /// <param name="parentTransform">�e��transform</param>
    /// <param name="tag">�����������^�O��</param>
    /// <returns>�q��transform</returns>
    public static Transform FindWithChildTag(this Transform parentTransform, Tag tag)
    {
        return parentTransform.Cast<Transform>()
                              .ToList()
                              .FirstOrDefault(t => t.tag.Contains(tag.ToString()));
        //foreach (Transform childTransform in parentTransform)
        //{
        //    if (childTransform.tag.Contains(tag.ToString()))
        //        return childTransform;
        //}
        //return null;
    }

    /// <summary>
    /// �w�肵���^�O�����܂ގq��GameObject��T���i������v�j
    /// </summary>
    /// <param name="parentGameObject">�e��GameObject</param>
    /// <param name="tag">�����������^�O��</param>
    /// <returns>�q��GameObject</returns>
    public static GameObject FindWithChildTag(this GameObject parentGameObject, Tag tag) => FindWithChildTag(parentGameObject.transform, tag)?.gameObject;

    /// <summary>
    /// �w�肵���^�O�����܂ޑS�Ă̎q��transform��T���i������v�j
    /// </summary>
    /// <param name="parentTransform">�e��transform</param>
    /// <param name="tag">�����������^�O��</param>
    /// <returns>�q��transform���X�g</returns>
    public static List<Transform> FindWithAllChildTag(this Transform parentTransform, Tag tag)
    {
        return parentTransform.Cast<Transform>()
                              .ToList()
                              .Where(t => t.tag.Contains(tag.ToString()))
                              .ToList();
        //List<Transform> children = new List<Transform>();
        //foreach (Transform childTransform in parentTransform)
        //{
        //    if (childTransform.tag.Contains(tag.ToString()))
        //        children.Add(childTransform);
        //}
        //return children;
    }


    //public static int CountWithChildTag(this Transform parentTransform, string tag)
    //{
    //    int count = 0;

    //    foreach (Transform child in parentTransform)
    //    {
    //        if (child.CompareTag(tag)) count++;
    //    }

    //    return count;
    //}

    /// <summary>
    /// �w�肵���^�O�����܂ގq��transform�̐���T���i������v�j
    /// </summary>
    /// <param name="parentTransform">�e��transform</param>
    /// <param name="tag">�����������^�O��</param>
    /// <returns>�^�O���܂ގq��transform�̐�</returns>
    public static int CountWithChildTag(this Transform parentTransform, Tag tag)
    {
        var a = parentTransform.Cast<Transform>()
                              .ToList()
                              .Where(t => t.tag.Contains(tag.ToString()))
                              .Count();
        int count = 0;
        string tagName = tag.ToString();

        foreach (Transform child in parentTransform)
        {
            if (child.CompareTag(tagName)) count++;
        }

        return count;
    }

    public static FighterID GetFighterID(this Transform transform) => transform.root.GetComponent<Fighter>().ID;
    public static FighterID GetFighterID(this GameObject gameObject) => gameObject.transform.root.GetComponent<Fighter>().ID;


    /// <summary>
    /// transform���Œ肵�āA�w�肵���q��V�����e�ɕύX����
    /// </summary>
    /// <param name="childObject">�e��ύX����q�I�u�W�F�N�g</param>
    /// <param name="parentObject">�ύX��̐e�I�u�W�F�N�g</param>
    /// <param name="p">���W���Œ肷�邩</param>
    /// <param name="r">�������Œ肷�邩</param>
    /// <param name="s">�傫�����Œ肷�邩</param>
    public static void ChangeParent(this GameObject childObject, Transform parentObject, bool p = false, bool r = false, bool s = false)
    {
        var localp = childObject.transform.localPosition; // ���W���Œ肷�邽�߈ꎞ�ۑ�
        var localr = childObject.transform.localRotation;
        var locals = childObject.transform.localScale;
        childObject.transform.SetParent(parentObject);
        childObject.transform.position = parentObject.transform.position;
        if (p)
            childObject.transform.localPosition = localp;
        if (r)
            childObject.transform.localRotation = localr;
        if (s)
            childObject.transform.localScale = locals;

    }

    /// <summary>
    /// ����̑��I�u�W�F�N�g�����݂���C���f�b�N�X��T��
    /// </summary>
    /// <param name="parentTransform">�e��transform</param>
    /// <returns>���������C���f�b�N�X</returns>
    public static int FindWithGrandchildCard(this Transform parentTransform)
    {
        int i = 0;
        int cardCount = parentTransform.transform.CountWithChildTag(Tag.Card);
        int selectedIndex = -1;
        while (i < cardCount)
        {
            if (!ReferenceEquals(parentTransform.transform.GetChild(i).Find("Face/SelectedBox"), null))
            {
                selectedIndex = i;
                return selectedIndex;
            }
            i++;
        }
        return selectedIndex;
    }

    /// <summary>
    /// �v���n�u�������ɖ����ɕt�^�����"(clone)"�̖��O���폜����
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public static GameObject FixName(this GameObject gameObject)
    {
        gameObject.name = gameObject.name.Substring(0, gameObject.name.Length - 7); // (clone)�̕������폜
        return gameObject;
    }

    public static Result ToEnum (this int button, string one, string two)
    {
        if (button == 0)
        {
            return (Result)Enum.Parse(typeof(Result), one);
        }
        else
        {
            return (Result)Enum.Parse(typeof(Result), two);
        }
    }

    public static Card GetCard(this Transform parentTransform) => parentTransform.GetComponentInChildren<Card>();

    public static List<string> SplitEx(this string text, char separator) => text.Split(new char[] { separator }, options: StringSplitOptions.RemoveEmptyEntries).ToList();

    public static void MoveX(this Transform transform, float move) => transform.position = new Vector3(transform.position.x + move, transform.position.y, transform.position.z);
    public static void MoveY(this Transform transform, float move) => transform.position = new Vector3(transform.position.x, transform.position.y + move, transform.position.z);
    public static void MoveZ(this Transform transform, float move) => transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + move);
    public static void LocalMoveX(this Transform transform, float move) => transform.localPosition = new Vector3(transform.localPosition.x + move, transform.localPosition.y, transform.localPosition.z);
    public static void LocalMoveY(this Transform transform, float move) => transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + move, transform.localPosition.z);
    public static void LocalMoveZ(this Transform transform, float move) => transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + move);

    public static bool GetDown(this PlayerInput playerInput, string Command) => playerInput != null && playerInput.actions[Command].triggered;
}

public static class ListExtensions
{
    /// <summary>
    /// �����ɂ���I�u�W�F�N�g���폜���ĕԂ�
    /// </summary>
    public static T Pop<T>(this IList<T> self)
    {
        int index = self.Count - 1;
        var result = self[index];
        self.RemoveAt(index);
        return result;
    }

    /// <summary>
    /// �����ɃI�u�W�F�N�g��ǉ����܂�
    /// </summary>
    public static void Push<T>(this IList<T> self, T item)
    {
        self.Add(item);
    }
}
