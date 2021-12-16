using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Reflection;
using UnityEngine.InputSystem;
using DG.Tweening;



/// <summary>
/// �J�[�\���I�����Ǘ�����
/// </summary>
public class SelectManager : SingletonMonoBehaviour<SelectManager>
{
    KeySelect keySelect;

    [SerializeField]
    private PlayerInput input;

    public GameObject SelectBox { get; private set; }   // �J�[�\��
    private readonly List<GameObject> SelectedBoxList = new List<GameObject>(); // �I�𒆂̃J�[�h���߂�J�[�\��

    private IFighter fighter1;
    private IFighter fighter2;

    [SerializeField]
    public GameObject SelectBoxPrefab;
    [SerializeField]
    public GameObject SelectedBoxPrefab;

    public int SelectedCount { get => SelectedCardList.Count; }

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
    /// ���ݒn�̃}�X�ځA�C���f�b�N�X
    /// </summary>
    private readonly List<int> selectZoneIndex = new List<int>() { 4, 2 };

    /// <summary>
    /// ��̃}�X�ړ��ňړ��\�ȏꍇ�p�̃C���f�b�N�X
    /// </summary>
    //[SerializeField]
    //private int MultiSelectIndex = 0;

    //private GameObject SelectObj { get => SelectObjList[selectZoneIndex[0]][selectZoneIndex[1]]; }
    public List<GameObject> SelectObjList1 = new List<GameObject>();
    public List<GameObject> SelectObjList2 = new List<GameObject>();
    public List<GameObject> SelectObjList3 = new List<GameObject>();

    private readonly List<Card> SelectedCardList = new List<Card>();

    private GameObject SelectActObj;

    void Start()
    {
        fighter1 = GameObject.Find("Fighter1").GetComponents<IFighter>().First(fighter => fighter.enabled);
        fighter2 = GameObject.Find("Fighter2").GetComponents<IFighter>().First(fighter => fighter.enabled);
        keySelect = new KeySelect(input, fighter1, fighter2);

        SelectBox = Instantiate(SelectBoxPrefab).FixName();
        SelectBox.ChangeParent(SelectObjList1[2].transform, true, true, true);

        ZoomImage = GameObject.Find("Canvas").transform.Find("ZoomCard").GetComponent<Image>();
        keySelect.ObserveEveryValueChanged(key => key.SelectTransform).Subscribe(_ => ChangeSelect().Forget());
        keySelect.ObserveEveryValueChanged(key => key.selectActionIndex).Where(i => i != -1).Subscribe(i => ChangeSelectBoxParent(SelectActObj.transform.GetChild(i).gameObject));
    }

    async UniTask ChangeSelect()
    {
        await AnimationManager.Instance.MoveSelectBox(SelectBox, keySelect.SelectTransform);
        ChangeSelectBoxParent(keySelect.SelectTransform.gameObject);
        SetZoomCard();
    }

    public Card GetSelectCard(Tag tag, FighterID fighterID)
    {
        if (tag == Tag.None || (HasTag(tag) && IsFighter(fighterID)))
        {
            return keySelect.SelectCard;
        }

        return null;
    }

    public ICardZone GetZone(Tag tag, FighterID fighterID)
    {
        // �J�[�\���ʒu���T�[�N�� ���� �J�[�\���ʒu���w�肵���t�@�C�^�[�̂���
        if (HasTag(tag) && IsFighter(fighterID))
        {
            return keySelect.SelectZone;
        }
        return null;
    }

    /// <summary>
    /// 1�����I���\�ȃJ�[�\���I�����A�I�𒆂������I�u�W�F�N�g��z�u
    /// </summary>
    /// <param name="tag">�I���\�ȃ}�X</param>
    /// <param name="fighterID">�I���\�ȃt�@�C�^�[ID</param>
    public void NormalSelected(Tag tag, FighterID fighterID)
    {
        //if (!HasTag(tag)) return null;
        //IFighter fighter = GetFighter(fighterID);
        //if (!IsFighter(fighterID)) return null;

        if (keySelect.SelectTransform.Find("SelectedBox"))
        {
            Cancel(keySelect.SelectCard);
            return;
        }
        var selectedBox = Instantiate(SelectedBoxPrefab).FixName();
        SelectedBoxList.Add(selectedBox);
        selectedBox.ChangeParent(keySelect.SelectTransform, true, true, true);
        selectedBox.transform.rotation = SelectBox.transform.rotation;
        //selectedBox.transform.LookAt(selectedBox.transform.localPosition.GetAddY(-1));
        //SelectedBox.transform.RotateAround(SelectedBox.transform.position, Vector3.forward, 180);
        SelectedCardList.Add(keySelect.SelectCard);
    }


    /// <summary>
    /// 1�����I���\�ȃJ�[�\�����m�肵�A�I�𒆂������I�u�W�F�N�g���폜
    /// </summary>
    /// <param name="tag">����\�ȃ}�X</param>
    /// <param name="fighterID">����\�ȃt�@�C�^�[ID</param>
    /// <param name="action">�I���A���肵���J�[�h�ɑ΂��Ƃ�s��</param>
    /// <returns></returns>
    public async UniTask<(ICardCircle, Transform)> NormalConfirm(Tag tag, FighterID fighterID, Action action)
    {
        await UniTask.NextFrame();

        if (!HasTag(tag)) return (null, null);

        IFighter fighter = GetFighter(fighterID);

        Card selectedCard = SelectedCardList[0];

        (ICardCircle Circle, Transform Card) result = (Circle: null, Card: null);

        if (action == Action.MOVE)
        {

            // �I�������J�[�\���ʒu����D ���� ���̃J�[�\���ʒu���T�[�N�� ���� ���̃J�[�\���ʒu���w�肵���t�@�C�^�[�̂���
            if (HasTag(Tag.Circle) && IsFighter(fighterID))
            {
                result = ((keySelect.SelectZone as ICardCircle), selectedCard.transform);
                goto END;
            }
            // �I�������J�[�\���ʒu�����A�K�[�h ���� ���̃J�[�\���ʒu�����A�K�[�h ���� ���̃J�[�\���ʒu���w�肵���t�@�C�^�[�̂��� �������c��ł���
            else if (HasTag(Tag.Rearguard) && IsFighter(fighterID))
            {
                result = ((keySelect.SelectZone as ICardCircle), selectedCard.transform);
                goto END;
            }
            else
            {
                return (null, null);
            }
        }
        else if (action == Action.ATTACK)
        {
            // �I�������J�[�\���ʒu���T�[�N�� ���� ���̃J�[�\���ʒu���T�[�N�� ���� ���̃J�[�\���ʒu���w�肵���t�@�C�^�[�̂��� ���� �w�肵���T�[�N���Ɋ��ɃJ�[�h�����݂���
            if (HasTag(Tag.Circle) && IsFighter(fighterID) && (keySelect.SelectZone as ISingleCardZone).Card != null)
            {
                //Debug.Log("V�ɃA�^�b�N");
                result = ((keySelect.SelectZone as ICardCircle), null);
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

        SelectedCardList.Clear();
        //foreach (var selectedBox in SelectedBoxList)
        //{
        //    Destroy(selectedBox);
        //}
        SelectedBoxList.ForEach(selectedBox => Destroy(selectedBox));
        SelectedBoxList.Clear();
        return result;

    }

    public async UniTask ForceConfirm(Tag tag, FighterID fighterID, Action action, Action<string, Card> endAction = null)
    {
        //var SelectedZip = SelectedBoxList.Zip(SelectedCardParentList, (box, parent) => (Box: box, Parent: parent));
        IFighter fighter = GetFighter(fighterID);
        //MultiSelectIndex = -1;
        SelectBox.transform.parent = null;

        if (tag == Tag.Deck)
        {
            SelectedCardList.ForEach(async card => {
                if(endAction == null) await CardManager.Instance.HandToDeck(fighter.Hand, fighter.Deck, card);
                else endAction("HandToDeck", card);
            });
        }
        else if (tag == Tag.Guardian)
        {
            SelectedCardList.ForEach(async card => {
                if (endAction == null) await CardManager.Instance.HandToGuardian(fighter.Hand, fighter.Guardian, card);
                else endAction("HandToGuardian", card);
            });
        }
        else if (tag == Tag.Drop)
        {
            SelectedCardList.ForEach(async card => await CardManager.Instance.HandToDrop(fighter.Hand, fighter.Drop, card));
        }
        else if (tag == Tag.Damage && action == Action.CounterBlast)
        {
            foreach (var card in SelectedCardList)
            {
                await CardManager.Instance.CounterBlast(card);
            }
        }

        SelectedBoxList.ForEach(selectedBox => Destroy(selectedBox));
        SelectedBoxList.Clear();
        SelectedCardList.Clear();

        await UniTask.NextFrame();

    }

    /// <summary>
    /// �Ō�ɑI�������I���ς݃J�[�\�����L�����Z��
    /// </summary>
    public void SingleCancel()
    {
        if (!SelectedCardList.Any()) return;
        SelectedCardList.Pop();
        Destroy(SelectedBoxList.Last());
        SelectedBoxList.Pop();
    }

    /// <summary>
    /// �w�肵���ʒu�ɂ���I���ς݃J�[�\�����L�����Z��
    /// </summary>
    /// <param name="card"></param>
    public void Cancel(Card cancelCard)
    {
        if (!SelectedCardList.Any()) return;
        int index = SelectedCardList.FindIndex(card => card == cancelCard);
        SelectedCardList.RemoveAt(index);
        Destroy(SelectedBoxList[index]);
        SelectedBoxList.RemoveAt(index);
    }


    /// <summary>
    /// �J�[�h�̊g��@�\�̃I���I�t��؂�ւ���
    /// </summary>
    public void ZoomCard()
    {
        ZoomImage.enabled = !ZoomImage.enabled;
        SetZoomCard();
    }

    /// <summary>
    /// �J�[�h�̊g��@�\�ŕ\������J�[�h�̐ݒ���s��
    /// </summary>
    private void SetZoomCard()
    {
        if (!ZoomImage.enabled) return;
        if (HasTag(Tag.Deck)) return;

        var card = keySelect.SelectCard;
        if (card is null) return;
        Texture2D cardTexture = card.GetTexture() as Texture2D;
        ZoomImage.sprite = Sprite.Create(cardTexture, new Rect(0.0f, 0.0f, cardTexture.width, cardTexture.height), new Vector2(0.5f, 0.5f), 100.0f);

    }

    /// <summary>
    /// ��D���ł̃J�[�\���̈ʒu����ԉE�i�Ō�Ɉ������J�[�h�j�ɂ���
    /// </summary>
    /// <param name="hand"></param>
    //private void ChangeHandCount(Hand hand)
    //{
    //    //Hand hand = fighter.hand;
    //    MultiSelectIndex = hand.cardList.Count -1;

    //    if (hand.Count > 0) // ��D�̃J�[�h�ɃJ�[�\�����ړ�������
    //    {
    //        SelectBox.ChangeParent(hand.transform.GetChild(MultiSelectIndex), p: true);
    //    }
    //    else // ��D���Ȃ��Ƃ�
    //        SelectBox.ChangeParent(SelectObj.transform, p: true);

    //    //if (hand.cardList.Count <= MultiSelectIndex)
    //    //{
    //    //    MultiSelectIndex = hand.cardList.Count - 1;
    //    //    if (MultiSelectIndex < 0) MultiSelectIndex = 0;
    //    //}
    //}

    public void SetActionCard(Card card)
    {
        Transform action = TextManager.Instance.SetActionList(card);
        int count = action.childCount;
        SelectActObj = action.gameObject;
        ChangeSelectBoxParent(action.GetChild(0).gameObject);
        keySelect.SetActionList(action);
    }

    public string ActionConfirm(int actorNumber)
    {
        string selectedAction = SelectBox.transform?.parent.name;
        if (IsSingle(keySelect.SelectZone))
            SelectBox.ChangeParent(keySelect.SelectZone.transform);
        else
            ChangeSelectBoxParent(keySelect.SelectTransform.gameObject);
        CancelAction();
        keySelect.DeleteActionList();
        return selectedAction;
    }

    public void CancelAction()
    {
        Destroy(SelectActObj);
        SelectActObj = null;
    }

    /// <summary>
    /// ���݂̃J�[�\���ʒu�ɂ���I�u�W�F�N�g�̃^�O�Ǝw�肵���^�O��������v���Ă��邩���ׂ�
    /// </summary>
    /// <param name="tag">����Ɏg���^�O</param>
    /// <returns>������v�������ǂ���</returns>
    private bool HasTag(Tag tag) => keySelect.SelectZone.transform.tag.Contains(tag.ToString());
    //private bool IsAction() => SelectObj?.FindWithChildTag(Tag.Card)?.FindWithChildTag(Tag.Action);

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
    private IFighter GetFighter() => keySelect.SelectZone.transform.root.GetComponents<IFighter>().First(fighter => fighter.enabled);

    /// <summary>
    /// �w�肵���t�@�C�^�[ID�����t�@�C�^�[��Ԃ�
    /// </summary>
    /// <param name="fighterID">����Ɏg���t�@�C�^�[ID</param>
    /// <returns>�����𖞂����t�@�C�^�[</returns>
    private IFighter GetFighter(FighterID fighterID) => fighter1.ID == fighterID ? fighter1 : fighter2;

    private bool IsSingle(ICardZone cardZone) => cardZone is ISingleCardZone;

    private void ChangeSelectBoxParent(GameObject parentObject)
    {
        Transform parentTransform = parentObject.transform;
        SelectBox.transform.SetParent(parentTransform);
        //SelectBox.transform.position = parentTransform.position;
        SelectBox.transform.localPosition = new Vector3(0, 0, 0);
        SelectBox.transform.localScale = new Vector3(1.1F, 1.1F, 1.1F);
    }

}


class KeySelect
{
    private PlayerInput input;

    private IFighter fighter1;
    private IFighter fighter2;

    private List<List<ICardZone>> SelectZoneList;
    private List<Dictionary<string, int>> IndexList;
    private readonly List<int> selectZoneIndex = new List<int>() { 4, 2 };
    public int selectActionIndex = -1;
    private Transform selectActionTransform = null;

    public bool IsUpDown = true;
    public bool IsLeftRight = true;

    public ICardZone SelectZone => SelectZoneList[selectZoneIndex[0]][selectZoneIndex[1]];

    public Card SelectCard
    {
        get
        {
            if (IsSingle()) return (SelectZone as ISingleCardZone).Card;
            else
            {
                IMultiCardZone multiZone = SelectZone as IMultiCardZone;
                if (multiZone.HasCard())
                    return (SelectZone as IMultiCardZone).GetCard(IndexList[GetFighter().ActorNumber][SelectZone.GetType().Name]);
                else return null;
            }
        }
    }

    public Transform SelectTransform
    {
        get
        {
            if (IsSingle()) return SelectZone.transform;
            else
            {
                IMultiCardZone multiZone = SelectZone as IMultiCardZone;
                if (multiZone.HasCard())
                {
                    if (multiZone.Count <= IndexList[GetFighter().ActorNumber][SelectZone.GetType().Name])
                        IndexList[GetFighter().ActorNumber][SelectZone.GetType().Name] = multiZone.Count - 1;
                    else if (0 > IndexList[GetFighter().ActorNumber][SelectZone.GetType().Name]) IndexList[GetFighter().ActorNumber][SelectZone.GetType().Name] = 0;
                    return (SelectZone as IMultiCardZone).GetCard(IndexList[GetFighter().ActorNumber][SelectZone.GetType().Name]).transform;

                }
                else return SelectZone.transform;
            }
        }
    }

    private enum DirectType
    {
        Right,
        Left,
        Up,
        Down,
    }

    public KeySelect(PlayerInput input, IFighter fighter1, IFighter fighter2)
    {
        this.input = input;
        this.fighter1 = fighter1;
        this.fighter2 = fighter2;

        SelectZoneList = new List<List<ICardZone>>()
        {
            new List<ICardZone>()
            {
                null,
                null,
                fighter2.Hand,
                null,
                null,
            },
            new List<ICardZone>()
            {
                fighter2.Drop,
                fighter2.Rearguards.Find(rear => rear.ID == 23),
                fighter2.Rearguards.Find(rear => rear.ID == 22),
                fighter2.Rearguards.Find(rear => rear.ID == 21),
                fighter2.Damage,
            },
            new List<ICardZone>()
            {
                fighter2.Deck,
                fighter2.Rearguards.Find(rear => rear.ID == 13),
                fighter2.Vanguard,
                fighter2.Rearguards.Find(rear => rear.ID == 11),
                fighter2.Order,
            },
            //new List<ICardZone>()
            //{
            //    fighter2.guardian,
            //    fighter2.guardian,
            //    fighter2.guardian,
            //    fighter2.guardian,
            //    fighter2.guardian,
            //},
            new List<ICardZone>()
            {
                null,
                null,
                fighter1.Guardian,
                null,
                null,
            },
            new List<ICardZone>()
            {
                fighter1.Order,
                fighter1.Rearguards.Find(rear => rear.ID == 11),
                fighter1.Vanguard,
                fighter1.Rearguards.Find(rear => rear.ID == 13),
                fighter1.Deck,
            },
            new List<ICardZone>()
            {
                fighter1.Damage,
                fighter1.Rearguards.Find(rear => rear.ID == 21),
                fighter1.Rearguards.Find(rear => rear.ID == 22),
                fighter1.Rearguards.Find(rear => rear.ID == 23),
                fighter1.Drop,
            },
            new List<ICardZone>()
            {
                null,
                null,
                fighter1.Hand,
                null,
                null,
            },
        };

        IndexList = new List<Dictionary<string, int>>()
        {
            new Dictionary<string, int>()
            {
                { "Hand", 0},
                { "Guardian", 0},
                { "Damage", 0},
            },
            new Dictionary<string, int>()
            {
                { "Hand", 0},
                { "Guardian", 0},
                { "Damage", 0},
            },
        };

        Observable.EveryUpdate()
                  .Where(_ => input.GetDown("Right"))
                  .Where(_ => IsLeftRight)
                  .Subscribe(_ => Move(DirectType.Right));
        Observable.EveryUpdate()
                  .Where(_ => input.GetDown("Left"))
                  .Where(_ => IsLeftRight)
                  .Subscribe(_ => Move(DirectType.Left));
        Observable.EveryUpdate()
                  .Where(_ => input.GetDown("Up"))
                  .Subscribe(_ => (IsUpDown ? (Action<DirectType>)Move : MoveSameCircle)(DirectType.Up)); // 3�����Z�q
        Observable.EveryUpdate()
                  .Where(_ => input.GetDown("Down"))
                  .Subscribe(_ => (IsUpDown ? (Action<DirectType>)Move : MoveSameCircle)(DirectType.Down));

        Observable.EveryUpdate()
                  .Where(_ => input.actions["Right"].ReadValue<float>() > 0)
                  .Where(_ => !IsLeftRight)
                  .ThrottleFirst(TimeSpan.FromSeconds(0.1))
                  .Subscribe(_ => ScrollManager.Instance.Scroll(ScrollManager.ScrollDirectType.Right));
        Observable.EveryUpdate()
                  .Where(_ => input.actions["Left"].ReadValue<float>() > 0)
                  .Where(_ => !IsLeftRight)
                  .ThrottleFirst(TimeSpan.FromSeconds(0.1))
                  .Subscribe(_ => ScrollManager.Instance.Scroll(ScrollManager.ScrollDirectType.Left));

        Observable.EveryUpdate().Where(_ => input.GetDown("Check")).Subscribe(_ => ToggleCheckList());

    }
    private void Move(DirectType direct)
    {
        ICardZone preSelectZone = SelectZone;

        switch (direct)
        {
            case DirectType.Right:
                selectZoneIndex[1]++;
                break;
            case DirectType.Left:
                selectZoneIndex[1]--;
                break;
            case DirectType.Up:
                selectZoneIndex[0]--;
                break;
            case DirectType.Down:
                selectZoneIndex[0]++;
                break;
        }

        if (0 > selectZoneIndex[1])
            selectZoneIndex[1] = 0;

        if (0 > selectZoneIndex[0])
            selectZoneIndex[0] = 0;

        if (SelectZoneList.Count <= selectZoneIndex[0])
            selectZoneIndex[0] = SelectZoneList.Count - 1;

        if (SelectZoneList[selectZoneIndex[0]].Count <= selectZoneIndex[1])
            selectZoneIndex[1] = SelectZoneList[selectZoneIndex[0]].Count - 1;

        if (SelectZone is null) selectZoneIndex[1] = SelectZoneList[selectZoneIndex[0]].FindIndex(zone => !(zone is null));
        //Debug.Log($"{selectZoneIndex[0]}, {selectZoneIndex[1]}");

        if (SelectZone is Hand)
        {
            SelectZone.transform.DOLocalMoveZ(-0.4F, 0.15f);
        }
        else if(preSelectZone is Hand && !(SelectZone is Hand))
        {
            preSelectZone.transform.DOLocalMoveZ(-0.5F, 0.15F);
        }

        switch (direct)
        {
            case DirectType.Right:
                if (SelectZone is Hand || SelectZone is Guardian)
                    if ((SelectZone as IMultiCardZone).Count - 1 > IndexList[GetFighter().ActorNumber][SelectZone.GetType().Name])
                        IndexList[GetFighter().ActorNumber][SelectZone.GetType().Name]++;
                break;
            case DirectType.Left:
                if (SelectZone is Hand || SelectZone is Guardian)
                    if (0 < IndexList[GetFighter().ActorNumber][SelectZone.GetType().Name])
                        IndexList[GetFighter().ActorNumber][SelectZone.GetType().Name]--;
                break;
            case DirectType.Up:
                break;
            case DirectType.Down:
                break;
        }

        IsUpDown = SelectZone as Damage ? false : true;

    }

    private void MoveSameCircle(DirectType direct)
    {
        switch (direct)
        {
            case DirectType.Up:
                if (SelectZone is Damage)
                {
                    IndexList[GetFighter().ActorNumber]["Damage"]--;
                    if (IndexList[GetFighter().ActorNumber]["Damage"] < 0)
                    {
                        IndexList[GetFighter().ActorNumber]["Damage"] = 0;
                        Move(direct);
                    }
                }
                else if(selectActionIndex > 0)
                {
                    selectActionIndex--;
                }
                break;
            case DirectType.Down:
                if (SelectZone is Damage)
                {
                    IndexList[GetFighter().ActorNumber]["Damage"]++;
                    if (IndexList[GetFighter().ActorNumber]["Damage"] >= (SelectZone as IMultiCardZone).Count)
                    {
                        IndexList[GetFighter().ActorNumber]["Damage"] = (SelectZone as IMultiCardZone).Count - 1;
                        Move(direct);
                    }
                }
                else if (selectActionIndex != -1 && selectActionTransform.childCount - 1 > selectActionIndex)
                {
                    selectActionIndex++;
                }
                break;
            default:
                break;
        }
    }

    public void SetActionList(Transform action)
    {
        selectActionTransform = action;
        selectActionIndex = 0;
        IsUpDown = false;
        IsLeftRight = false;
    }

    public void DeleteActionList()
    {
        selectActionTransform = null;
        selectActionIndex = -1;
        IsUpDown = true;
        IsLeftRight = true;
    }
    public void ToggleCheckList()
    {
        if (IsUpDown && IsLeftRight)
        {
            if (SelectZone is Deck)
            {
                ScrollManager.Instance.SetCheckList((SelectZone as Deck).cardList);
                IsUpDown = false;
                IsLeftRight = false;
            }
        }
        else if (!IsUpDown && !IsLeftRight)
        {
            ScrollManager.Instance.DeleteCheckList();
            IsUpDown = true;
            IsLeftRight = true;
        }
    }

    bool IsSingle() => !(SelectZone is Hand || SelectZone is Guardian || SelectZone is Damage);
    IFighter GetFighter() => SelectZone.transform.root.GetComponents<IFighter>().First(fighter => fighter.enabled);
}