using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IFighter
{
    bool enabled { get; set; }

    public FighterID ID { get; }

    public int ActorNumber { get; set; }
    public IFighter OpponentFighter { get; set; }
    public GameObject Field { get; }

    public Hand Hand { get; }
    public Deck Deck { get; }
    public Vanguard Vanguard { get; }
    public List<Rearguard> Rearguards { get; }
    public Drop Drop { get; }
    public Damage Damage { get; }
    public Drive Drive { get; }
    public Guardian Guardian { get; }
    public Order Order { get; }
    public Soul Soul { get; }

    public Dictionary<int, Card> CardDic { get; set; }

    public ICardCircle SelectedAttackZone { get; set; }
    public ICardCircle SelectedTargetZone { get; set; }
    void CreateDeck();
    UniTask SetFirstVanguard();
    UniTask Mulligan();
    UniTask DrawCard(int count);
    UniTask StandUpVanguard();
    UniTask StandPhase(CancellationToken cancellationToken);
    UniTask DrawPhase(CancellationToken cancellationToken);
    UniTask RidePhase(CancellationToken cancellationToken);
    UniTask<bool> MainPhase(CancellationToken cancellationToken);
    UniTask<(ICardCircle selectedAttackZone, ICardCircle selectedTargetZone)> AttackStep();
    UniTask<bool> GuardStep();
    UniTask DriveTriggerCheck(int checkCount);
    UniTask DamageTriggerCheck(int critical);
    UniTask EndStep();
    UniTask EndPhase(CancellationToken cancellationToken);
    UniTask RetireCard(Card card);
    UniTask ReceivedData(List<object> list);
    UniTask ReceivedGeneralData(List<object> list);
    void ReceivedState(string state);
}
