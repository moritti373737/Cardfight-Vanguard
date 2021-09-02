using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

/// <summary>
/// �J�[�\���I�����Ǘ�����
/// </summary>
public class SelectManager : SingletonMonoBehaviour<SelectManager>
{
    public GameObject SelectBox;   // �J�[�\��
    private GameObject SelectedBox; // �I�𒆂̃J�[�h���߂�J�[�\��
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
            // �E�[�ɂ��Ȃ� ���� ��D�ɃJ�[�\�����Ȃ��Ƃ�
            selectZoneIndex[1]++;
            changeSelectBox = true;

        }
        else if (left && selectZoneIndex[1] > 0 && !IsHand())
        {
            // ���[�ɂ��Ȃ� ���� ��D�ɃJ�[�\�����Ȃ��Ƃ�
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

        // �G���A�ړ������Ƃ�
        if (changeSelectBox)
        {
            Fighter fighter = GetFighter();
            Transform hand = fighter.transform.FindWithChildTag(Tag.Hand);

            // ��D�ȊO�̏ꏊ�����D�Ɉړ������Ƃ�
            if (IsHand() && hand.CountWithChildTag(Tag.EmptyCard) > 0)
            {
                MultiSelectIndex = hand.CountWithChildTag(Tag.EmptyCard) / 2; // ��D�̐��ɍ��킹�ď�����
                SelectBox.ChangeParent(hand.GetChild(MultiSelectIndex), p: true);
            }
            else
                SelectBox.ChangeParent(SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].transform, p: true);

            // �q�I�u�W�F�N�g��S�Ď擾����
            //foreach (Transform childTransform in hand.transform)
            //{
            //    Debug.Log(childTransform.gameObject.name);
            //}
        }
        // �J�[�\������D��ɂ��鎞
        else if (IsHand())
        {
            Fighter fighter = GetFighter();
            Transform hand = fighter.transform.FindWithChildTag(Tag.Hand);

            // �J�[�\�������E�Ɉړ�������
            if (right && hand.childCount - 1 > MultiSelectIndex)
            {
                MultiSelectIndex++;
            }
            else if (left && MultiSelectIndex > 0)
            {
                MultiSelectIndex--;
            }

            if (hand.CountWithChildTag(Tag.EmptyCard) > 0) // ��D�̃J�[�h�ɃJ�[�\�����ړ�������
                SelectBox.ChangeParent(hand.GetChild(MultiSelectIndex), p: true);
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
    /// <param name="fighterID">�I���\�ȃt�@�C�^�[ID</param>
    /// <returns>���ۂɑI��������</returns>
    public bool SingleSelected(Tag tag, FighterID fighterID)
    {
        Fighter fighter = GetFighter(fighterID);

        if (!HasTag(tag)) return false;

        SelectedBox = Instantiate(SelectedBoxPrefab).FixName();

        // �J�[�\���ʒu����D ���� �J�[�\���ʒu���w�肵���t�@�C�^�[�̂��� ���� �w�肵���t�@�C�^�[�̎�D��0������Ȃ�
        if (HasTag(Tag.Hand) && IsFighter(fighterID) && fighter.transform.FindWithChildTag(Tag.Hand).CountWithChildTag(Tag.EmptyCard) > 0)
        {
            var selectedCard = fighter.transform.FindWithChildTag(Tag.Hand).GetChild(MultiSelectIndex);
            SelectedBox.ChangeParent(selectedCard, true, true, true);
            //SelectedBox.transform.RotateAround(SelectedBox.transform.position, Vector3.forward, 180);
            SelectedCardParentList.Add(selectedCard);
        }
        // �J�[�\���ʒu�����A�K�[�h ���� �J�[�\���ʒu���w�肵���t�@�C�^�[�̂��� ���� �w�肵�����A�K�[�h�Ɋ��ɃJ�[�h�����݂���
        else if (HasTag(Tag.Rearguard) && IsFighter(fighterID) && SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].FindWithChildTag(Tag.Card) != null)
        {
            var selectedRearguard = SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]];
            SelectedBox.ChangeParent(selectedRearguard.transform, true, true, true);
            SelectedCardParentList.Add(selectedRearguard.transform);
        }
        // �J�[�\���ʒu���T�[�N�� ���� �J�[�\���ʒu���w�肵���t�@�C�^�[�̂��� ���� �w�肵���T�[�N���Ɋ��ɃJ�[�h�����݂���
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
    /// 1�����I���\�ȃJ�[�\�����m�肵�A�I�𒆂������I�u�W�F�N�g���폜
    /// </summary>
    /// <param name="tag">����\�ȃ}�X</param>
    /// <param name="fighterID">����\�ȃt�@�C�^�[ID</param>
    /// <param name="action">�I���A���肵���J�[�h�ɑ΂��Ƃ�s��</param>
    /// <returns>���ۂɌ��肵����</returns>
    public bool SingleConfirm(Tag tag, FighterID fighterID, Action action)
    {
        if (!HasTag(tag)) return false;

        Fighter fighter = GetFighter(fighterID);


        foreach (var selected in SelectedCardParentList)
        {
            if (action == Action.MOVE)
            {

                // �I�������J�[�\���ʒu����D ���� ���̃J�[�\���ʒu���T�[�N�� ���� ���̃J�[�\���ʒu���w�肵���t�@�C�^�[�̂���
                if (selected.parent.ExistTag(Tag.Hand) && HasTag(Tag.Circle) && IsFighter(fighterID))
                {
                    Debug.Log(fighter.transform.FindWithChildTag(Tag.Hand).GetComponent<Hand>());
                    Debug.Log(SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].GetComponent<ICardCircle>());
                    Debug.Log(selected.FindWithChildTag(Tag.Card).GetComponent<Card>());
                    StartCoroutine(CardManager.Instance.HandToField(fighter.transform.FindWithChildTag(Tag.Hand).GetComponent<Hand>(), SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].GetComponent<ICardCircle>(), selected.FindWithChildTag(Tag.Card).GetComponent<Card>()));
                }
                // �I�������J�[�\���ʒu�����A�K�[�h ���� ���̃J�[�\���ʒu�����A�K�[�h ���� ���̃J�[�\���ʒu���w�肵���t�@�C�^�[�̂��� �������c��ł���
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
                // �I�������J�[�\���ʒu���T�[�N�� ���� ���̃J�[�\���ʒu���T�[�N�� ���� ���̃J�[�\���ʒu���w�肵���t�@�C�^�[�̂���
                if (selected.ExistTag(Tag.Circle) && HasTag(Tag.Vanguard) && IsFighter(fighterID))
                {
                    Debug.Log("V�ɃA�^�b�N");
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
    /// �I�𒆂̃J�[�\�����L�����Z��
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
    /// ���݂̃J�[�\���ʒu�ɂ���I�u�W�F�N�g�̃^�O�Ǝw�肵���^�O��������v���Ă��邩���ׂ�
    /// </summary>
    /// <param name="tag">����Ɏg���^�O</param>
    /// <returns>������v�������ǂ���</returns>
    private bool HasTag(Tag tag) => SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].tag.Contains(tag.ToString());

    /// <summary>
    /// ���݂̃J�[�\���ʒu�ɂ���I�u�W�F�N�g�����L����t�@�C�^�[�Ǝw�肵���t�@�C�^�[����v���Ă��邩���ׂ�
    /// </summary>
    /// <param name="fighterID">����Ɏg���t�@�C�^�[ID</param>
    /// <returns>��v�������ǂ���</returns>
    private bool IsFighter(FighterID fighterID) => GetFighter().ID == fighterID;

    /// <summary>
    /// ���݂̃J�[�\���ʒu�ɂ���I�u�W�F�N�g�����L����t�@�C�^�[��Ԃ�
    /// </summary>
    /// <returns>�����𖞂����t�@�C�^�[</returns>
    private Fighter GetFighter() => SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]].transform.root.GetComponent<Fighter>();

    /// <summary>
    /// �w�肵���t�@�C�^�[ID�����t�@�C�^�[��Ԃ�
    /// </summary>
    /// <param name="fighterID">����Ɏg���t�@�C�^�[ID</param>
    /// <returns>�����𖞂����t�@�C�^�[</returns>
    private Fighter GetFighter(FighterID fighterID) => fighter1.ID == fighterID ? fighter1 : fighter2;

}
