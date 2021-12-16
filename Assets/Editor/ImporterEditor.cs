using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;


[CustomEditor(typeof(Importer))]
public class ImpoterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var impoter = target as Importer;
        DrawDefaultInspector();

        if (GUILayout.Button("�f�[�^�̍쐬"))
        {
            Debug.Log("�쐬�{�^���������ꂽ");
            SetCardDataToScriptableObject(impoter);
        }
    }

    public void SetCardDataToScriptableObject(Importer impoter)
    {
        if (impoter.pack == null)
        {
            Debug.LogWarning("�ǂݍ��ރp�b�N�����Z�b�g����Ă��܂���B");
            return;
        }

        List<TextAsset> cardTextList = Resources.LoadAll<TextAsset>(impoter.pack).ToList();
        //cardTextList.ToList().ForEach(text => Debug.Log(text));

        if (!cardTextList.Any())
        {
            Debug.LogWarning(impoter.pack + " : �ǂݍ��ރp�b�N�����Ԉ���Ă��܂��B");
            return;
        }

        foreach (TextAsset textAsset in cardTextList)
        {
            var cardData = CreateInstance<CardData>();

            List<string> cardText = textAsset.text.Replace("\r\n", "\n").SplitEx('\n');

            cardData.Name = cardText[1].SplitEx(',')[1];
            cardData.UnitType = cardText[2].SplitEx(',')[1];
            cardData.Clan = cardText[3].SplitEx(',')[1];
            cardData.Race = cardText[4].SplitEx(',')[1];
            cardData.Nation = cardText[5].SplitEx(',')[1];
            cardData.Grade = int.Parse(cardText[6].SplitEx(',')[1]);
            cardData.DefaultPower = int.Parse(cardText[7].SplitEx(',')[1]);
            cardData.DefaultCritical = int.Parse(cardText[8].SplitEx(',')[1]);
            cardData.Shield = int.Parse(cardText[9].SplitEx(',')[1].Replace("-", "0"));
            var skillText = cardText[10].SplitEx(',')[1];
            if (skillText == "�u�[�X�g") cardData.Ability = Card.AbilityType.Boost;
            else if (skillText == "�C���^�[�Z�v�g") cardData.Ability = Card.AbilityType.Intercept;
            else if (skillText == "�c�C���h���C�u") cardData.Ability = Card.AbilityType.TwinDrive;
            else if (skillText == "�g���v���h���C�u") cardData.Ability = Card.AbilityType.TripleDrive;
            var triggerText = cardText[11].SplitEx(',')[1];
            if (triggerText == "-") cardData.Trigger = Card.TriggerType.None;
            else
            {
                var text = triggerText.SplitEx('+');
                if (text[0] == "�N���e�B�J���g���K�[") cardData.Trigger = Card.TriggerType.Critical;
                else if (text[0] == "�h���[�g���K�[") cardData.Trigger = Card.TriggerType.Draw;
                else if (text[0] == "�t�����g�g���K�[") cardData.Trigger = Card.TriggerType.Front;
                else if (text[0] == "�q�[���g���K�[") cardData.Trigger = Card.TriggerType.Heal;
                else if (text[0] == "�X�^���h�g���K�[") cardData.Trigger = Card.TriggerType.Stand;
                else if (text[0] == "�I�[�o�[�g���K�[") cardData.Trigger = Card.TriggerType.Over;
                cardData.TriggerPower = int.Parse(text[1]);
            }
            //cardData.Skill = Resources.Load<SkillData>(cardText[14].SplitEx(',')[1] + "skill");
            cardData.Flavor = cardText[13].SplitEx(',')[1];
            cardData.Number = cardText[14].SplitEx(',')[1];
            cardData.Rarity = cardText[15].SplitEx(',')[1];

            string path = "Assets/Resources/" + cardText[14].SplitEx(',')[1] + "data.asset";
            // �C���X�^���X���������̂��A�Z�b�g�Ƃ��ĕۑ�
            var asset = (CardData)AssetDatabase.LoadAssetAtPath(path, typeof(CardData));
            if (asset == null)
            {
                // �w��̃p�X�Ƀt�@�C�������݂��Ȃ��ꍇ�͐V�K�쐬
                AssetDatabase.CreateAsset(cardData, path);
            }
            else
            {
                // �w��̃p�X�Ɋ��ɓ����̃t�@�C�������݂���ꍇ�͍X�V
                EditorUtility.CopySerialized(cardData, asset);
                AssetDatabase.SaveAssets();
            }
            AssetDatabase.Refresh();
        }

        Debug.Log(impoter.pack + " : �p�b�N���쐬���܂����B");
    }
}