using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// プロジェクトで使用する拡張メソッド
/// </summary>
public static class Extensions
{
    // 今のX位置から引数のX分だけ追加で移動
    //public static void AddPosX(this Transform transform, float x)
    //{
    //    var pos = transform.position;
    //    pos.x += x;
    //    transform.position = pos;
    //}

    /// <summary>
    /// 指定したタグが存在するか調べる（部分一致）
    /// </summary>
    /// <param name="transform">検索対象</param>
    /// <param name="tag">検索したいタグ</param>
    /// <returns>存在するか</returns>
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
    /// 指定したタグ名を含む子のtransformを探す（部分一致）
    /// </summary>
    /// <param name="parentTransform">親のtransform</param>
    /// <param name="tag">検索したいタグ名</param>
    /// <returns>子のtransform</returns>
    public static Transform FindWithChildTag(this Transform parentTransform, Tag tag)
    {
        foreach (Transform childTransform in parentTransform)
        {
            if (childTransform.tag.Contains(tag.ToString()))
                return childTransform;
        }
        return null;
    }

    /// <summary>
    /// 指定したタグ名を含む子のGameObjectを探す（部分一致）
    /// </summary>
    /// <param name="parentGameObject">親のGameObject</param>
    /// <param name="tag">検索したいタグ名</param>
    /// <returns>子のGameObject</returns>
    public static GameObject FindWithChildTag(this GameObject parentGameObject, Tag tag)
    {
        foreach (Transform childTransform in parentGameObject.transform)
        {
            if (childTransform.tag.Contains(tag.ToString()))
                return childTransform.gameObject;
        }
        return null;
    }

    /// <summary>
    /// 指定したタグ名を含む全ての子のtransformを探す（部分一致）
    /// </summary>
    /// <param name="parentTransform">親のtransform</param>
    /// <param name="tag">検索したいタグ名</param>
    /// <returns>子のtransformリスト</returns>
    public static List<Transform> FindWithAllChildTag(this Transform parentTransform, Tag tag)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform childTransform in parentTransform)
        {
            if (childTransform.tag.Contains(tag.ToString()))
                children.Add(childTransform);
        }
        return children;
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
    /// 指定したタグ名を含む子のtransformの数を探す（部分一致）
    /// </summary>
    /// <param name="parentTransform">親のtransform</param>
    /// <param name="tag">検索したいタグ名</param>
    /// <returns>タグを含む子のtransformの数</returns>
    public static int CountWithChildTag(this Transform parentTransform, Tag tag)
    {
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
    /// transformを固定して、指定した子を新しい親に変更する
    /// </summary>
    /// <param name="childObject">親を変更する子オブジェクト</param>
    /// <param name="parentObject">変更先の親オブジェクト</param>
    /// <param name="p">座標を固定するか</param>
    /// <param name="r">向きを固定するか</param>
    /// <param name="s">大きさを固定するか</param>
    public static void ChangeParent(this GameObject childObject, Transform parentObject, bool p = false, bool r = false, bool s = false)
    {
        var localp = childObject.transform.localPosition; // 座標を固定するため一時保存
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
    /// 特定の孫オブジェクトが存在するインデックスを探す
    /// </summary>
    /// <param name="parentTransform">親のtransform</param>
    /// <returns>検索したインデックス</returns>
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
    /// プレハブ生成時に末尾に付与される"(clone)"の名前を削除する
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public static GameObject FixName(this GameObject gameObject)
    {
        gameObject.name = gameObject.name.Substring(0, gameObject.name.Length - 7); // (clone)の部分を削除
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
}

public static class ListExtensions
{
    /// <summary>
    /// 末尾にあるオブジェクトを削除して返す
    /// </summary>
    public static T Pop<T>(this IList<T> self)
    {
        int index = self.Count - 1;
        var result = self[index];
        self.RemoveAt(index);
        return result;
    }

    /// <summary>
    /// 末尾にオブジェクトを追加します
    /// </summary>
    public static void Push<T>(this IList<T> self, T item)
    {
        self.Add(item);
    }
}
