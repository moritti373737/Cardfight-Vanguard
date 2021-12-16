using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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

    public GameObject PhasePrefab;

    // Start is called before the first frame update
    void Start()
    {
        Canvas = Instantiate(CanvasPrefab).FixName();

        Canvas.GetComponentInChildren<Slider>().onValueChanged.AddListener(value =>
        {
            Time.timeScale = value;
        });

        Canvas.GetComponentInChildren<Button>().onClick.AddListener(() => GameMaster.Instance.DebugShuffle());
    }

    public void SetPhaseText(string phase)
    {
        //Canvas.GetComponentInChildren<TextMeshProUGUI>().text = phase;
        //GameObject effect = Instantiate(PhasePrefab);
        //effect.GetComponent<Renderer>().material.renderQueue = 3005;

        TextMeshProUGUI textMeshPro = Canvas.GetComponentInChildren<TextMeshProUGUI>();
        DOTweenTMPAnimator textAnim = new DOTweenTMPAnimator(textMeshPro);

        textMeshPro.text = phase;

        var phaseTransform = Canvas.transform.Find("Phase").GetComponent<RectTransform>();
        phaseTransform.DOMoveX(300F, 0);

        textMeshPro.characterSpacing = -50;
        textMeshPro.DOFade(0, 0);


        //var sequence = DOTween.Sequence();

        //for (int iCnt = 0; iCnt < textAnim.textInfo.characterCount; iCnt++)
        //{
        //    //textAnim.DOFadeChar(iCnt, 1f, 2.0f).SetDelay(0.2f * iCnt);
        //    sequence.Join(textAnim.DOOffsetChar(iCnt, Vector3.left * (400f - iCnt * 20F), 1f));
        //}

        ////textMeshPro.characterSpacing = -50;


        DOTween.Sequence().PrependInterval(0.2F).Join(phaseTransform.DOMoveX(-200F, 1))

        // 文字間隔を開ける
        .Join(DOTween.To(() => textMeshPro.characterSpacing, value => textMeshPro.characterSpacing = value, 10, 1F).SetEase(Ease.OutQuart))

        .Join(textMeshPro.DOFade(1, 0.5F))
        .Insert(0.8F, textMeshPro.DOFade(0, 0.5F));

        //sequence.Play();
    }

    public void SetStatusText(Card card)
    {
        var cardCircle = card.Parent;
        if (cardCircle is null) return;
        Transform status = cardCircle.transform.FindWithChildTag(Tag.StatusText);
        if (status is null) // 初めてテキストを作るとき
        {
            status = Instantiate(StatusPrefab).FixName().transform;
            status.gameObject.ChangeParent(cardCircle.transform, true, true, true);
        }
        else status.gameObject.SetActive(true);

        TextMeshProUGUI textMeshPro = status.transform.Find("Power").GetComponent<TextMeshProUGUI>();

        status.DOMoveY(0.01F, 0);
        textMeshPro.DOCounter(int.Parse(textMeshPro.text), card.Power, 0.25F, false);

        status.transform.Find("Critical").GetComponent<TextMeshProUGUI>().text = card.Critical.ToString(); // クリティカル値をセット

        //print($"{cardCircle}, {cardCircle.transform.root}");
        //Transform status = cardCircle.transform.FindWithChildTag(Tag.StatusText);
        //if (status == null) // 初めてテキストを作るとき
        //{
        //    status = Instantiate(StatusPrefab).FixName().transform;
        //    status.gameObject.ChangeParent(cardCircle.transform, true, true, true);
        //}
        //else status.gameObject.SetActive(true);

        //Card card = cardCircle.Card;

        //status.transform.Find("Critical").GetComponent<SpriteRenderer>().sprite = CriticalSprite[card.Critical]; // クリティカル値をセット

        //string power = card.Power.ToString();
        //int inactiveCount = 5 - power.Length;
        //List<Transform> powerDigit = status.transform.FindWithAllChildTag(Tag.Power);

        //for (int i = 0; i < 5; i++)
        //{
        //    if (inactiveCount > i)
        //    {
        //        powerDigit[i].gameObject.SetActive(false);
        //    }
        //    else
        //    {
        //        powerDigit[i].gameObject.SetActive(true);
        //        powerDigit[i].GetComponent<SpriteRenderer>().sprite = CriticalSprite[int.Parse(power[i - inactiveCount].ToString())];
        //    }
        //}
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
