using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class SkillDataJson : SingletonMonoBehaviour<SkillDataJson>
{
    public SkillData LoadSkillData(string number)
    {
        if (!System.IO.File.Exists("Assets/Resources/" + number + "skill.json")) return null;

        var jsonString = Resources.Load<TextAsset>(number + "skill").ToString();

        if (jsonString is null) return null;
        else return Convert(jsonString);

    }

    private SkillData Convert(string jsonString)
    {
        //"{\n  \"CardNumber\": \"TD01/001\",\n  \"SkillList\": [\n    {\n      \"cardNumber\": \"TD01/001\",\n      \"category\": \"Activated\",\n      \"place\": \"Rearguard\",\n      \"condition\": [\n        {\n          \"SourceFighter\": \"Own\",\n          \"SourceCard\": \"Own\",\n          \"SourceCardOption\": \"\",\n          \"TargetFighter\": \"Own\",\n          \"TargetCard\": \"Rearguard\",\n          \"TargetCardOption\": \"\"\n        }\n      ],\n      \"cost\": [\n        {\n          \"Cost\": \"CounterBlast\",\n          \"Count\": 2,\n          \"Option\": \"\"\n        }\n      ],\n      \"skill\": [\n        {\n          \"TargetFighter\": \"Your\",\n          \"TargetCard\": \"Rearguard\",\n          \"TargetOption\": \"\",\n          \"Skill\": \"Retire\",\n          \"SkillOption\": 1\n        }\n      ]\n    }\n  ]\n}"

        // �ϐ���, enum��
        Dictionary<string, string> enumDic = new Dictionary<string, string>()
        {
            {"category", "CategoryType"},
            {"place", "Tag"},
            {"SourceFighter", "FighterType"},
            {"SourceCard", "ConditionType" },
            {"TargetFighter", "FighterType"},
            {"TargetCard", "ConditionType"},
            {"Action", "ActionType"},
            {"Result", "ActionType"},
            {"Cost", "CostType"},
            {"Skill", "SkillType"},
        };

        var jsonStringBuilder = new StringBuilder(jsonString);
        foreach (KeyValuePair<string, string> pair in enumDic)
        {
            int offset = 0;
            while (true) // json���ɓ����value�����݂��邽�ߕ����񒲂ׂ�
            {
                var searchKey = jsonStringBuilder.ToString().IndexOf(pair.Key, offset);
                if (searchKey == -1) break;
                var endcheck = jsonStringBuilder.ToString().IndexOf(":", searchKey) - searchKey - 1 == pair.Key.Length;
                if (!endcheck)
                {
                    offset = searchKey + 1;
                    continue; // "SourceCard"��"SourceCardOption"�̂悤�ɕ�����v���N����Ȃ��悤�Ɋm�F����
                }
                int start = searchKey + pair.Key.Length + 4;                     // json����value�J�n�ʒu
                int end = jsonStringBuilder.ToString().IndexOf(",", start) - 1;  // json����value�I���ʒu
                //Debug.Log($"{start}, {end}");
                //Debug.Log(jsonStringBuilder.ToString().Substring(start, end - start));
                //Debug.Log(Type.GetType(pair.Value));
                //Debug.Log((int)Enum.Parse(Type.GetType(pair.Value), jsonStringBuilder.ToString().Substring(start, end - start)));
                //Debug.Log(Enum.Parse(Type.GetType(pair.Value), jsonStringBuilder.ToString().Substring(start, end - start)).GetType());
                int enumInt = (int)Enum.Parse(Type.GetType(pair.Value), jsonStringBuilder.ToString().Substring(start, end - start)); // �w�肵��enum������int�^�̒l���擾����
                jsonStringBuilder.Replace(jsonStringBuilder.ToString().Substring(start, end - start), enumInt.ToString(), start, end - start); // �w�肵���ʒu��enum�̒l�Œu��������
                offset = end;
            }
        }


        //json�ɂ���N���X�̃C���X�^���X����
        //SkillData jsonClass = new SkillData();

        //jsonClass.SkillList.Add(new Skill() { cardNumber = "124", category = CategoryType.Activated });

        //var weapon = new SkillData2()
        //{
        //    CardNumber = "AF",
        //    SkillList = new List<Skill>()
        //    {
        //        new Skill()
        //        {
        //            cardNumber = "fdas",
        //            category = CategoryType.Activated,
        //        }
        //    },
        //};

        //JsonUtility���g����JSON��(��2������true�ɂ���Ɠǂ݂₷�����`�����)
        //string output = JsonUtility.ToJson(jsonClass, prettyPrint: true);
        SkillData output = JsonUtility.FromJson<SkillData>(jsonStringBuilder.ToString());

        //JSON�m�F
        Debug.Log(jsonStringBuilder.ToString());
        //Debug.Log(output);
        //Debug.Log(output.SkillList[0].condition[0].SourceFighter);
        //Debug.Log(output.SkillList[0].condition[0].SourceCard);
        //Debug.Log(output.SkillList[0].condition[0].TargetFighter);
        //Debug.Log(output.SkillList[0].condition[0].TargetCard);

        return output;
    }

}
