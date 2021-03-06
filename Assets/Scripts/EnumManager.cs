using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum FightMode
{
    PVP,
    PVE,
    EVE,
}

public enum Tag
{
    None,
    Card,
    Vanguard,
    Rearguard,
    Deck,
    Hand,
    Soul,
    Circle,
    Drop,
    Damage,
    Order,
    Guardian,
    Drive,
    Field,
    Power,
    StatusText,
    Action
}

public enum FighterID
{
    ONE = 0,
    TWO = 1,
}

public enum Action
{
    None,
    MOVE,
    ATTACK,
    GUARD,
    CALL,
    CounterBlast,
}

public enum Result
{
    YES,
    NO,
    CANCEL,
    END,
    RESTART,
    NONE,
}

public class EnumManager
{

}
