using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    [SerializeField]
    Fighter Fighter1;
    [SerializeField]
    Fighter Fighter2;

    public ReactiveCollection<string> Ability = new ReactiveCollection<string>();


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
