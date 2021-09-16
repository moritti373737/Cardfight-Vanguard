using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
public class DeckGenerater : SingletonMonoBehaviour<DeckGenerater>
{
    public GameObject cardPrefab;

    /*public void Generate(List<CardData> cardDataList, Deck _deck)
    {
        for (int i = 0; i < 1; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab);
            cardObj.name = cardDataList[i].name;

            Card card = cardObj.GetComponent<Card>();
            //card.Load(cardDataList[i]);
            _deck.Add(card);
        }
    }*/
    private int Offset = 0;

    public void Generate(Deck deck, FighterID fighterID)
    {
        (List<Texture2D> cardSpriteList, List<string> filename, List<int> cardNumber) = LoadDeckData();
        int spriteNumber = 0;
        int nextSpriteCardNumber = cardNumber[spriteNumber];
        int sum = cardNumber.Sum();
        for (int i = 0; i < sum; i++)
        {
            if (nextSpriteCardNumber <= i)
            {
                spriteNumber++;
                nextSpriteCardNumber += cardNumber[spriteNumber];
            }

            GameObject cardObj = Instantiate(cardPrefab);
            cardObj.name = "Card" + (i + Offset);

            Material material = new Material(Shader.Find("Standard"));
            material.SetTexture("_MainTex", cardSpriteList[spriteNumber]);

            // Rendering Mode を Fade に変更するための処理
            // ここから
            material.SetFloat("_Mode", (float)2);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            // ここまで

            Card card = cardObj.GetComponent<Card>();
            card.transform.Find("Face").GetComponent<MeshRenderer>().material = material;
            //Debug.Log(cardSpriteList[spriteNumber].name.Substring(0, cardSpriteList[spriteNumber].name.Length - 3));
            //Debug.Log(filename[spriteNumber]);
            card.SetStatus(filename[spriteNumber]);
            card.FighterID = fighterID;

            SkillManager.Instance.InitSkill(card);
            //card.cardModel.face = cardSpriteList[0];
            //card.CardModel.ToggleFace(true);
            //card.Load(cardDataList[i]);
            deck.Add(card);
            //Debug.Log(i);
            //cardTextList.ForEach(cardText => Resources.UnloadAsset(cardText));

        }

        Offset += 50;
    }

    private (List<Texture2D> cardSpriteList, List<string> cardTextList, List<int> cardNumber) LoadDeckData()
    {
        string dirName;
        string[,] saveData = LoadSave();
        List<Texture2D> cardSpriteList = new List<Texture2D>();
        List<string> cardTextList = new List<string>();
        List<int> cardNumber = new List<int>();

        for (int i = 0; i < saveData.GetLength(0); i++)
        {
            dirName = saveData[i, 0] + "/" + saveData[i, 1] + "png";
            cardSpriteList.Add(Resources.Load<Texture2D>(dirName));
            dirName = saveData[i, 0] + "/" + saveData[i, 1];
            cardTextList.Add(dirName);
            cardNumber.Add(int.Parse(saveData[i, 2]));

            //Debug.Log(saveData[i,0]);
            //Debug.Log(saveData[i,1]);
            //Debug.Log(saveData[i,2]);
        }

        //deckGenerater.Generate();
        return (cardSpriteList, cardTextList, cardNumber);
    }

    private string[,] LoadSave()
    {
        string[] textMessage; //テキストの加工前の一行を入れる変数
        string[,] saveData; //テキストの複数列を入れる2次元は配列
        int rowLength; //テキスト内の行数を取得する変数
        int columnLength; //テキスト内の列数を取得する変数

        TextAsset saveText = Resources.Load("save/004") as TextAsset;
        //Debug.Log(saveText.text);
        string TextLines = saveText.text; //テキスト全体をstring型で入れる変数を用意して入れる

        Resources.UnloadAsset(saveText);

        //Splitで一行づつを代入した1次配列を作成
        textMessage = TextLines.Split('\n'); //

        //行数と列数を取得
        columnLength = textMessage[0].Split(',').Length;
        rowLength = textMessage.Length;

        //2次配列を定義
        saveData = new string[rowLength, columnLength];

        for (int i = 0; i < rowLength; i++)
        {

            string[] tempWords = textMessage[i].Split(','); //textMessageをカンマごとに分けたものを一時的にtempWordsに代入

            for (int n = 0; n < columnLength; n++)
            {
                saveData[i, n] = tempWords[n]; //2次配列saveDataにカンマごとに分けたtempWordsを代入していく
                //Debug.Log(saveData[i, n]);
            }
        }
        return saveData;

    }
}