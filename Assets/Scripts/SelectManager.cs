using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

/// <summary>
/// �J�[�\���I�����Ǘ�����
/// </summary>
public class SelectManager : MonoBehaviour
{
    private GameObject SelectBox;   // �J�[�\��
    private GameObject SelectedBox; // �I�𒆂̃J�[�h���߂�J�[�\��
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

    /// <summary>
    /// ���ݒn�̃}�X�ځA�C���f�b�N�X
    /// </summary>
    private ReactiveCollection<int> selectZoneIndex = new ReactiveCollection<int>() { 0, 2 };

    /// <summary>
    /// ��̃}�X�ړ��ňړ��\�ȏꍇ�p�̃C���f�b�N�X
    /// </summary>
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

        // �㉺���E�̓��͔���
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
            // �E�[�ɂ��Ȃ��Ƃ�
            selectZoneIndex[1]++;
            changeSelectBox = true;

        }
        else if (left && selectZoneIndex[1] > 0 && !IsHand())
        {
            // ���[�ɂ��Ȃ��Ƃ�
            selectZoneIndex[1]--;
            changeSelectBox = true;
        }
        else if (up && selectZoneIndex[0] > 0)
        {
            // ��[�ɂ��Ȃ��Ƃ�
            selectZoneIndex[0]--;
            changeSelectBox = true;
        }
        else if (down && SelectObjList.Count - 1 > selectZoneIndex[0])
        {
            // ���[�ɂ��Ȃ��Ƃ�
            selectZoneIndex[0]++;
            changeSelectBox = true;
        }

        if (changeSelectBox)
        {
            // �G���A�ړ������Ƃ�
            if (IsHand() && hand.transform.CountWithChildTag(Tag.Card) > 0)
                SelectBox.ChangeParent(hand.transform.GetChild(MultiSelectIndex).GetChild(0), p: true);
            else
                SelectBox.ChangeParent(SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].transform, p: true);

            // �q�I�u�W�F�N�g��S�Ď擾����
            //foreach (Transform childTransform in hand.transform)
            //{
            //    Debug.Log(childTransform.gameObject.name);
            //}
        }
        if (IsHand())
        {
            // �J�[�\������D��ɂ��鎞
            if (right && hand.transform.childCount - 1 > MultiSelectIndex)
            {
                MultiSelectIndex++;

            }
            else if (left && MultiSelectIndex > 0)
            {
                MultiSelectIndex--;
            }

            if (hand.transform.CountWithChildTag(Tag.Card) == 0)
                SelectBox.ChangeParent(SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].transform, p: true);
            else
                SelectBox.ChangeParent(hand.transform.GetChild(MultiSelectIndex).GetChild(0), p: true);
            //Debug.Log("hand!");
            //Debug.Log(MultiSelectIndex);
        }

    }

    /// <summary>
    /// 1�����I���\�ȃJ�[�\���I�����A�I�𒆂������I�u�W�F�N�g��z�u
    /// </summary>
    /// <returns>���ۂɑI��������</returns>
    public bool SingleSelected()
    {
        if (!IsHand() || hand.transform.CountWithChildTag(Tag.Card) == 0) return false;
        //Debug.Log(hand.transform.childCount);
        //Debug.Log(hand.transform.CountWithChildTag(Tag.Card));
        SelectedBox = Instantiate(SelectedBoxPrefab);
        SelectedBox.name = SelectedBox.name.Substring(0, SelectedBox.name.Length - 7); // (clone)�̕������폜
        SelectedBox.ChangeParent(hand.transform.GetChild(MultiSelectIndex).GetChild(0), true, true, true);
        return true;
    }

    /// <summary>
    /// 1�����I���\�ȃJ�[�\�����m�肵�A�I�𒆂������I�u�W�F�N�g���폜
    /// </summary>
    /// <param name="tag">����\�ȃ}�X</param>
    /// <returns>���ۂɌ��肵����</returns>
    public bool SingleConfirm(string tag)
    {
        if (!HasTag(tag)) return false;
        int i = 0;
        int cardCount = hand.transform.CountWithChildTag(Tag.Card);
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

        StartCoroutine(CardManager.Instance.HandToField(hand, SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].GetComponent<ICardCircle>(), selectedIndex));
        Destroy(SelectedBox);
        return true;

    }

    /// <summary>
    /// �I�𒆂̃J�[�\�����L�����Z��
    /// </summary>
    public void SingleCansel()
    {
        Destroy(SelectedBox);
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

    private bool IsHand() => !ReferenceEquals(SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].GetComponent<Hand>(), null);

    //private bool IsCardCircle() => SelectObjList[selectZoneIndex.Item1][selectZoneIndex[1]].tag.Contains("Circle");

    //private bool IsVanguard() => SelectObjList[selectZoneIndex.Item1][selectZoneIndex[1]].tag.Contains("Vanguard");
    private bool HasTag(string tag) => SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].tag.Contains(tag);



}
