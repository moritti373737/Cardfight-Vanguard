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

    public Sprite[] CriticalSprite;

    // Start is called before the first frame update
    void Start()
    {
        Canvas = Instantiate(CanvasPrefab).FixName();

        // âÊëúÇÉçÅ[ÉhÇ∑ÇÈÅB
        CriticalSprite = Resources.LoadAll<Sprite>("Images/digit1");
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
        print($"{cardCircle}, {cardCircle.GetTransform().root}");
        var status = Instantiate(StatusPrefab).FixName();
        status.ChangeParent(cardCircle.GetTransform(), true, true, true);

        Card card = cardCircle.GetCard();
        status.transform.Find("Critical").GetComponent<SpriteRenderer>().sprite = CriticalSprite[card.Critical];

        string power = card.Power.ToString();
        int inactiveCount = 5 - power.Length;
        List<Transform> powerDigit = status.transform.FindWithAllChildTag(Tag.Power);

        for (int i = 0; i < 5; i++)
        {
            if (inactiveCount > i)
            {
                powerDigit[i].gameObject.SetActive(false);
                continue;
            }
            powerDigit[i].GetComponent<SpriteRenderer>().sprite = CriticalSprite[int.Parse(power[i - inactiveCount].ToString())];
        }
    }

    public void DestroyStatusText(ICardCircle cardCircle)
    {
        List<Transform> statusTextList = cardCircle.GetTransform().FindWithAllChildTag(Tag.StatusText);
        foreach (var statusText in statusTextList)
        {
            Destroy(statusText.gameObject);
        }
    }

}
