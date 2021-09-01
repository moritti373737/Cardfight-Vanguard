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
    public Hand hand1;
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
    /// ���ݒn�̃}�X�ځA�C���f�b�N�X
    /// </summary>
    private ReactiveCollection<int> selectZoneIndex = new ReactiveCollection<int>() { 5, 2 };

    /// <summary>
    /// ��̃}�X�ړ��ňړ��\�ȏꍇ�p�̃C���f�b�N�X
    /// </summary>
    private int MultiSelectIndex = 0;

    private List<List<GameObject>> SelectObjList;
    public List<GameObject> SelectObjList1 = new List<GameObject>();
    public List<GameObject> SelectObjList2 = new List<GameObject>();
    public List<GameObject> SelectObjList3 = new List<GameObject>();

    private Tag SingleSelectedTag = Tag.None;

    // Start is called before the first frame update
    void Start()
    {
        SelectObjList = new List<List<GameObject>>()
        {
            new List<GameObject>()
            {
                hand1.gameObject,
                hand1.gameObject,
                hand1.gameObject,
                hand1.gameObject,
                hand1.gameObject,
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
            if (IsHand() && hand1.transform.CountWithChildTag(Tag.Card) > 0)
                SelectBox.ChangeParent(hand1.transform.GetChild(MultiSelectIndex).GetChild(0), p: true);
            else
                SelectBox.ChangeParent(SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].transform, p: true);

            // �q�I�u�W�F�N�g��S�Ď擾����
            //foreach (Transform childTransform in hand.transform)
            //{
            //    Debug.Log(childTransform.gameObject.name);
            //}
        }

        // �J�[�\������D��ɂ��鎞
        if (IsHand())
        {
            // �J�[�\�������E�Ɉړ�������
            if (right && hand1.transform.childCount - 1 > MultiSelectIndex)
            {
                MultiSelectIndex++;

            }
            else if (left && MultiSelectIndex > 0)
            {
                MultiSelectIndex--;
            }

            if (hand1.transform.CountWithChildTag(Tag.Card) > 0) // ��D�̃J�[�h�ɃJ�[�\�����ړ�������
                SelectBox.ChangeParent(hand1.transform.GetChild(MultiSelectIndex).GetChild(0), p: true);
            else // ��D���Ȃ��Ƃ�
                SelectBox.ChangeParent(SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].transform, p: true);
            //Debug.Log("hand!");
            //Debug.Log(MultiSelectIndex);
        }

    }

    /// <summary>
    /// 1�����I���\�ȃJ�[�\���I�����A�I�𒆂������I�u�W�F�N�g��z�u
    /// </summary>
    /// <param name="tag">�I���\�ȃ}�X</param>
    /// <returns>���ۂɑI��������</returns>
    public bool SingleSelected(Tag tag)
    {
        if (!HasTag(tag)) return false;
        if (HasTag(Tag.Hand) && hand1.transform.CountWithChildTag(Tag.Card) > 0)
        {
            SelectedBox = Instantiate(SelectedBoxPrefab);
            SelectedBox.name = SelectedBox.name.Substring(0, SelectedBox.name.Length - 7); // (clone)�̕������폜
            SelectedBox.ChangeParent(hand1.transform.GetChild(MultiSelectIndex).GetChild(0), true, true, true);
            SingleSelectedTag = Tag.Hand;
            return true;
        }
        if (HasTag(Tag.Rearguard) && SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].FindWithChildTag(Tag.Card) != null)
        {
            SelectedBox = Instantiate(SelectedBoxPrefab);
            SelectedBox.name = SelectedBox.name.Substring(0, SelectedBox.name.Length - 7); // (clone)�̕������폜
            SelectedBox.ChangeParent(SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].transform, true, true, true);
            SingleSelectedTag = Tag.Rearguard;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 1�����I���\�ȃJ�[�\�����m�肵�A�I�𒆂������I�u�W�F�N�g���폜
    /// </summary>
    /// <param name="tag">����\�ȃ}�X</param>
    /// <returns>���ۂɌ��肵����</returns>
    public bool SingleConfirm(Tag tag)
    {
        if (!HasTag(tag)) return false;
        if (SingleSelectedTag == Tag.Hand && HasTag(Tag.Circle))
        {
            int selectedIndex = hand1.transform.FindWithGrandchildCard();
            StartCoroutine(CardManager.Instance.HandToField(hand1, SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].GetComponent<ICardCircle>(), selectedIndex));
            SingleSelectedTag = Tag.None;
            Destroy(SelectedBox);
            return true;
        }
        else if (SingleSelectedTag == Tag.Rearguard && HasTag(Tag.Rearguard))
        {
            List<Transform> fieldList = Field1.transform.FindWithAllChildTag(Tag.Rearguard);
            foreach (var rearguard in fieldList)
            {
                if(rearguard.FindWithChildTag(Tag.Card) != null)
                {
                    StartCoroutine(CardManager.Instance.RearToRear(rearguard.GetComponent<ICardCircle>(), SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].GetComponent<ICardCircle>()));
                    break;
                }
            }
            SingleSelectedTag = Tag.None;
            Destroy(SelectedBox);
            return true;
        }


        return false;

    }

    /// <summary>
    /// �I�𒆂̃J�[�\�����L�����Z��
    /// </summary>
    public void SingleCansel()
    {
        SingleSelectedTag = Tag.None;
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
    private bool HasTag(Tag tag) => SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].tag.Contains(tag.ToString());



}
