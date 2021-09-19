using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextManager : SingletonMonoBehaviour<TextManager>
{

    public GameObject CanvasPrefab;
    private GameObject Canvas { get; set; }

    public GameObject StatusPrefab;

    public GameObject ActionListPrefab;
    private GameObject ActionList { get; set; }

    public Sprite[] CriticalSprite;

    // Start is called before the first frame update
    void Start()
    {
        Canvas = Instantiate(CanvasPrefab).FixName();

        // 画像をロードする。
        CriticalSprite = Resources.LoadAll<Sprite>("Images/digit1");

        Canvas.GetComponentInChildren<Slider>().onValueChanged.AddListener(value =>
        {
            Time.timeScale = value;
        });
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetPhaseText(string phase)
    {
        Canvas.GetComponentInChildren<Text>().text = phase;
    }

    public void SetStatusText(ICardCircle cardCircle)
    {
        print($"{cardCircle}, {cardCircle.transform.root}");
        Transform status = cardCircle.transform.FindWithChildTag(Tag.StatusText);
        if (status == null) // 初めてテキストを作るとき
        {
            status = Instantiate(StatusPrefab).FixName().transform;
            status.gameObject.ChangeParent(cardCircle.transform, true, true, true);
        }
        else status.gameObject.SetActive(true);

        Card card = cardCircle.Card;

        status.transform.Find("Critical").GetComponent<SpriteRenderer>().sprite = CriticalSprite[card.Critical]; // クリティカル値をセット

        string power = card.Power.ToString();
        int inactiveCount = 5 - power.Length;
        List<Transform> powerDigit = status.transform.FindWithAllChildTag(Tag.Power);

        for (int i = 0; i < 5; i++)
        {
            if (inactiveCount > i)
            {
                powerDigit[i].gameObject.SetActive(false);
            }
            else
            {
                powerDigit[i].gameObject.SetActive(true);
                powerDigit[i].GetComponent<SpriteRenderer>().sprite = CriticalSprite[int.Parse(power[i - inactiveCount].ToString())];
            }
        }
    }

    public void DestroyStatusText(ICardCircle cardCircle)
    {
        cardCircle.transform.FindWithChildTag(Tag.StatusText)?.gameObject.SetActive(false);
    }

    public Transform SetActionList(Card card)
    {
        ActionList = Instantiate(ActionListPrefab).FixName();
        ActionList.transform.parent = card.transform;
        ActionList.transform.localRotation = Quaternion.Euler(0, 180, 0);
        ActionList.transform.localPosition = new Vector3(0, 0, 0.01F);
        ActionList.transform.localScale = card.transform.localScale;
        return ActionList.transform;
    }
}
