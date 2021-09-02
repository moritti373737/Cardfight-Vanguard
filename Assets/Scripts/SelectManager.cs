using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

/// <summary>
/// カーソル選択を管理する
/// </summary>
public class SelectManager : SingletonMonoBehaviour<SelectManager>
{
    public GameObject SelectBox;   // カーソル
    private GameObject SelectedBox; // 選択中のカードを占めるカーソル
    public Fighter fighter1;
    public Fighter fighter2;
    private Hand hand1;
    private Hand hand2;
    public GameObject Field1;
    public GameObject Field2;

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

    /// <summary>
    /// 現在地のマス目、インデックス
    /// </summary>
    private ReactiveCollection<int> selectZoneIndex = new ReactiveCollection<int>() { 5, 2 };

    /// <summary>
    /// 一つのマス目内で移動可能な場合用のインデックス
    /// </summary>
    private int MultiSelectIndex = 0;

    private List<List<GameObject>> SelectObjList;
    public List<GameObject> SelectObjList1 = new List<GameObject>();
    public List<GameObject> SelectObjList2 = new List<GameObject>();
    public List<GameObject> SelectObjList3 = new List<GameObject>();

    private List<Transform> SelectedCardParentList = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        hand1 = fighter1.transform.FindWithChildTag(Tag.Hand).GetComponent<Hand>();
        hand2 = fighter2.transform.FindWithChildTag(Tag.Hand).GetComponent<Hand>();

        SelectObjList = new List<List<GameObject>>()
        {
            new List<GameObject>()
            {
                hand2.gameObject,
                hand2.gameObject,
                hand2.gameObject,
                hand2.gameObject,
                hand2.gameObject,
            },
            new List<GameObject>()
            {
                Field2.FindWithChildTag(Tag.Drop),
                Field2.transform.Find("Rearguard2-3").gameObject,
                Field2.transform.Find("Rearguard2-2").gameObject,
                Field2.transform.Find("Rearguard2-1").gameObject,
                Field2.FindWithChildTag(Tag.Damage),
            },
            new List<GameObject>()
            {
                Field2.FindWithChildTag(Tag.Deck),
                Field2.transform.Find("Rearguard1-3").gameObject,
                Field2.FindWithChildTag(Tag.Vanguard),
                Field2.transform.Find("Rearguard1-1").gameObject,
                Field2.FindWithChildTag(Tag.Order),
            },
            new List<GameObject>()
            {
                Field2.FindWithChildTag(Tag.Guardian),
                Field2.FindWithChildTag(Tag.Guardian),
                Field2.FindWithChildTag(Tag.Guardian),
                Field2.FindWithChildTag(Tag.Guardian),
                Field2.FindWithChildTag(Tag.Guardian),
            },
            new List<GameObject>()
            {
                Field1.FindWithChildTag(Tag.Guardian),
                Field1.FindWithChildTag(Tag.Guardian),
                Field1.FindWithChildTag(Tag.Guardian),
                Field1.FindWithChildTag(Tag.Guardian),
                Field1.FindWithChildTag(Tag.Guardian),
            },
            new List<GameObject>()
            {
                Field1.FindWithChildTag(Tag.Order),
                Field1.transform.Find("Rearguard1-1").gameObject,
                Field1.FindWithChildTag(Tag.Vanguard),
                Field1.transform.Find("Rearguard1-3").gameObject,
                Field1.FindWithChildTag(Tag.Deck),
            },
            new List<GameObject>()
            {
                Field1.FindWithChildTag(Tag.Damage),
                Field1.transform.Find("Rearguard2-1").gameObject,
                Field1.transform.Find("Rearguard2-2").gameObject,
                Field1.transform.Find("Rearguard2-3").gameObject,
                Field1.FindWithChildTag(Tag.Drop),
            },
            new List<GameObject>()
            {
                hand1.gameObject,
                hand1.gameObject,
                hand1.gameObject,
                hand1.gameObject,
                hand1.gameObject,
            },
        };

        SelectBox = Instantiate(SelectBoxPrefab).FixName();
        SelectBox.ChangeParent(SelectObjList1[2].transform, true, true, true);
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

        //SelectObjList.Add(SelectObjList1);
        //SelectObjList.Add(SelectObjList2);
        //SelectObjList.Add(SelectObjList3);

        hand1.cardList.ObserveCountChanged().Subscribe(count => ChangeHandCount(count));

    }

    // Update is called once per frame
    void Update()
    {
        bool right = false;
        bool left = false;
        bool up = false;
        bool down = false;
        bool changeSelectBox = false;

        // 上下左右の入力判定
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

        if (right && SelectObjList[selectZoneIndex[0]].Count - 1 > selectZoneIndex[1] && !IsHand())
        {
            // 右端にいない かつ 手札にカーソルがないとき
            selectZoneIndex[1]++;
            changeSelectBox = true;

        }
        else if (left && selectZoneIndex[1] > 0 && !IsHand())
        {
            // 左端にいない かつ 手札にカーソルがないとき
            selectZoneIndex[1]--;
            changeSelectBox = true;
        }
        else if (up && selectZoneIndex[0] > 0)
        {
            // 上端にいないとき
            selectZoneIndex[0]--;
            changeSelectBox = true;
        }
        else if (down && SelectObjList.Count - 1 > selectZoneIndex[0])
        {
            // 下端にいないとき
            selectZoneIndex[0]++;
            changeSelectBox = true;
        }

        // エリア移動したとき
        if (changeSelectBox)
        {
            Fighter fighter = GetFighter();
            Transform hand = fighter.transform.FindWithChildTag(Tag.Hand);

            // 手札以外の場所から手札に移動したとき
            if (IsHand() && hand.CountWithChildTag(Tag.EmptyCard) > 0)
            {
                MultiSelectIndex = hand.CountWithChildTag(Tag.EmptyCard) / 2; // 手札の数に合わせて初期化
                SelectBox.ChangeParent(hand.GetChild(MultiSelectIndex), p: true);
            }
            else
                SelectBox.ChangeParent(SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].transform, p: true);

            // 子オブジェクトを全て取得する
            //foreach (Transform childTransform in hand.transform)
            //{
            //    Debug.Log(childTransform.gameObject.name);
            //}
        }
        // カーソルが手札上にある時
        else if (IsHand())
        {
            Fighter fighter = GetFighter();
            Transform hand = fighter.transform.FindWithChildTag(Tag.Hand);

            // カーソルを左右に移動させる
            if (right && hand.childCount - 1 > MultiSelectIndex)
            {
                MultiSelectIndex++;
            }
            else if (left && MultiSelectIndex > 0)
            {
                MultiSelectIndex--;
            }

            if (hand.CountWithChildTag(Tag.EmptyCard) > 0) // 手札のカードにカーソルを移動させる
                SelectBox.ChangeParent(hand.GetChild(MultiSelectIndex), p: true);
            else // 手札がないとき
                SelectBox.ChangeParent(SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].transform, p: true);
            //Debug.Log("hand!");
            //Debug.Log(MultiSelectIndex);
        }

    }

    /// <summary>
    /// 1つだけ選択可能なカーソル選択し、選択中を示すオブジェクトを配置
    /// </summary>
    /// <param name="tag">選択可能なマス</param>
    /// <param name="fighterID">選択可能なファイターID</param>
    /// <returns>実際に選択したか</returns>
    public bool SingleSelected(Tag tag, FighterID fighterID)
    {
        Fighter fighter = GetFighter(fighterID);

        if (!HasTag(tag)) return false;

        SelectedBox = Instantiate(SelectedBoxPrefab).FixName();

        // カーソル位置が手札 かつ カーソル位置が指定したファイターのもの かつ 指定したファイターの手札が0枚じゃない
        if (HasTag(Tag.Hand) && IsFighter(fighterID) && fighter.transform.FindWithChildTag(Tag.Hand).CountWithChildTag(Tag.EmptyCard) > 0)
        {
            var selectedCard = fighter.transform.FindWithChildTag(Tag.Hand).GetChild(MultiSelectIndex);
            SelectedBox.ChangeParent(selectedCard, true, true, true);
            //SelectedBox.transform.RotateAround(SelectedBox.transform.position, Vector3.forward, 180);
            SelectedCardParentList.Add(selectedCard);
        }
        // カーソル位置がリアガード かつ カーソル位置が指定したファイターのもの かつ 指定したリアガードに既にカードが存在する
        else if (HasTag(Tag.Rearguard) && IsFighter(fighterID) && SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].FindWithChildTag(Tag.Card) != null)
        {
            var selectedRearguard = SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]];
            SelectedBox.ChangeParent(selectedRearguard.transform, true, true, true);
            SelectedCardParentList.Add(selectedRearguard.transform);
        }
        // カーソル位置がサークル かつ カーソル位置が指定したファイターのもの かつ 指定したサークルに既にカードが存在する
        else if (HasTag(Tag.Circle) && IsFighter(fighterID) && SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].FindWithChildTag(Tag.Card) != null)
        {
            var selectedRearguard = SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]];
            SelectedBox.ChangeParent(selectedRearguard.transform, true, true, true);
            SelectedCardParentList.Add(selectedRearguard.transform);
        }
        else
        {
            SingleCansel();
            return false;
        }
        return true;
    }

    /// <summary>
    /// 1つだけ選択可能なカーソルを確定し、選択中を示すオブジェクトを削除
    /// </summary>
    /// <param name="tag">決定可能なマス</param>
    /// <param name="fighterID">決定可能なファイターID</param>
    /// <param name="action">選択、決定したカードに対しとる行動</param>
    /// <returns>実際に決定したか</returns>
    public bool SingleConfirm(Tag tag, FighterID fighterID, Action action)
    {
        if (!HasTag(tag)) return false;

        Fighter fighter = GetFighter(fighterID);


        foreach (var selected in SelectedCardParentList)
        {
            if (action == Action.MOVE)
            {

                // 選択したカーソル位置が手札 かつ 今のカーソル位置がサークル かつ 今のカーソル位置が指定したファイターのもの
                if (selected.parent.ExistTag(Tag.Hand) && HasTag(Tag.Circle) && IsFighter(fighterID))
                {
                    Debug.Log(fighter.transform.FindWithChildTag(Tag.Hand).GetComponent<Hand>());
                    Debug.Log(SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].GetComponent<ICardCircle>());
                    Debug.Log(selected.FindWithChildTag(Tag.Card).GetComponent<Card>());
                    StartCoroutine(CardManager.Instance.HandToField(fighter.transform.FindWithChildTag(Tag.Hand).GetComponent<Hand>(), SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].GetComponent<ICardCircle>(), selected.FindWithChildTag(Tag.Card).GetComponent<Card>()));
                }
                // 選択したカーソル位置がリアガード かつ 今のカーソル位置がリアガード かつ 今のカーソル位置が指定したファイターのもの かつ同じ縦列である
                else if (selected.ExistTag(Tag.Rearguard) && HasTag(Tag.Rearguard) && IsFighter(fighterID)
                    && selected.GetComponent<Rearguard>().IsSameColumn(SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].GetComponent<Rearguard>()))
                {
                    StartCoroutine(CardManager.Instance.RearToRear(selected.GetComponent<Rearguard>(), SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].GetComponent<Rearguard>(), selected.FindWithChildTag(Tag.Card).GetComponent<Card>()));
                }
                else
                {
                    return false;
                }
            }
            else if (action == Action.ATTACK)
            {
                // 選択したカーソル位置がサークル かつ 今のカーソル位置がサークル かつ 今のカーソル位置が指定したファイターのもの
                if (selected.ExistTag(Tag.Circle) && HasTag(Tag.Vanguard) && IsFighter(fighterID))
                {
                    Debug.Log("Vにアタック");
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        SelectedCardParentList.Clear();
        Destroy(SelectedBox);
        return true;

    }

    /// <summary>
    /// 選択中のカーソルをキャンセル
    /// </summary>
    public void SingleCansel()
    {
        SelectedCardParentList.Clear();
        Destroy(SelectedBox);
    }

    public void MultSelected()
    {

    }

    private void ChangeHandCount(int count)
    {
        if (hand1.cardList.Count <= MultiSelectIndex)
        {
            MultiSelectIndex = hand1.cardList.Count - 1;
        }
    }

    private bool IsHand() => !ReferenceEquals(SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].GetComponent<Hand>(), null);

    //private bool IsCardCircle() => SelectObjList[selectZoneIndex.Item1][selectZoneIndex[1]].tag.Contains("Circle");

    //private bool IsVanguard() => SelectObjList[selectZoneIndex.Item1][selectZoneIndex[1]].tag.Contains("Vanguard");

    /// <summary>
    /// 現在のカーソル位置にあるオブジェクトのタグと指定したタグが部分一致しているか調べる
    /// </summary>
    /// <param name="tag">判定に使うタグ</param>
    /// <returns>部分一致したかどうか</returns>
    private bool HasTag(Tag tag) => SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].tag.Contains(tag.ToString());

    /// <summary>
    /// 現在のカーソル位置にあるオブジェクトを所有するファイターと指定したファイターが一致しているか調べる
    /// </summary>
    /// <param name="fighterID">判定に使うファイターID</param>
    /// <returns>一致したかどうか</returns>
    private bool IsFighter(FighterID fighterID) => GetFighter().ID == fighterID;

    /// <summary>
    /// 現在のカーソル位置にあるオブジェクトを所有するファイターを返す
    /// </summary>
    /// <returns>条件を満たすファイター</returns>
    private Fighter GetFighter() => SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].transform.root.GetComponent<Fighter>();

    /// <summary>
    /// 指定したファイターIDを持つファイターを返す
    /// </summary>
    /// <param name="fighterID">判定に使うファイターID</param>
    /// <returns>条件を満たすファイター</returns>
    private Fighter GetFighter(FighterID fighterID) => fighter1.ID == fighterID ? fighter1 : fighter2;

}
