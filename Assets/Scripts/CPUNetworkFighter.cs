using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine.InputSystem;
using System.Reflection;
using System.Threading;

public class CPUNetworkFighter : MonoBehaviour, IFighter
{
    [SerializeField]
    private readonly PhotonController photonController;

    [field: SerializeField]
    public FighterID ID { get; private set; }

    [field: SerializeField]
    public int ActorNumber { get; set; }

    [field: SerializeField]
    private int Turn { get; set; } = 1;

    public IFighter OpponentFighter { get; set; }

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
        DeckGenerater.Instance.Generate(Deck, ActorNumber);
    }

    /// <summary>
    /// �t�@�[�X�g���@���K�[�h���Z�b�g����
    /// </summary>
    /// <returns></returns>
    public async UniTask SetFirstVanguard()
    {
        await CardManager.Instance.DeckToCircle(Deck, Vanguard, 0);
    }

    /// <summary>
    /// ��D�̃J�[�h����������i�}���K���j
    /// �}���K����̃V���b�t�������͖�����
    /// </summary>
    public async UniTask Mulligan()
    {
        print("�}���K���J�n");
        // �O���[�h1-3�܂�1���Âc��
        List<Card> cardList = Hand.cardList.Where(card => card.Grade == 0)
                                           .Union(Hand.cardList.Where(card => card.Grade == 1).Skip(1))
                                           .Union(Hand.cardList.Where(card => card.Grade == 2).Skip(1))
                                           .Union(Hand.cardList.Where(card => card.Grade == 3).Skip(1))
                                           .ToList();
        int count = cardList.Count;

        for (int i = 0; i < count; i++)
        {
            Card card = cardList.Pop();
            photonController.SendData("HandToDeck", card); // ������cardList�̗v�f�����ω����邽�ߒʏ��ForEach�͎g�p�s��
            await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
        }

        await DrawCard(count);
    }

    /// <summary>
    /// �J�[�h���f�b�L����w�薇������
    /// </summary>
    /// <param name="count">��������</param>
    /// <returns></returns>
    public async UniTask DrawCard(int count)
    {
        //Debug.Log("Draw����");


        //CardManager.DeckToHand(deck, hand);
        for (int i = 0; i < count; i++)
        {
            photonController.SendData("DeckToHand", card: Deck.GetCard(0));
            await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
            Debug.Log("true�ł�");
            //await CardManager.Instance.DeckToHand(Deck, Hand, 0);
        }

    }

    /// <summary>
    /// �X�^���h�A�b�v���@���K�[�h�I
    /// </summary>
    public async UniTask StandUpVanguard()
    {
        var card = Vanguard.Card;
        card.SetState(Card.StateType.FaceUp, true);
        await CardManager.Instance.RotateCard(Vanguard.Card);
    }

    /// <summary>
    /// �X�^���h�t�F�C�Y
    /// </summary>
    public async UniTask StandPhase(CancellationToken cancellationToken)
    {
        await UniTask.NextFrame();
        //await UniTask.WaitUntil(() => input.GetDown("Enter"));
        await CardManager.Instance.StandCard(Vanguard);
        Rearguards.ForEach(async rear => await CardManager.Instance.StandCard(rear));
    }

    /// <summary>
    /// �h���[�t�F�C�Y
    /// </summary>
    public async UniTask DrawPhase(CancellationToken cancellationToken)
    {
        //await UniTask.WaitUntil(() => input.GetDown("Enter"));
        await DrawCard(1);
    }

    /// <summary>
    /// ���C�h�t�F�C�Y
    /// </summary>
    public async UniTask RidePhase(CancellationToken cancellationToken)
    {
        // ���@���K�[�h�̃O���[�h+1�̃J�[�h�̒��Ńp���[���ő�̃J�[�h
        Card card = Hand.cardList.Where(card => card.Grade == Vanguard.Card.Grade + 1)
                                 .OrderByDescending(card => card.Power)
                                 .FirstOrDefault();
        if (card == null) return; // G�A�V�X�g�X�e�b�v�̎���

        Card removedCard = Vanguard.Card; // �J�[�h�����ɑ��݂���ꍇ�̓\�E���Ɉړ�������
        photonController.SendData("HandToVanguard", card.GetComponent<Card>());
        await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
        if (removedCard != null)
        {
            Debug.Log("�\�E���ɑ����");
            photonController.SendData("VanguardToSoul", removedCard);
            await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
        }

    }

    ///// <summary>
    ///// ���C���t�F�C�Y
    ///// </summary>
    public async UniTask<bool> MainPhase(CancellationToken cancellationToken)
    {
        await UniTask.NextFrame();
        Debug.Log("���C���J�n�I");

        if (Turn == 1) return false;

        return false;

    }

    ///// <summary>
    ///// �A�^�b�N�t�F�C�Y���̃A�^�b�N�X�e�b�v
    ///// </summary>
    //public async UniTask<(ICardCircle, ICardCircle)> AttackStep()
    //{
    //    await UniTask.NextFrame();
    //    print("�J�n");

    //    //if (Turn == 1) return true;

    //    Result result = Result.NONE;
    //    ICardCircle selectedAttackZone = null;
    //    ICardCircle selectedBoostZone = null;
    //    ICardCircle selectedTargetZone = null;

    //    int i = 0;

    //    List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsV = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsV2 = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsB = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsV3 = new List<Func<UniTask<Result>>>();

    //    var State = functionsV;

    //    functionsV.Add(async () =>
    //    {
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
    //        return resultInt.ToEnum("YES", "END");
    //    });
    //    functionsV.Add(async () =>
    //    {
    //        Card selectedAttackCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID); // ������̃T�[�N����I���������ǂ���
    //        if (selectedAttackCard == null) return Result.NO;
    //        selectedAttackZone = selectedAttackCard.GetComponentInParent<ICardCircle>();

    //        //if (!selectedAttackZone.Card.JudgeStep(Card.StateType.Stand)) return Result.NO; // �U���\�ȃJ�[�h������
    //        if (!selectedAttackZone.Front) return Result.NO;
    //        State = functionsV2;
    //        functions.AddRange(functionsV2);
    //        var result = SelectManager.Instance.NormalSelected(Tag.Circle, ID); // �U������J�[�h��I������
    //        if (result != null)
    //            return Result.YES;
    //        else
    //            return Result.NO;
    //    });


    //    functionsV2.Add(async () =>
    //    {
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
    //        return resultInt.ToEnum("YES", "CANCEL");
    //    });
    //    functionsV2.Add(async () =>
    //    {
    //        Card selectedBoostCard = await SelectManager.Instance.GetSelect(Tag.Rearguard, ID); // �u�[�X�g����
    //        if (selectedBoostCard != null)
    //        {
    //            selectedBoostZone = selectedBoostCard.GetComponentInParent<Rearguard>();
    //            if (!selectedBoostZone.Card.JudgeState(Card.StateType.Stand)) return Result.NO; // �u�[�X�g�\�ȃJ�[�h������
    //            if (!selectedBoostZone.IsSameColumn(selectedAttackZone)) return Result.NO;
    //            if (selectedBoostZone.Card.Ability != Card.AbilityType.Boost) return Result.NO;
    //            SelectManager.Instance.NormalSelected(Tag.Circle, ID); // �u�[�X�g����J�[�h��I������
    //            //selectedAttackZone.Card.BoostedPower = selectedBoostZone.Card.Power;
    //            photonController.SendGeneralData("Boost", new object[2] { selectedBoostZone.Name, selectedAttackZone.Name });
    //            await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
    //            ActionManager.Instance.ActionHistory.Add(new ActionData("Boost", ID, selectedBoostZone.Card, selectedBoostZone, selectedAttackZone));
    //            State = functionsB;
    //            functions.AddRange(functionsB);
    //            return Result.YES;
    //        }
    //        (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Circle, OpponentFighter.ID, Action.ATTACK); // ����ɍU������
    //        if (circle == null) return Result.NO;
    //        selectedTargetZone = circle;
    //        photonController.SendGeneralData("Attack", new object[2] { selectedAttackZone.Name, selectedTargetZone.Name });
    //        await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
    //        ActionManager.Instance.ActionHistory.Add(new ActionData("Attack", ID, selectedAttackZone.Card, selectedAttackZone, selectedTargetZone));
    //        State = functionsV3;
    //        functions.AddRange(functionsV3);
    //        return Result.YES;
    //    });

    //    functionsB.Add(async () =>
    //    {
    //        print("T");
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Cancel")));
    //        return resultInt.ToEnum("YES", "CANCEL");
    //    });
    //    functionsB.Add(async () => {
    //        (ICardCircle circle, Transform card) = await SelectManager.Instance.NormalConfirm(Tag.Circle, OpponentFighter.ID, Action.ATTACK); // ����ɍU������
    //        if (circle == null) return Result.NO;
    //        selectedTargetZone = circle;
    //        photonController.SendGeneralData("Attack", new object[2] { selectedAttackZone.Name, selectedTargetZone.Name });
    //        await UniTask.WaitUntil(() => NextController.Instance.JudgeProcessNext(ActorNumber));
    //        ActionManager.Instance.ActionHistory.Add(new ActionData("Attack", ID, selectedAttackZone.Card, selectedAttackZone, selectedTargetZone));
    //        State = functionsV3;
    //        functions.AddRange(functionsV3);
    //        return Result.YES;
    //    });

    //    functionsV3.Add(async () =>
    //    {
    //        print("V3");
    //        await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //        print("V3Enter");
    //        await CardManager.Instance.RestCard(selectedAttackZone);
    //        if (selectedBoostZone != null) await CardManager.Instance.RestCard(selectedBoostZone);
    //        //if (selectedAttackZone.V) await DriveTriggerCheck();
    //        return Result.YES;
    //    });

    //    functions.AddRange(functionsV);

    //    while (i < functions.Count)
    //    {
    //        await UniTask.NextFrame();
    //        print($"{ i}, { result}, { SelectManager.Instance.SelectedCount}, {functions.Count}");
    //        result = await functions[i]();
    //        switch (result)
    //        {
    //            case Result.YES:
    //                i++;
    //                break;
    //            case Result.NO:
    //                i--;
    //                break;
    //            case Result.CANCEL:
    //                functions.RemoveRange(functions.Count - State.Count, State.Count);
    //                i -= State.Count;
    //                State = functionsV;
    //                SelectManager.Instance.SingleCancel();
    //                break;
    //            case Result.END:
    //                return (null, null);
    //        }
    //    }

    //    return (selectedAttackZone, selectedTargetZone);
    //}

    ///// <summary>
    ///// �A�^�b�N�t�F�C�Y���̃K�[�h�X�e�b�v
    ///// </summary>
    //public async UniTask<bool> GuardStep()
    //{
    //    await UniTask.NextFrame();
    //    Result result = Result.NONE;
    //    int i = 0;
    //    Card selectedCard = null;

    //    List<Func<UniTask<Result>>> functionsSelect = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functionsSubmit = new List<Func<UniTask<Result>>>();
    //    List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

    //    var State = functions;


    //    functionsSelect.Add(async () =>
    //    {
    //        int resultInt = await UniTask.WhenAny(UniTask.WaitUntil(() => input.GetDown("Enter")), UniTask.WaitUntil(() => input.GetDown("Submit")));
    //        if (resultInt == 1)
    //        {
    //            State = functionsSubmit;
    //            functions.Pop();
    //            functions.AddRange(functionsSubmit);
    //        }
    //        return Result.YES;
    //    });

    //    functionsSelect.Add(async () =>
    //    {
    //        selectedCard = await SelectManager.Instance.GetSelect(Tag.Hand, ID);
    //        if (selectedCard != null)
    //        {
    //            SelectManager.Instance.NormalSelected(Tag.Hand, ID);
    //            functions.AddRange(functionsSelect);
    //            return Result.YES;
    //        }
    //        return Result.NO;
    //    });

    //    functionsSubmit.Add(async () =>
    //    {
    //        print(SelectManager.Instance.SelectedCount);
    //        await SelectManager.Instance.ForceConfirm(Tag.Guardian, ID, Action.MOVE);
    //        return Result.YES;
    //    });

    //    functionsSubmit.Add(async () =>
    //    {
    //        await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //        //await CardManager.Instance.GuardianToDrop(guardian, drop);
    //        return Result.YES;
    //    });

    //    functions.AddRange(functionsSelect);

    //    while (i < functions.Count)
    //    {
    //        await UniTask.NextFrame();
    //        result = await functions[i]();
    //        switch (result)
    //        {
    //            case Result.YES:
    //                i++;      // 1���ɐi��
    //                break;
    //            case Result.NO:
    //                i--;
    //                break;
    //            case Result.CANCEL:
    //                functions.RemoveRange(functions.Count - State.Count, State.Count);
    //                i -= State.Count;
    //                State = functions;
    //                SelectManager.Instance.SingleCancel();
    //                break;
    //            case Result.END:
    //                return false;
    //        }
    //    }

    //    print("owari");
    //    return true;
    //}

    ///// <summary>
    ///// �h���C�u�g���K�[�`�F�b�N
    ///// </summary>
    //public async UniTask DriveTriggerCheck(int count)
    //{
    //    for (int i = 0; i < count; i++)
    //    {
    //        await CardManager.Instance.DeckToDrive(Deck, Drive);
    //        if (Drive.GetCard().Trigger != Card.TriggerType.None)
    //            await GetTrigger(Drive.GetCard());
    //        await CardManager.Instance.DriveToHand(Drive, Hand);
    //    }

    //}

    ///// <summary>
    ///// �_���[�W�g���K�[�`�F�b�N
    ///// </summary>
    //public async UniTask DamageTriggerCheck(int count)
    //{
    //    for (int i = 0; i < count; i++)
    //    {
    //        await CardManager.Instance.DeckToDrive(Deck, Drive);
    //        await CardManager.Instance.DriveToDamage(Drive, Damage);
    //    }
    //}

    ///// <summary>
    ///// �g���K�[�擾
    ///// </summary>
    //public async UniTask GetTrigger(Card triggerCard)
    //{
    //    await UniTask.NextFrame();
    //    Result result = Result.NONE;
    //    int i = 0;
    //    Card selectedPowerUpCard = null;

    //    List<Func<UniTask<Result>>> functions = new List<Func<UniTask<Result>>>();

    //    switch (triggerCard.Trigger)
    //    {
    //        case Card.TriggerType.Critical:
    //            functions.Add(async () => {
    //                await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //                return Result.YES;
    //            });
    //            functions.Add(async () => {
    //                Card selectedCriticalUpCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID);
    //                if (selectedCriticalUpCard == null) return Result.NO;
    //                selectedCriticalUpCard.AddCritical(1);
    //                return Result.YES;
    //            });
    //            break;
    //        case Card.TriggerType.Draw:
    //            functions.Add(async () => {
    //                await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //                return Result.YES;
    //            });
    //            functions.Add(async () => {
    //                await DrawCard(1);
    //                return Result.YES;
    //            });
    //            break;
    //        case Card.TriggerType.Front:
    //            break;
    //        case Card.TriggerType.Heal:
    //            functions.Add(async () => {
    //                if (Damage.Count == 0) return Result.YES;
    //                if (Damage.Count < OpponentFighter.Damage.Count) return Result.YES;
    //                await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //                return Result.YES;
    //            });
    //            functions.Add(async () => {
    //                Card selectedDamageCard = await SelectManager.Instance.GetSelect(Tag.Damage, ID);
    //                if (selectedDamageCard == null) return Result.NO;
    //                await CardManager.Instance.DamageToDrop(Damage, Drop, selectedDamageCard);
    //                return Result.YES;
    //            });
    //            break;
    //        case Card.TriggerType.Stand:
    //            functions.Add(async () => {
    //                await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //                return Result.YES;
    //            });
    //            functions.Add(async () => {
    //                Card selectedStandCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID);
    //                if (selectedStandCard == null) return Result.NO;
    //                var selectedStandCircle = selectedStandCard.GetComponentInParent<ICardCircle>();
    //                if (selectedStandCircle.R == true) await CardManager.Instance.StandCard(selectedStandCircle);
    //                return Result.YES;
    //            });
    //            break;
    //        case Card.TriggerType.Over:
    //            break;
    //        default:
    //            break;
    //    }

    //    functions.Add(async () => {
    //        await UniTask.WaitUntil(() => input.GetDown("Enter"));
    //        return Result.YES;
    //    });
    //    //functions.Add(async () => await SelectManager.Instance.GetSelect(Tag.Hand, ID));
    //    functions.Add(async () => {
    //        selectedPowerUpCard = await SelectManager.Instance.GetSelect(Tag.Circle, ID);
    //        if (selectedPowerUpCard == null) return Result.NO;
    //        selectedPowerUpCard.AddPower(triggerCard.TriggerPower);
    //        return Result.YES;
    //    });

    //    while (i < functions.Count)
    //    {
    //        await UniTask.NextFrame();
    //        result = await functions[i]();
    //        switch (result)
    //        {
    //            case Result.YES:
    //                i++;    // 1���ɐi��
    //                break;
    //            case Result.NO:
    //                i -= 1; // 1�O�ɖ߂�
    //                break;
    //            case Result.CANCEL:
    //                SelectManager.Instance.SingleCancel();
    //                i = 0;  // �ŏ��ɖ߂�
    //                break;
    //            case Result.END:
    //                return; // �I������
    //        }
    //    }
    //}

    ///// <summary>
    ///// �A�^�b�N�t�F�C�Y���̃G���h�X�e�b�v
    ///// </summary>
    //public async UniTask EndStep()
    //{
    //    Vanguard.Card.BoostedPower = 0;
    //    Rearguards.Where(rear => rear.Card != null).Select(rear => rear.Card.BoostedPower = 0);
    //}

    ///// <summary>
    ///// �G���h�t�F�C�Y
    ///// </summary>
    //public async UniTask EndPhase(CancellationToken cancellationToken)
    //{
    //    await UniTask.NextFrame();

    //    Vanguard.Card.Reset();
    //    Rearguards.ForEach(rear => rear.Card?.Reset());

    //    Turn++;
    //}

    ///// <summary>
    ///// �w�肵���T�[�N���̃J�[�h��ދp������
    ///// </summary>
    ///// <param name="circle">�ދp������T�[�N��</param>

    //public async UniTask RetireCard(Card card)
    //{
    //    await CardManager.Instance.CardToDrop(Drop, circle.Pull());
    //}

    /// <summary>
    /// ���C���f�[�^�iCardManager�Ŏ��s����֐��̓��e�j����M�����Ƃ��̏���
    /// �J�[�h�Ɋւ��鏈���������Ă���
    /// </summary>
    /// <param name="args">���M��ID�A�֐����A�J�[�hID</param>
    public async UniTask ReceivedData(List<object> args)
    {
        int actorNumber = (int)args[0];
        string funcname = ((string)args[1]);
        int toIndex = funcname.IndexOf("To");
        string source = funcname.Substring(0, toIndex);
        string target = funcname.Substring(toIndex + 2, funcname.Length - toIndex - 2);
        //Debug.Log(ActorNumber);
        //Debug.Log(args[0]);
        Debug.Log($"{source} to {target}");

        //Debug.Log(CardDic[(int)args[2]]);

        funcname = funcname.Replace("Vanguard", "Circle");
        //Debug.Log(funcname);

        object sourceObj = null, targetObj = null;
        Type type = this.GetType();
        if (source.Contains("Rearguard"))
        {
            Debug.Log(source.Substring(source.Length - 2));
            var rearguard = Rearguards.Find(rear => rear.ID.ToString() == source.Substring(source.Length - 2));
            Debug.Log(rearguard);
            sourceObj = rearguard;
            funcname = "CircleTo" + funcname.Substring(funcname.IndexOf("To") + 2, funcname.Length - funcname.IndexOf("To") - 2);
        }
        if (target.Contains("Rearguard"))
        {
            Debug.Log(target.Substring(target.Length - 2));
            var rearguard = Rearguards.Find(rear => rear.ID.ToString() == target.Substring(target.Length - 2));
            Debug.Log(rearguard);
            targetObj = rearguard;
            funcname = funcname.Substring(0, funcname.IndexOf("To")) + "ToCircle";
        }

        //Debug.Log($"{source} to {target}");
        //Debug.Log(funcname);

        if (sourceObj == null) sourceObj = type?.GetProperty(source)?.GetValue(this);
        if (targetObj == null) targetObj = type?.GetProperty(target)?.GetValue(this);

        if (sourceObj == null || targetObj == null) Debug.LogError("Null�G���[");
        //Debug.Log($"{sourceObj} to {targetObj}");


        MethodInfo method = CardManager.Instance.GetType().GetMethod(funcname);
        if (method == null) Debug.LogError("Null�G���[");
        object[] args2 =
        {
            sourceObj,
            targetObj,
            CardDic[(int)args[2]],
        };
        await (UniTask)method.Invoke(CardManager.Instance, args2);

        print($"{actorNumber}�I�������");
        NextController.Instance.SetProcessNext(actorNumber, true);
        //foreach (var arg in args)
        //{
        //    Debug.Log(arg);
        //}
    }

    /// <summary>
    /// ���܂��܂ȏ����iAttack�Ȃǁj����M�������̏���
    /// </summary>
    /// <param name="args">���M��ID�A�������A�I�v�V����(Array)</param>
    /// <returns></returns>
    public async UniTask ReceivedGeneralData(List<object> args)
    {
        await UniTask.NextFrame();

        int actorNumber = (int)args[0];
        string type = ((string)args[1]);
        object[] options = (object[])args[2];
        if (type == "Attack")
        {
            print($"{options[0]} ���� {options[1]} �ɍU�������I");
        }
        else if (type == "Boost")
        {
            print($"{options[0]} �� {options[1]} ���u�[�X�g�����I");
        }

        print("�I�������");
        NextController.Instance.SetProcessNext(actorNumber, true);
    }

    /// <summary>
    /// State�f�[�^����M�����Ƃ��̏���
    /// �t�F�C�Y�̊Ǘ��Ɏg���\��
    /// </summary>
    /// <param name="state">�t�F�C�Y��</param>
    public void ReceivedState(string state)
    {
        TextManager.Instance.SetPhaseText(state);
    }

    public UniTask<(ICardCircle selectedAttackZone, ICardCircle selectedTargetZone)> AttackStep()
    {
        throw new NotImplementedException();
    }

    public UniTask<bool> GuardStep()
    {
        throw new NotImplementedException();
    }

    public UniTask DriveTriggerCheck(int checkCount)
    {
        throw new NotImplementedException();
    }

    public UniTask DamageTriggerCheck(int critical)
    {
        throw new NotImplementedException();
    }

    public UniTask EndStep()
    {
        throw new NotImplementedException();
    }

    public UniTask EndPhase(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public UniTask RetireCard(Card card)
    {
        throw new NotImplementedException();
    }
}