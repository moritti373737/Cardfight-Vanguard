using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
public class DeckGenerater : MonoBehaviour
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

    public void Generate(Deck _deck)
    {
        (List<Texture2D> cardSpriteList, List<TextAsset> cardTextList, List<int> cardNumber) = LoadDeckData();
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
            cardObj.name = "Card" + i;

            Card card = cardObj.GetComponent<Card>();
            Material material = new Material(Shader.Find("Standard"));
            material.SetTexture("_MainTex", cardSpriteList[spriteNumber]);

            // Rendering Mode �� Fade �ɕύX���邽�߂̏���
            // ��������
            material.SetFloat("_Mode", (float)2);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            // �����܂�

            card.transform.Find("Face").GetComponent<MeshRenderer>().material = material;

            //card.cardModel.face = cardSpriteList[0];
            //card.CardModel.ToggleFace(true);
            //card.Load(cardDataList[i]);
            _deck.Add(card);
            //Debug.Log(i);

        }
    }

    private (List<Texture2D> cardSpriteList, List<TextAsset> cardTextList, List<int> cardNumber) LoadDeckData()
    {
        string dirName;
        string[,] saveData = LoadSave();
        List<Texture2D> cardSpriteList = new List<Texture2D>();
        List<TextAsset> cardTextList = new List<TextAsset>();
        List<int> cardNumber = new List<int>();

        for (int i = 0; i < saveData.GetLength(0); i++)
        {
            dirName = saveData[i, 0] + "/" + saveData[i, 1] + "img";
            cardSpriteList.Add(Resources.Load<Texture2D>(dirName));
            dirName = saveData[i, 0] + "/" + saveData[i, 1] + "txt";
            cardTextList.Add(Resources.Load(dirName) as TextAsset);
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
        string[] textMessage; //�e�L�X�g�̉��H�O�̈�s������ϐ�
        string[,] saveData; //�e�L�X�g�̕����������2�����͔z��
        int rowLength; //�e�L�X�g���̍s�����擾����ϐ�
        int columnLength; //�e�L�X�g���̗񐔂��擾����ϐ�

        TextAsset saveText = Resources.Load("save/004") as TextAsset;
        //Debug.Log(saveText.text);
        string TextLines = saveText.text; //�e�L�X�g�S�̂�string�^�œ����ϐ���p�ӂ��ē����

        //Split�ň�s�Â�������1���z����쐬
        textMessage = TextLines.Split('\n'); //

        //�s���Ɨ񐔂��擾
        columnLength = textMessage[0].Split(',').Length;
        rowLength = textMessage.Length;

        //2���z����`
        saveData = new string[rowLength, columnLength];

        for (int i = 0; i < rowLength; i++)
        {

            string[] tempWords = textMessage[i].Split(','); //textMessage���J���}���Ƃɕ��������̂��ꎞ�I��tempWords�ɑ��

            for (int n = 0; n < columnLength; n++)
            {
                saveData[i, n] = tempWords[n]; //2���z��saveData�ɃJ���}���Ƃɕ�����tempWords�������Ă���
                //Debug.Log(saveData[i, n]);
            }
        }
        return saveData;

    }
}