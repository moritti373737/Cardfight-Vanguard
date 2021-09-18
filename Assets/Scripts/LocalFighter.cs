using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using System.Reflection;

public class LocalFighter : MonoBehaviour, IFighter
{
    [SerializeField]
    private PlayerInput input;

    [field: SerializeField]
    public FighterID ID { get; private set; }

    [field: SerializeField]
    public int ActorNumber { get; set; }

    [field: SerializeField]
    private int Turn { get; set; } = 1;

    private IFighter OpponentFighter { get; set; }

    public GameObject Field { get; private set; }

    public Hand Hand { get; private set; }
    public Deck Deck { get; private set; }
    public Vanguard Vanguard { get; private set; }
    public List<Rearguard> Rearguards { get; private set; } = new List<Rearguard>();
    public Drop Drop { get; private set; }
    public Damage Damage { get; private set; }
    public Drive Drive { get; private set; }
    public Guardian Guardian { get; private set; }
    public Order Order { get; private set; }
    public Soul Soul { get; private set; }

    public Dictionary<int, Card> CardDic { get; set; }

    public ICardCircle SelectedAttackZone { get; set; } = null;
    public ICardCircle SelectedTargetZone { get; set; } = null;

    private void OnEnable()
    {
        //�q�I�u�W�F�N�g��S�Ď擾����
        //foreach (Transform childTransform in field.transform)
        //{
        //    if (childTransform.tag.Contains("Vanguard"))
        //        vanguard = childTransform.GetComponent<Vanguard>();
        //}
        Field = transform.FindWithChildTag(Tag.Field).gameObject;
        Hand = transform.FindWithChildTag(Tag.Hand).GetComponent<Hand>();
        Deck = Field.transform.FindWithChildTag(Tag.Deck).GetComponent<Deck>();
        Vanguard = Field.transform.FindWithChildTag(Tag.Vanguard).GetComponent<Vanguard>();
        Drop = Field.transform.FindWithChildTag(Tag.Drop).GetComponent<Drop>();
        Damage = Field.transform.FindWithChildTag(Tag.Damage).GetComponent<Damage>();
        Drive = Field.transform.FindWithChildTag(Tag.Drive).GetComponent<Drive>();
        Guardian = Field.transform.FindWithChildTag(Tag.Guardian).GetComponent<Guardian>();
        Order = Field.transform.FindWithChildTag(Tag.Order).GetComponent<Order>();
        Soul = Field.transform.FindWithChildTag(Tag.Soul).GetComponent<Soul>();

        List<Transform> rearguards = Field.transform.FindWithAllChildTag(Tag.Rearguard);
        foreach (var rear in rearguards)
        {
            Rearguards.Add(rear.GetComponent<Rearguard>());
        }

        OpponentFighter = GameObject.FindGameObjectsWithTag("Fighter")
                                    .Select(obj => obj.GetComponent<IFighter>())
                                    .First(fighter => fighter.ID != ID);
    }

    /// <summary>
    /// �f�b�L���쐬����
    /// </summary>
    public void CreateDeck()
    {
        DeckGenerater.Instance.Generate(Deck, ID == FighterID.ONE ? 0 : 1);
    }

    /// <summary>
    /// �t�@�[�X�g���@���K�[�h���Z�b�g����
    /// </summary>
    /// <returns></returns>
    public void SetFirstVanguard()
    {
        _ = CardManager.Instance.DeckToCircle(Deck, Vanguard, 0);
    }

    /// <summary>
    /// ��D�̃J�[�h����������i�}���K���j
    /// �}���K����̃V���b�t�������͖�����
    /// </summary>
    public async UniTask Mulligan()
    {
        while (true)
        {
            await UniTask.NextFrame();

            int inputIndex = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));

            if (inputIndex == 0) await SelectManager.Instance.NormalSelected(Tag.Hand, ID); // Enter���͎�
            else if (inputIndex == 1 && SelectManager.Instance.SelectedCount == 0) return;  // Submit���͎�
            else break;
        }

        int ToDeckCount = SelectManager.Instance.SelectedCount;

        await SelectManager.Instance.ForceConfirm(Tag.Deck, ID, Action.MOVE);

        await DrawCard(ToDeckCount);
    }

    /// <summary>
    /// �J�[�h���f�b�L����w�薇������
    /// </summary>
    /// <param name="count">��������</param>
    /// <returns></returns>
    public async UniTask DrawCard(int count)
    {
        //Debug.Log("Draw����");

        await UniTask.WaitUntil(() => input.GetDown("Enter"));

        //CardManager.DeckToHand(deck, hand);
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToHand(Deck, Hand, Deck.GetCard(0));
        }

    }

    /// <summary>
    /// �X�^���h�A�b�v���@���K�[�h�I
    /// </summary>
    public async UniTask StandUpVanguard()
    {
        var card = Vanguard.Card;
        card.SetState(Card.State.FaceUp, true);
        await CardManager.Instance.RotateCard(Vanguard);
    }

    /// <summary>
    /// �X�^���h�t�F�C�Y
    /// </summary>
    public async UniTask StandPhase()
    {
        await UniTask.NextFrame();
        await UniTask.WaitUntil(() => input.GetDown("Enter"));
        await CardManager.Instance.StandCard(Vanguard);
        Rearguards.ForEach(async rear => await CardManager.Instance.StandCard(rear));
    }

    /// <summary>
    /// �h���[�t�F�C�Y
    /// </summary>
    public async UniTask DrawPhase()
    {
        await UniTask.WaitUntil(() => input.GetDown("Enter"));
        await DrawCard(1);
    }

    /// <summary>
    /// ���C�h�t�F�C�Y
    /// </summary>
    public async UniTask RidePhase()
    {
        await UniTask.NextFrame();

        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

        functions.Add(async () => {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
            return resultInt.ToEnum("YES", "END");
        });
        functions.Add(async () => {
            Card selectedCard = await SelectManager.Instance.GetSelect(Tag.Hand, ID);
            if (selectedCard == null) return Result.NO;
            if (selectedCard.Grade > Vanguard.Card.Grade + 1) return Result.NO; // �O���[�h�̃`�F�b�N
            var result = await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
            if (result != null)
                return Result.YES;
            else
                return Result.NO;
        });
        functions.Add(async () => {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functions.Add(async () => {
            (ICardCircle vanguard, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Vanguard, ID, Action.MOVE);
            if (vanguard == null) return Result.NO;
            Card removedCard = vanguard.Card; // �J�[�h�����ɑ��݂���ꍇ�̓\�E���Ɉړ�������
            await CardManager.Instance.HandToCircle(Hand, vanguard, card.GetComponent<Card>());
            Debug.Log(removedCard);
            if (removedCard != null)
            {
                Debug.Log("�\�E���ɑ����");
                await CardManager.Instance.CircleToSoul(vanguard, Soul, removedCard);
            }
            return Result.YES;
        });

        int i = 0;
        Result result = Result.NONE;
        while (i < functions.Count)
        {
            await UniTask.NextFrame();
            result = await functions[i]();
            switch (result)
            {
                case Result.YES:
                    i++;    // 1���ɐi��
                    break;
                case Result.NO:
                    i -= 1; // 1�O�ɖ߂�
                    break;
                case Result.CANCEL:
                    SelectManager.Instance.SingleCancel();
                    i = 0;  // �ŏ��ɖ߂�
                    break;
                case Result.END:
                    return; // �I������
            }
        }

        print("owari");

    }

    /// <summary>
    /// ���C���t�F�C�Y
    /// </summary>
    public async UniTask<bool> MainPhase()
    {
        await UniTask.NextFrame();
        Card selectedCard = null;
        Debug.Log("���C���J�n�I");
        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV2 = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsC = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsM = new List<Func<UniTask<Result>>>();

        var state = functionsV;

        functionsV.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
            return resultInt.ToEnum("YES", "END");
        });
        functionsV.Add(async () =>
        {
            selectedCard = await SelectManager.Instance.GetSelect(Tag.None, ID);
            //ICardZone selectedZone = await SelectManager.Instance.GetZone(Tag.None, ID);
            if (selectedCard == null) return Result.NO;
            print(selectedCard);
            Transform action = TextManager.Instance.SetActionList(selectedCard);
            SelectManager.Instance.SetActionObj(action);
            state = functionsV2;
            functions.AddRange(functionsV2);
            return Result.YES;
        });

        functionsV2.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsV2.Add(async () =>
        {
            string act = SelectManager.Instance.ActionConfirm(ActorNumber);
            //return Result.YES;

            if (act == "Call")
            {
                selectedCard = await SelectManager.Instance.GetSelect(Tag.Hand, ID);
                if (selectedCard != null)
                {
                    //if (selectedCard.Grade > Vanguard.Card.Grade) return Result.RESTART; // �O���[�h�̃`�F�b�N
                    await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
                    state = functionsC;
                    functions.AddRange(functionsC);
                    return Result.YES;
                }

                selectedCard = await SelectManager.Instance.GetSelect(Tag.Rearguard, ID);
                if (selectedCard != null)
                {
                    await SelectManager.Instance.NormalSelected(Tag.Rearguard, ID);
                    state = functionsM;
                    functions.AddRange(functionsM);
                    return Result.YES;
                }
            }
            else if (act == "Skill")
            {
                print("�X�L������");
                await SkillManager.Instance.StartActivate(selectedCard, 0);
            }

            return Result.RESTART;

        });

        // �����܂ŋ��ʏ���

        // �������番��
        // ����1
        functionsC.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsC.Add(async () =>
        {
            (ICardCircle rearguard, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Rearguard, ID, Action.MOVE);
            if (rearguard == null) return Result.NO;
            Card removedCard = rearguard.Card; // �J�[�h�����ɑ��݂���ꍇ�͈ړ�������
            await CardManager.Instance.HandToCircle(Hand, rearguard, card.GetComponent<Card>());
            if (removedCard != null) await CardManager.Instance.CircleToDrop(rearguard, Drop, removedCard);
            return Result.YES;
        });

        // ����2
        functionsM.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsM.Add(async () =>
        {
            //Card targetCard = await SelectManager.Instance.GetSelect(Tag.Rearguard, ID);
            Rearguard targetZone = (Rearguard)await SelectManager.Instance.GetZone(Tag.Rearguard, ID);
            if (targetZone == null) return Result.NO;
            Rearguard selectedRearguard = selectedCard.transform.GetComponentInParent<Rearguard>();
            if (!selectedRearguard.IsSameColumn(targetZone)) return Result.NO;
            (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Rearguard, ID, Action.MOVE);
            await CardManager.Instance.CircleToCircle(selectedRearguard, (Rearguard)circle, card.GetComponent<Card>());
            return Result.YES;
        });

        functions.AddRange(functionsV);

        int i = 0;
        Result result = Result.NONE;
        while (i < functions.Count)
        {
            await UniTask.NextFrame();
            result = await functions[i]();
            switch (result)
            {
                case Result.YES:
                    i++;      // 1���ɐi��
                    break;
                case Result.NO:
                    i -= 1;   // 1�O�ɖ߂�
                    break;
                case Result.CANCEL:
                    functions.RemoveRange(functions.Count - state.Count, state.Count);
                    i -= state.Count; // ���O�̕���_�ɖ߂�
                    state = functionsV;
                    SelectManager.Instance.SingleCancel();
                    SelectManager.Instance.ActionConfirm(ActorNumber);
                    break;
                case Result.RESTART:
                    i = 0;
                    functions.Clear();
                    functions.AddRange(functionsV);
                    break;
                case Result.END:
                    return false;
            }
        }

        print("owari");
        return true;

     }

    /// <summary>
    /// �A�^�b�N�t�F�C�Y���̃A�^�b�N�X�e�b�v
    /// </summary>
    public async UniTask<(ICardCircle, ICardCircle)> AttackStep()
    {
        await UniTask.NextFrame();
        print("�J�n");

        //if (Turn == 1) return true;

        Result result = Result.NONE;
        ICardCircle selectedBoostZone = null;

        int i = 0;

        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV2 = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsB = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsV3 = new List<Func<UniTask<Result>>>();

        var state = functionsV;

        functionsV.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
            return resultInt.ToEnum("YES", "END");
        });
        functionsV.Add(async () =>
        {
            Card selectedAttackCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID); // ������̃T�[�N����I���������ǂ���
            if (selectedAttackCard == null) return Result.NO;
            SelectedAttackZone = selectedAttackCard.GetComponentInParent<ICardCircle>();

            //if (!selectedAttackZone.Card.JudgeStep(Card.State.Stand)) return Result.NO; // �U���\�ȃJ�[�h������
            if (!SelectedAttackZone.Front) return Result.NO;
            state = functionsV2;
            functions.AddRange(functionsV2);
            var result = await SelectManager.Instance.NormalSelected(Tag.Circle, ID); // �U������J�[�h��I������
                if (result != null)
                    return Result.YES;
                else
                    return Result.NO;
        });


        functionsV2.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsV2.Add(async () =>
        {
            Card selectedBoostCard = await SelectManager.Instance.GetSelect(Tag.Rearguard, ID); // �u�[�X�g����
            if (selectedBoostCard != null)
            {
                selectedBoostZone = selectedBoostCard.GetComponentInParent<Rearguard>();
                if (!selectedBoostZone.Card.JudgeState(Card.State.Stand)) return Result.NO; // �u�[�X�g�\�ȃJ�[�h������
                if (!selectedBoostZone.IsSameColumn(SelectedAttackZone)) return Result.NO;
                if (selectedBoostZone.Card.Ability != Card.AbilityType.Boost) return Result.NO;
                await SelectManager.Instance.NormalSelected(Tag.Circle, ID); // �u�[�X�g����J�[�h��I������
                SelectedAttackZone.Card.BoostedPower = selectedBoostZone.Card.Power;
                ActionManager.Instance.ActionHistory.Add(new ActionData("Boost", ID, selectedBoostZone.Card, selectedBoostZone, SelectedAttackZone));
                state = functionsB;
                functions.AddRange(functionsB);
                return Result.YES;
            }
            (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Circle, OpponentFighter.ID, Action.ATTACK); // ����ɍU������
            if (circle == null) return Result.NO;
            SelectedTargetZone = circle;
            ActionManager.Instance.ActionHistory.Add(new ActionData("Attack", ID, SelectedAttackZone.Card, SelectedAttackZone, SelectedTargetZone));
            state = functionsV3;
            functions.AddRange(functionsV3);
            return Result.YES;
        });

        functionsB.Add(async () =>
        {
            print("T");
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
            return resultInt.ToEnum("YES", "CANCEL");
        });
        functionsB.Add(async () => {
            (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Circle, OpponentFighter.ID, Action.ATTACK); // ����ɍU������
            if (circle == null) return Result.NO;
            SelectedTargetZone = circle;
            ActionManager.Instance.ActionHistory.Add(new ActionData("Attack", ID, SelectedAttackZone.Card, SelectedAttackZone, SelectedTargetZone));
            state = functionsV3;
            functions.AddRange(functionsV3);
            return Result.YES;
        });

        functionsV3.Add(async () =>
        {
            print("V3");
            await UniTask.WaitUntil(() => input.GetDown("Enter"));
            print("V3Enter");
            await CardManager.Instance.RestCard(SelectedAttackZone);
            if (selectedBoostZone != null) await CardManager.Instance.RestCard(selectedBoostZone);
            return Result.YES;
        });

        functions.AddRange(functionsV);

        while (i < functions.Count)
        {
            await UniTask.NextFrame();
            print($"{ i}, { result}, { SelectManager.Instance.SelectedCount}, {functions.Count}");
            result = await functions[i]();
            switch (result)
            {
                case Result.YES:
                    i++;
                    break;
                case Result.NO:
                    i--;
                    break;
                case Result.CANCEL:
                    functions.RemoveRange(functions.Count - state.Count, state.Count);
                    i -= state.Count;
                    state = functionsV;
                    SelectManager.Instance.SingleCancel();
                    break;
                case Result.END:
                    return (null, null);
            }
        }

        return (SelectedAttackZone, SelectedTargetZone);
    }

    /// <summary>
    /// �A�^�b�N�t�F�C�Y���̃K�[�h�X�e�b�v
    /// </summary>
    public async UniTask<bool> GuardStep()
    {
        await UniTask.NextFrame();
        Result result = Result.NONE;
        int i = 0;
        Card selectedCard = null;

        List<Func<UniTask<Result>>> functionsSelect = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functionsSubmit = new List<Func<UniTask<Result>>>();
        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

        var state = functions;


        functionsSelect.Add(async () =>
        {
            int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
            if (resultInt == 1)
            {
                state = functionsSubmit;
                functions.Pop();
                functions.AddRange(functionsSubmit);
            }
            return Result.YES;
        });

        functionsSelect.Add(async () =>
        {
            selectedCard = await SelectManager.Instance.GetSelect(Tag.Hand, ID);
            if (selectedCard != null)
            {
                await SelectManager.Instance.NormalSelected(Tag.Hand, ID);
                functions.AddRange(functionsSelect);
                return Result.YES;
            }
            return Result.NO;
        });

        functionsSubmit.Add(async () =>
        {
            await SelectManager.Instance.ForceConfirm(Tag.Guardian, ID, Action.MOVE);
            return Result.YES;
        });

        functionsSubmit.Add(async () =>
        {
            await UniTask.WaitUntil(() => input.GetDown("Enter"));
            //await CardManager.Instance.GuardianToDrop(guardian, drop);
            return Result.YES;
        });

        functions.AddRange(functionsSelect);

        while (i < functions.Count)
        {
            await UniTask.NextFrame();
            result = await functions[i]();
            switch (result)
            {
                case Result.YES:
                    i++;      // 1���ɐi��
                    break;
                case Result.NO:
                    i--;
                    break;
                case Result.CANCEL:
                    functions.RemoveRange(functions.Count - state.Count, state.Count);
                    i -= state.Count;
                    state = functions;
                    SelectManager.Instance.SingleCancel();
                    break;
                case Result.END:
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// �h���C�u�g���K�[�`�F�b�N
    /// </summary>
    public async UniTask DriveTriggerCheck(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToDrive(Deck, Drive, Deck.Pull(0));
            if (Drive.GetCard().Trigger != Card.TriggerType.None)
                await GetTrigger(Drive.GetCard());
            await CardManager.Instance.DriveToHand(Drive, Hand, Drive.Card);
        }

    }

    /// <summary>
    /// �_���[�W�g���K�[�`�F�b�N
    /// </summary>
    public async UniTask DamageTriggerCheck(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await CardManager.Instance.DeckToDrive(Deck, Drive, Deck.Pull(0));
            await CardManager.Instance.DriveToDamage(Drive, Damage, Drive.Card);
        }
    }

    /// <summary>
    /// �g���K�[�擾
    /// </summary>
    public async UniTask GetTrigger(Card triggerCard)
    {
        await UniTask.NextFrame();
        Result result = Result.NONE;
        int i = 0;
        Card selectedPowerUpCard = null;

        List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

        switch (triggerCard.Trigger)
        {
            case Card.TriggerType.Critical:
                functions.Add(async () => {
                    await UniTask.WaitUntil(() => input.GetDown("Enter"));
                    return Result.YES;
                });
                functions.Add(async () => {
                    Card selectedCriticalUpCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID);
                    if (selectedCriticalUpCard == null) return Result.NO;
                    selectedCriticalUpCard.AddCritical(1);
                    return Result.YES;
                });
                break;
            case Card.TriggerType.Draw:
                functions.Add(async () => {
                    await UniTask.WaitUntil(() => input.GetDown("Enter"));
                    return Result.YES;
                });
                functions.Add(async () => {
                    await DrawCard(1);
                    return Result.YES;
                });
                break;
            case Card.TriggerType.Front:
                break;
            case Card.TriggerType.Heal:
                functions.Add(async () => {
                    if (Damage.Count == 0) return Result.YES;
                    if (Damage.Count < OpponentFighter.Damage.Count) return Result.YES;
                    await UniTask.WaitUntil(() => input.GetDown("Enter"));
                    return Result.YES;
                });
                functions.Add(async () => {
                    Card selectedDamageCard = await SelectManager.Instance.GetSelect(Tag.Damage, ID);
                    if (selectedDamageCard == null) return Result.NO;
                    await CardManager.Instance.DamageToDrop(Damage, Drop, selectedDamageCard);
                    return Result.YES;
                });
                break;
            case Card.TriggerType.Stand:
                functions.Add(async () => {
                    await UniTask.WaitUntil(() => input.GetDown("Enter"));
                    return Result.YES;
                });
                functions.Add(async () => {
                    Card selectedStandCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID);
                    if (selectedStandCard == null) return Result.NO;
                    var selectedStandCircle = selectedStandCard.GetComponentInParent<ICardCircle>();
                    if (selectedStandCircle.R == true) await CardManager.Instance.StandCard(selectedStandCircle);
                    return Result.YES;
                });
                break;
            case Card.TriggerType.Over:
                break;
            default:
                break;
        }

        functions.Add(async () => {
            await UniTask.WaitUntil(() => input.GetDown("Enter"));
            return Result.YES;
        });
        //functions.Add(async () => await SelectManager.Instance.GetSelect(Tag.Hand, ID));
        functions.Add(async () => {
            selectedPowerUpCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID);
            if (selectedPowerUpCard == null) return Result.NO;
            selectedPowerUpCard.AddPower(triggerCard.TriggerPower);
            return Result.YES;
        });

        while (i < functions.Count)
        {
            await UniTask.NextFrame();
            result = await functions[i]();
            switch (result)
            {
                case Result.YES:
                    i++;    // 1���ɐi��
                    break;
                case Result.NO:
                    i -= 1; // 1�O�ɖ߂�
                    break;
                case Result.CANCEL:
                    SelectManager.Instance.SingleCancel();
                    i = 0;  // �ŏ��ɖ߂�
                    break;
                case Result.END:
                    return; // �I������
            }
        }
    }

    /// <summary>
    /// �A�^�b�N�t�F�C�Y���̃G���h�X�e�b�v
    /// </summary>
    public async UniTask EndStep()
    {
        Vanguard.Card.BoostedPower = 0;
        Rearguards.Where(rear => rear.Card != null).Select(rear => rear.Card.BoostedPower = 0);
    }

    /// <summary>
    /// �G���h�t�F�C�Y
    /// </summary>
    public async UniTask EndPhase()
    {
        await UniTask.NextFrame();

        Vanguard.Card.Reset();
        Rearguards.ForEach(rear => rear.Card?.Reset());

        Turn++;
    }

    /// <summary>
    /// �w�肵���T�[�N���̃J�[�h��ދp������
    /// </summary>
    /// <param name="circle">�ދp������T�[�N��</param>

    public async UniTask RetireCard(ICardCircle circle)
    {
        await CardManager.Instance.CardToDrop(Drop, circle.Pull());
    }

    /// <summary>
    /// ���C���f�[�^�iCardManager�Ŏ��s����֐��̓��e�j����M�����Ƃ��̏���
    /// �J�[�h�Ɋւ��鏈���������Ă���
    /// </summary>
    /// <param name="args">���M��ID�A�֐����A�J�[�hID</param>
    public async UniTask ReceivedData(List<object> args) { }

    /// <summary>
    /// ���܂��܂ȏ����iAttack�Ȃǁj����M�������̏���
    /// </summary>
    /// <param name="args">���M��ID�A�������A�I�v�V����(Array)</param>
    /// <returns></returns>
    public async UniTask ReceivedGeneralData(List<object> args) { }

    /// <summary>
    /// State�f�[�^����M�����Ƃ��̏���
    /// �t�F�C�Y�̊Ǘ��Ɏg���\��
    /// </summary>
    /// <param name="state">�t�F�C�Y��</param>
    public void ReceivedState(string state) { }

    private ICardCircle StringToCircle(string name) => name == "Vanguard" ? (ICardCircle)Vanguard : (ICardCircle)Rearguards.Find(rear => rear.Name == name);
}
