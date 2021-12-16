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

        if (GUILayout.Button("データの作成"))
        {
            Debug.Log("作成ボタンが押された");
            SetCardDataToScriptableObject(impoter);
        }
    }

    public void SetCardDataToScriptableObject(Importer impoter)
    {
        if (impoter.pack == null)
        {
            Debug.LogWarning("読み込むパック名がセットされていません。");
            return;
        }

        List<TextAsset> cardTextList = Resources.LoadAll<TextAsset>(impoter.pack).ToList();
        //cardTextList.ToList().ForEach(text => Debug.Log(text));

        if (!cardTextList.Any())
        {
            Debug.LogWarning(impoter.pack + " : 読み込むパック名が間違っています。");
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
            if (skillText == "ブースト") cardData.Ability = Card.AbilityType.Boost;
            else if (skillText == "インターセプト") cardData.Ability = Card.AbilityType.Intercept;
            else if (skillText == "ツインドライブ") cardData.Ability = Card.AbilityType.TwinDrive;
            else if (skillText == "トリプルドライブ") cardData.Ability = Card.AbilityType.TripleDrive;
            var triggerText = cardText[11].SplitEx(',')[1];
            if (triggerText == "-") cardData.Trigger = Card.TriggerType.None;
            else
            {
                var text = triggerText.SplitEx('+');
                if (text[0] == "クリティカルトリガー") cardData.Trigger = Card.TriggerType.Critical;
                else if (text[0] == "ドロートリガー") cardData.Trigger = Card.TriggerType.Draw;
                else if (text[0] == "フロントトリガー") cardData.Trigger = Card.TriggerType.Front;
                else if (text[0] == "ヒールトリガー") cardData.Trigger = Card.TriggerType.Heal;
                else if (text[0] == "スタンドトリガー") cardData.Trigger = Card.TriggerType.Stand;
                else if (text[0] == "オーバートリガー") cardData.Trigger = Card.TriggerType.Over;
                cardData.TriggerPower = int.Parse(text[1]);
            }
            //cardData.Skill = Resources.Load<SkillData>(cardText[14].SplitEx(',')[1] + "skill");
            cardData.Flavor = cardText[13].SplitEx(',')[1];
            cardData.Number = cardText[14].SplitEx(',')[1];
            cardData.Rarity = cardText[15].SplitEx(',')[1];

            string path = "Assets/Resources/" + cardText[14].SplitEx(',')[1] + "data.asset";
            // インスタンス化したものをアセットとして保存
            var asset = (CardData)AssetDatabase.LoadAssetAtPath(path, typeof(CardData));
            if (asset == null)
            {
                // 指定のパスにファイルが存在しない場合は新規作成
                AssetDatabase.CreateAsset(cardData, path);
            }
            else
            {
                // 指定のパスに既に同名のファイルが存在する場合は更新
                EditorUtility.CopySerialized(cardData, asset);
                AssetDatabase.SaveAssets();
            }
            AssetDatabase.Refresh();
        }

        Debug.Log(impoter.pack + " : パックを作成しました。");
    }
}