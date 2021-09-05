using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using System.Linq;

/// <summary>
/// カーソル選択を管理する
/// </summary>
public class SelectManager : SingletonMonoBehaviour<SelectManager>
{
    public GameObject SelectBox { get; private set; }   // カーソル
    private List<GameObject> SelectedBoxList = new List<GameObject>(); // 選択中のカードを占めるカーソル
    public Fighter fighter1;
    public Fighter fighter2;
    private Hand hand1;
    private Hand hand2;

    public GameObject SelectBoxPrefab;
    public GameObject SelectedBoxPrefab;

    private Image ZoomImage;
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
    private GameObject SelectObj { get => SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]]; }
    public List<GameObject> SelectObjList1 = new List<GameObject>();
    public List<GameObject> SelectObjList2 = new List<GameObject>();
    public List<GameObject> SelectObjList3 = new List<GameObject>();

    private List<Transform> SelectedCardParentList = new List<Transform>();

// Start is called before the first frame update
    void Start()
    {
        hand1 = fighter1.hand;
        hand2 = fighter2.hand;
        GameObject field1 = fighter1.field;
        GameObject field2 = fighter2.field;

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
                fighter2.drop.gameObject,
                field2.transform.Find("Rearguard23").gameObject,
                field2.transform.Find("Rearguard22").gameObject,
                field2.transform.Find("Rearguard21").gameObject,
                fighter2.damage.gameObject,
            },
            new List<GameObject>()
            {
                fighter2.deck.gameObject,
                field2.transform.Find("Rearguard13").gameObject,
                fighter2.vanguard.gameObject,
                field2.transform.Find("Rearguard11").gameObject,
                fighter1.guardian.gameObject,
                fighter2.order.gameObject,
            },
            new List<GameObject>()
            {
                fighter2.guardian.gameObject,
                fighter2.guardian.gameObject,
                fighter2.guardian.gameObject,
                fighter2.guardian.gameObject,
                fighter2.guardian.gameObject,
            },
            new List<GameObject>()
            {
                fighter1.guardian.gameObject,
                fighter1.guardian.gameObject,
                fighter1.guardian.gameObject,
                fighter1.guardian.gameObject,
                fighter1.guardian.gameObject,
            },
            new List<GameObject>()
            {
                fighter1.order.gameObject,
                field1.transform.Find("Rearguard11").gameObject,
                fighter1.vanguard.gameObject,
                field1.transform.Find("Rearguard13").gameObject,
                fighter1.deck.gameObject,
            },
            new List<GameObject>()
            {
                fighter1.damage.gameObject,
                field1.transform.Find("Rearguard21").gameObject,
                field1.transform.Find("Rearguard22").gameObject,
                field1.transform.Find("Rearguard23").gameObject,
                fighter1.drop.gameObject,
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
        ZoomImage = GameObject.Find("Canvas").transform.Find("ZoomCard").GetComponent<Image>();
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

        if (MultiSelectIndex == -1) return;

        // エリア移動したとき
        if (changeSelectBox)
        {
            Fighter fighter = GetFighter();
            Hand hand = fighter.hand;

            // 手札以外の場所から手札に移動したとき
            if (IsHand() && hand.Count() > 0)
            {
                MultiSelectIndex = hand.Count() / 2; // 手札の数に合わせて初期化
                SelectBox.ChangeParent(hand.transform.GetChild(MultiSelectIndex), p: true);
            }
            else
                SelectBox.ChangeParent(SelectObj.transform, p: true);

            SetZoomCard();
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
            Hand hand = fighter.hand;

            // カーソルを左右に移動させる
            if (right && hand.Count() - 1 > MultiSelectIndex)
            {
                MultiSelectIndex++;
            }
            else if (left && MultiSelectIndex > 0)
            {
                MultiSelectIndex--;
            }

            if (hand.Count() > 0) // 手札のカードにカーソルを移動させる
            {
                SelectBox.ChangeParent(hand.transform.GetChild(MultiSelectIndex), p: true);
            }
            else // 手札がないとき
                SelectBox.ChangeParent(SelectObj.transform, p: true);
            //Debug.Log("hand!");
            //Debug.Log(MultiSelectIndex);
            SetZoomCard();
        }

    }

    public async UniTask<ICardZone> GetSelect(Tag tag, FighterID fighterID)
    {
        Fighter fighter = GetFighter(fighterID);

        if (!HasTag(tag)) return null;

        // カーソル位置が手札 かつ カーソル位置が指定したファイターのもの かつ 指定したファイターの手札が0枚じゃない
        if (HasTag(Tag.Hand) && IsFighter(fighterID) && fighter.hand.Count() > 0)
        {
            return fighter.hand.transform.GetChild(MultiSelectIndex).GetComponent<ICardZone>();
        }
        // カーソル位置がリアガード かつ カーソル位置が指定したファイターのもの かつ 指定したリアガードに既にカードが存在する
        //else if (HasTag(Tag.Rearguard) && IsFighter(fighterID) && SelectObj.FindWithChildTag(Tag.Card) != null)
        //{
        //    return Result.YES;
        //}
        // カーソル位置がサークル かつ カーソル位置が指定したファイターのもの かつ 指定したサークルに既にカードが存在する
        else if (HasTag(Tag.Circle) && IsFighter(fighterID) && SelectObj.FindWithChildTag(Tag.Card) != null)
        {
            return SelectObj.GetComponent<ICardZone>();
        }
        return null;
    }

    /// <summary>
    /// 1つだけ選択可能なカーソル選択し、選択中を示すオブジェクトを配置
    /// </summary>
    /// <param name="tag">選択可能なマス</param>
    /// <param name="fighterID">選択可能なファイターID</param>
    /// <returns>実際に選択したゾーンのtransform</returns>
    public async UniTask<Transform> NormalSelected(Tag tag, FighterID fighterID)
    {
        Fighter fighter = GetFighter(fighterID);

        if (!HasTag(tag)) return null;

        // カーソル位置が手札 かつ カーソル位置が指定したファイターのもの かつ 指定したファイターの手札が0枚じゃない
        if (HasTag(Tag.Hand) && IsFighter(fighterID) && fighter.hand.Count() > 0)
        {
            var selectedCard = fighter.hand.transform.GetChild(MultiSelectIndex);
            if (selectedCard.Find("SelectedBox")) return null;
            var selectedBox = Instantiate(SelectedBoxPrefab).FixName();
            SelectedBoxList.Add(selectedBox);
            selectedBox.ChangeParent(selectedCard, true, true, true);
            //SelectedBox.transform.RotateAround(SelectedBox.transform.position, Vector3.forward, 180);
            SelectedCardParentList.Add(selectedCard);
            return fighter.hand.transform;
        }
        // カーソル位置がリアガード かつ カーソル位置が指定したファイターのもの かつ 指定したリアガードに既にカードが存在する
        else if (HasTag(Tag.Rearguard) && IsFighter(fighterID) && SelectObj.FindWithChildTag(Tag.Card) != null)
        {
            var selectedBox = Instantiate(SelectedBoxPrefab).FixName();
            SelectedBoxList.Add(selectedBox);
            var selectedRearguard = SelectObj.transform;
            selectedBox.ChangeParent(selectedRearguard, true, true, true);
            SelectedCardParentList.Add(selectedRearguard);
            return selectedRearguard;
        }
        // カーソル位置がサークル かつ カーソル位置が指定したファイターのもの かつ 指定したサークルに既にカードが存在する
        else if (HasTag(Tag.Circle) && IsFighter(fighterID) && SelectObj.FindWithChildTag(Tag.Card) != null)
        {
            var selectedBox = Instantiate(SelectedBoxPrefab).FixName();
            SelectedBoxList.Add(selectedBox);
            var selectedRearguard = SelectObj.transform;
            selectedBox.ChangeParent(selectedRearguard, true, true, true);
            SelectedCardParentList.Add(selectedRearguard);
            return selectedRearguard;
        }
        //SingleCansel();
        return null;
    }


    /// <summary>
    /// 1つだけ選択可能なカーソルを確定し、選択中を示すオブジェクトを削除
    /// </summary>
    /// <param name="tag">決定可能なマス</param>
    /// <param name="fighterID">決定可能なファイターID</param>
    /// <param name="action">選択、決定したカードに対しとる行動</param>
    /// <returns></returns>
    public async UniTask<(Transform, Transform)> NormalConfirm(Tag tag, FighterID fighterID, Action action)
    {
        if (!HasTag(tag)) return (null, null);

        Fighter fighter = GetFighter(fighterID);

        Transform selected = SelectedCardParentList[0];

        (Transform Area, Transform Card) result = (Area: null, Card: null);

        if (action == Action.MOVE)
        {

            // 選択したカーソル位置が手札 かつ 今のカーソル位置がサークル かつ 今のカーソル位置が指定したファイターのもの
            if (selected.parent.ExistTag(Tag.Hand) && HasTag(Tag.Circle) && IsFighter(fighterID))
            {
                result = (SelectObj.transform, selected.FindWithChildTag(Tag.Card));
                goto END;
            }
            // 選択したカーソル位置がリアガード かつ 今のカーソル位置がリアガード かつ 今のカーソル位置が指定したファイターのもの かつ同じ縦列である
            else if (selected.ExistTag(Tag.Rearguard) && HasTag(Tag.Rearguard) && IsFighter(fighterID)
                && selected.GetComponent<Rearguard>().IsSameColumn(SelectObj.GetComponent<Rearguard>()))
            {
                result = (SelectObj.transform, selected.FindWithChildTag(Tag.Card));
                goto END;
            }
            else
            {
                return (null, null);
            }
        }
        else if (action == Action.ATTACK)
        {
            // 選択したカーソル位置がサークル かつ 今のカーソル位置がサークル かつ 今のカーソル位置が指定したファイターのもの かつ 指定したサークルに既にカードが存在する
            if (selected.ExistTag(Tag.Circle) && HasTag(Tag.Vanguard) && IsFighter(fighterID) && SelectObj.FindWithChildTag(Tag.Card) != null)
            {
                //Debug.Log("Vにアタック");
                result = (SelectObj.transform, null);
                goto END;
            }
            else
            {
                return (null, null);
            }
        }
        else
        {
            return (null, null);
        }

        END:

        SelectedCardParentList.Clear();
        foreach (var selectedBox in SelectedBoxList)
        {
            Destroy(selectedBox);
        }
        SelectedBoxList.Clear();
        return result;

    }

    public async UniTask ForceConfirm(Tag tag, FighterID fighterID, Action action)
    {
        var SelectedZip = SelectedBoxList.Zip(SelectedCardParentList, (box, parent) => (Box: box, Parent: parent));
        Fighter fighter = GetFighter(fighterID);

        MultiSelectIndex = -1;
        SelectBox.transform.parent = null;

        foreach (var selectedZip in SelectedZip)
        {
            //if (selectedZip.Parent.Find("SelectBox") == SelectBox.transform)
            //{
            //    print("yabai");
            //    // カーソルを左右に移動させる
            //    if (MultiSelectIndex > 0)
            //        MultiSelectIndex
            //    else
            //        MultiSelectIndex++;
            //    if (fighter.hand.Count() > 0) // 手札のカードにカーソルを移動させる
            //    {
            //        SelectBox.ChangeParent(fighter.hand.transform.GetChild(MultiSelectIndex), p: true);
            //    }
            //    else // 手札がないとき
            //        SelectBox.ChangeParent(SelectObj.transform, p: true);
            //}
            await CardManager.Instance.HandToDeck(fighter.hand, fighter.deck, selectedZip.Parent.FindWithChildTag(Tag.Card).GetComponent<Card>());
        }
        SelectedBoxList.Clear();
        SelectedCardParentList.Clear();
        foreach (var box in SelectedBoxList)
        {
            Destroy(box);
        }

        await UniTask.NextFrame();

        MultiSelectIndex = 0;
        if (fighter.hand.Count() > 0) // 手札のカードにカーソルを移動させる
        {
            SelectBox.ChangeParent(fighter.hand.transform.GetChild(MultiSelectIndex), p: false);
        }
        //else // 手札がないとき
        //    SelectBox.ChangeParent(SelectObj.transform, p: true);
    }

    /// <summary>
    /// 選択中のカーソルをキャンセル
    /// </summary>
    public void SingleCansel()
    {
        SelectedCardParentList.Pop();
        Destroy(SelectedBoxList.Last());
        SelectedBoxList.Pop();
    }

    public void MultSelected()
    {

    }

    public int SelectedCount() => SelectedCardParentList.Count;

    /// <summary>
    /// カードの拡大機能のオンオフを切り替える
    /// </summary>
    public void ZoomCard()
    {
        ZoomImage.enabled = !ZoomImage.enabled;
        SetZoomCard();
    }

    /// <summary>
    /// カードの拡大機能で表示するカードの設定を行う
    /// </summary>
    private void SetZoomCard()
    {
        if (!ZoomImage.enabled) return;
        if (HasTag(Tag.Deck)) return;
        if (HasTag(Tag.Hand) && GetFighter(FighterID.ONE).hand.Count() > 0)
        {
            var card = SelectObj.transform.GetChild(MultiSelectIndex).FindWithChildTag(Tag.Card).GetComponent<Card>();
            var cardTexture = (Texture2D)card.GetTexture();
            ZoomImage.sprite = Sprite.Create(cardTexture, new Rect(0.0f, 0.0f, cardTexture.width, cardTexture.height), new Vector2(0.5f, 0.5f), 100.0f); ;
        }
        else if (SelectObj.FindWithChildTag(Tag.Card) != null)
        {
            var card = SelectObj.FindWithChildTag(Tag.Card).GetComponent<Card>();
            var cardTexture = (Texture2D)card.GetTexture();
            ZoomImage.sprite = Sprite.Create(cardTexture, new Rect(0.0f, 0.0f, cardTexture.width, cardTexture.height), new Vector2(0.5f, 0.5f), 100.0f); ;
        }
    }

    private void ChangeHandCount(int count)
    {
        if (hand1.cardList.Count <= MultiSelectIndex)
        {
            MultiSelectIndex = hand1.cardList.Count - 1;
            if (MultiSelectIndex < 0) MultiSelectIndex = 0;
        }
    }

    private bool IsHand() => !ReferenceEquals(SelectObj.GetComponent<Hand>(), null);

    //private bool IsCardCircle() => SelectObjList[selectZoneIndex.Item1][selectZoneIndex[1]].tag.Contains("Circle");

    //private bool IsVanguard() => SelectObjList[selectZoneIndex.Item1][selectZoneIndex[1]].tag.Contains("Vanguard");

    /// <summary>
    /// 現在のカーソル位置にあるオブジェクトのタグと指定したタグが部分一致しているか調べる
    /// </summary>
    /// <param name="tag">判定に使うタグ</param>
    /// <returns>部分一致したかどうか</returns>
    private bool HasTag(Tag tag) => SelectObj.tag.Contains(tag.ToString());

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
    private Fighter GetFighter() => SelectObj.transform.root.GetComponent<Fighter>();

    /// <summary>
    /// 指定したファイターIDを持つファイターを返す
    /// </summary>
    /// <param name="fighterID">判定に使うファイターID</param>
    /// <returns>条件を満たすファイター</returns>
    private Fighter GetFighter(FighterID fighterID) => fighter1.ID == fighterID ? fighter1 : fighter2;

}
