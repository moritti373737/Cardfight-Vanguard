using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}

public enum FighterID
{
    ONE,
    TWO,
}

public enum Action
{
    None,
    MOVE,
    ATTACK,
    GUARD,
    CALL,
}

public enum Result
{
    YES,
    NO,
    CANCEL,
    END,
    NONE,
}

public class EnumManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
