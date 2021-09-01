using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    // ¡‚ÌXˆÊ’u‚©‚çˆø”‚ÌX•ª‚¾‚¯’Ç‰Á‚ÅˆÚ“®
    //public static void AddPosX(this Transform transform, float x)
    //{
    //    var pos = transform.position;
    //    pos.x += x;
    //    transform.position = pos;
    //}

    public static Transform FindWithChildTag(this Transform transform, string tag)
    {
        foreach (Transform childTransform in transform)
        {
            if (childTransform.tag.Contains(tag))
                return childTransform;
        }
        return null;
    }

    public static Transform FindWithChildTag(this Transform transform, Tag tag)
    {
        foreach (Transform childTransform in transform)
        {
            if (childTransform.tag.Contains(tag.ToString()))
                return childTransform;
        }
        return null;
    }

    public static int CountWithChildTag(this Transform parentTransform, string tag)
    {
        int count = 0;

        foreach (Transform child in parentTransform)
        {
            if (child.CompareTag(tag)) count++;
        }

        return count;
    }

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
}