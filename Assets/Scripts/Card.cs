using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public int ID { get; private set; } // カード固有のID

    public string Name { get; private set; }     // カード名
    public string UnitType { get; private set; } // ノーマル or トリガーユニット
    public string Clan { get; private set; }     // クラン名
    public string Race { get; private set; }     // 種族名
    public string Nation { get; private set; }   // 国家名
    public int Grade { get; private set; }
    public int Power { get; private set; }
    public int Critical { get; private set; }
    public int Shield { get; private set; }
    public SkillType Skill { get; private set; }
    public string Gift { get; private set; }
    public string Effect { get; private set; }
    public string Flavor { get; private set; }
    public string Number { get; private set; }
    public string Rarity { get; private set; }


    [Flags]
    public enum State
    {
        Stand = 1 << 0,     //2進数だと0001　(10進数だと1)
        Gira = 1 << 1,      //2進数だと0010　(10進数だと2)
        Hoimi = 1 << 2,     //2進数だと0100　(10進数だと4)
        Mera = 1 << 3       //2進数だと1000　(10進数だと8)
    }
    public State state { get; set; }

    public enum SkillType
    {
        Boost,
        Intercept,
        TwinDrive,
        TripleDrive,
    }

    void Start()
    {
        ID = int.Parse(transform.name.Substring(4));
        state = State.Stand;
    }


    public void SetStatus(TextAsset cardText)
    {
        List<string> cardTextList = cardText.text.Replace("\r\n", "\n").SplitEx('\n');

        Name = cardTextList[1].SplitEx(',')[1];
        UnitType = cardTextList[2].SplitEx(',')[1];
        Clan = cardTextList[3].SplitEx(',')[1];
        Race = cardTextList[4].SplitEx(',')[1];
        Nation = cardTextList[5].SplitEx(',')[1];
        Grade = int.Parse(cardTextList[6].SplitEx(',')[1]);
        Power = int.Parse(cardTextList[7].SplitEx(',')[1]);
        Critical = int.Parse(cardTextList[8].SplitEx(',')[1]);
        Shield = int.Parse(cardTextList[9].SplitEx(',')[1].Replace("-", "0"));
        var skillText = cardTextList[10].SplitEx(',')[1];
        if (skillText == "ブースト") Skill = SkillType.Boost;
        else if (skillText == "インターセプト") Skill = SkillType.Intercept;
        else if (skillText == "ツインドライブ") Skill = SkillType.TwinDrive;
        else if (skillText == "トリプルドライブ") Skill = SkillType.TripleDrive;
        Gift = cardTextList[11].SplitEx(',')[1];
        Effect = cardTextList[12].SplitEx(',')[1];
        Flavor = cardTextList[13].SplitEx(',')[1];
        Number = cardTextList[14].SplitEx(',')[1].Replace("/", "-");
        Rarity = cardTextList[15].SplitEx(',')[1];



        if (ID == 0) print(cardText);
    }

    public void TurnOver() => transform.Rotate(0, 180, 0);

    public Texture GetTexture() => transform.Find("Face").GetComponent<Renderer>().material.mainTexture;

    public bool JudgeState(State judgeState) => state == (state | judgeState);
}
