using System.Collections;
using System.Collections.Generic;
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
    /// 指定したタグ名を含む子のtransformを探す
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
    /// 指定したタグ名を含む子のtransformの数を探す
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
}