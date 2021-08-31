using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class SelectManager : MonoBehaviour
{
    private GameObject SelectBox;
    private GameObject SelectedBox;
    public Hand hand;

    public GameObject SelectBoxPrefab;
    public GameObject SelectedBoxPrefab;
    //public GameObject R13;

    //private enum Zone
    //{
    //    V,
    //    R11,
    //    R13,
    //    R21,
    //    R22,
    //    R23,
    //    DECK,
    //    DROP,
    //    HAND,
    //};

    private (int, int) selectZoneIndex = (0, 2);
    private int MultiSelectIndex = 0;

    //private List<List<Zone>> SelectList = new List<List<Zone>>();
    private List<List<GameObject>> SelectObjList = new List<List<GameObject>>();
    public List<GameObject> SelectObjList1 = new List<GameObject>();
    public List<GameObject> SelectObjList2 = new List<GameObject>();
    public List<GameObject> SelectObjList3 = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        SelectBox = Instantiate(SelectBoxPrefab);
        SelectBox.name = "SelectBox";
        ChangeParent(SelectObjList1[2].transform, SelectBox, true, true, true);
        //SelectList.Add(new List<Zone>() { Zone.R11, Zone.V, Zone.R13, Zone.DECK });
        //SelectList.Add(new List<Zone>() { Zone.R21, Zone.R22, Zone.R23, Zone.DROP });
        //SelectList.Add(new List<Zone>() { Zone.HAND, Zone.HAND, Zone.HAND, Zone.HAND });

        //for (int i = 0; i < SelectList.Count; i++)
        //{
        //    for (int j = 0; j < SelectList[i].Count; j++)
        //    {
        //        Debug.Log(SelectList[i][j]);
        //    }
        //}

        SelectObjList.Add(SelectObjList1);
        SelectObjList.Add(SelectObjList2);
        SelectObjList.Add(SelectObjList3);

        hand.cardList.ObserveCountChanged().Subscribe(count => ChangeHandCount(count));

    }

    // Update is called once per frame
    void Update()
    {
        bool right = false;
        bool left = false;
        bool up = false;
        bool down = false;
        bool changeSelectBox = false;

        if (Input.GetButtonDown("Horizontal") && Input.GetAxisRaw("Horizontal") == 1)
        {
            right = true;
        }
        else if (Input.GetButtonDown("Horizontal") && Input.GetAxisRaw("Horizontal") == -1)
        {
            left = true;
        }
        else if (Input.GetButtonDown("Vertical") && Input.GetAxisRaw("Vertical") == 1)
        {
            up = true;
        }
        else if (Input.GetButtonDown("Vertical") && Input.GetAxisRaw("Vertical") == -1)
        {
            down = true;
        }

        if (right && SelectObjList[selectZoneIndex.Item1].Count - 1 > selectZoneIndex.Item2 && !IsHand())
        {
            selectZoneIndex.Item2++;
            changeSelectBox = true;

        }
        else if (left && selectZoneIndex.Item2 > 0 && !IsHand())
        {
            selectZoneIndex.Item2--;
            changeSelectBox = true;
        }
        else if (up && selectZoneIndex.Item1 > 0)
        {
            selectZoneIndex.Item1--;
            changeSelectBox = true;
        }
        else if (down && SelectObjList.Count - 1 > selectZoneIndex.Item1)
        {
            selectZoneIndex.Item1++;
            changeSelectBox = true;

        }

        if (changeSelectBox)
        {
            Debug.Log("changeSelectBox");
            //ChangeParent(SelectObjList[selectZoneIndex.Item1][selectZoneIndex.Item2].transform, SelectBox, p: true);
            if (IsHand() && CountChildAndTag(hand.transform, "Card") > 0)
                ChangeParent(hand.transform.GetChild(MultiSelectIndex).GetChild(0), SelectBox, p: true);
            else
                ChangeParent(SelectObjList[selectZoneIndex.Item1][selectZoneIndex.Item2].transform, SelectBox, p: true);
            //Debug.Log(hand.transform.childCount);
            // 子オブジェクトを全て取得する
            //foreach (Transform childTransform in hand.transform)
            //{
            //    Debug.Log(childTransform.gameObject.name);
            //}
        }
        //Debug.Log(selectZone);
        //Debug.Log(SelectObjList[selectZone.Item1][selectZone.Item2].GetType());
        //Debug.Log(hand.Count());

        //Debug.Log(hand.GetType());
        //Debug.Log(SelectObjList[selectZone.Item1][selectZone.Item2].GetComponent<Hand>());

        if (IsHand())
        {

            if (right && hand.transform.childCount - 1 > MultiSelectIndex)
            {
                MultiSelectIndex++;

            }
            else if (left && MultiSelectIndex > 0)
            {
                MultiSelectIndex--;
            }

            if (CountChildAndTag(hand.transform, "Card") == 0)
                ChangeParent(SelectObjList[selectZoneIndex.Item1][selectZoneIndex.Item2].transform, SelectBox, p: true);
            else
                ChangeParent(hand.transform.GetChild(MultiSelectIndex).GetChild(0), SelectBox, p: true);
            //Debug.Log("hand!");
            //Debug.Log(MultiSelectIndex);
        }

    }

    public bool SingleSelected()
    {
        if (!IsHand()) return false;
        Debug.Log(hand.transform.childCount);
        Debug.Log(CountChildAndTag(hand.transform, "Card"));
        SelectedBox = Instantiate(SelectedBoxPrefab);
        SelectedBox.name = SelectedBox.name.Substring(0, SelectedBox.name.Length - 7); // (clone)の部分を削除
        ChangeParent(hand.transform.GetChild(MultiSelectIndex).GetChild(0), SelectedBox, true, true, true);
        return true;
    }

    public bool SingleConfirm(string tag)
    {
        if (!HasTag(tag)) return false;
        int i = 0;
        int cardCount = CountChildAndTag(hand.transform, "Card");
        int selectedIndex = -1;
        while (i < cardCount)
        {
            if (!ReferenceEquals(hand.transform.GetChild(i).Find("Face/SelectedBox"), null))
            {
                selectedIndex = i;
                break;
            }
            i++;
        }

        StartCoroutine(CardManager.Instance.HandToField(hand, SelectObjList[selectZoneIndex.Item1][selectZoneIndex.Item2].GetComponent<ICardCircle>(), selectedIndex));
        Destroy(SelectedBox);
        return true;

    }

    public void MultSelected()
    {

    }

    private void ChangeHandCount(int count)
    {
        if (hand.cardList.Count <= MultiSelectIndex)
        {
            MultiSelectIndex = hand.cardList.Count - 1;
        }
    }

    private void ChangeParent(Transform parentObject, GameObject childObject, bool p = false, bool r = false, bool s = false)
    {
        var localp = childObject.transform.localPosition; // Local座標は固定するため一時保存
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

    private bool IsHand() => !ReferenceEquals(SelectObjList[selectZoneIndex.Item1][selectZoneIndex.Item2].GetComponent<Hand>(), null);

    //private bool IsCardCircle() => SelectObjList[selectZoneIndex.Item1][selectZoneIndex.Item2].tag.Contains("Circle");

    //private bool IsVanguard() => SelectObjList[selectZoneIndex.Item1][selectZoneIndex.Item2].tag.Contains("Vanguard");
    private bool HasTag(string tag) => SelectObjList[selectZoneIndex.Item1][selectZoneIndex.Item2].tag.Contains(tag);


    private int CountChildAndTag(Transform parentObject, string tag)
    {
        int count = 0;

        foreach (Transform child in parentObject)
        {
            if (child.tag == tag) count++;
        }

        return count;
    }

}
