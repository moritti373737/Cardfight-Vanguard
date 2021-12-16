using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScrollManager : SingletonMonoBehaviour<ScrollManager>
{
    [SerializeField]
    private GameObject CheckCardPrefab;

    private List<RectTransform> CheckList = new List<RectTransform>();
    private int selectIndex = 0;

    public enum ScrollDirectType
    {
        Right,
        Left,
    }

    public void SetCheckList(List<Card> cardList)
    {
        GameObject canvas = GameObject.Find("Canvas");
        var image = canvas.transform.Find("CheckPanel");
        if (image.gameObject.activeSelf) return;
        image.gameObject.SetActive(true);

        foreach (var (card, index) in cardList.Select((card, index) => (card, index)))
        {
            var check = Instantiate(CheckCardPrefab);
            Texture texture = card.Face.GetComponent<Renderer>().material.mainTexture;
            check.GetComponent<Image>().sprite = Sprite.Create(texture as Texture2D, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
            check.transform.SetParent(image);
            var checkRext = check.GetComponent<RectTransform>();
            checkRext.localScale = Vector3.one;
            checkRext.localPosition = new Vector3(index * 300, 0, 0);
            CheckList.Add(checkRext);
        }
    }
    internal void DeleteCheckList()
    {
        GameObject canvas = GameObject.Find("Canvas");
        var image = canvas.transform.Find("CheckPanel");
        CheckList.ForEach(card => Destroy(card.gameObject));
        CheckList.Clear();
        selectIndex = 0;
        image.gameObject.SetActive(false);
    }

    public void Scroll(ScrollDirectType direct)
    {
        switch (direct)
        {
            case ScrollDirectType.Right:
                if (CheckList.Count - 1 > selectIndex)
                selectIndex++;
                break;
            case ScrollDirectType.Left:
                if (0 < selectIndex) selectIndex--;
                break;
        }

        foreach (var (card, index) in CheckList.Select((card, index) => (card, index)))
        {
            card.localPosition = new Vector3((index - selectIndex) * 300, 0, 0);
        }
    }

}
