using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    /// �w�肵���^�O�����܂ގq��transform��T��
    /// </summary>
    /// <param name="parentTransform">�e��transform</param>
    /// <param name="tag">�����������^�O��</param>
    /// <returns>�q��transform</returns>
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
    /// �w�肵���^�O�����܂ގq��GameObject��T��
    /// </summary>
    /// <param name="parentGameObject">�e��GameObject</param>
    /// <param name="tag">�����������^�O��</param>
    /// <returns>�q��GameObject</returns>
    public static GameObject FindWithChildTag(this GameObject parentGameObject, Tag tag)
    {
        foreach (Transform childTransform in parentGameObject.transform)
        {
            if (childTransform.tag.Contains(tag.ToString()))
                return childTransform.gameObject;
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
    /// �w�肵���^�O�����܂ގq��transform�̐���T��
    /// </summary>
    /// <param name="parentTransform">�e��transform</param>
    /// <param name="tag">�����������^�O��</param>
    /// <returns>�^�O���܂ގq��transform�̐�</returns>
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
}