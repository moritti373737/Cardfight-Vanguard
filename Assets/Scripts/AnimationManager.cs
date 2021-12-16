using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



/// <summary>
/// アニメーション（移動や透過、パーティクルなど）の処理全般
/// </summary>
public class AnimationManager : SingletonMonoBehaviour<AnimationManager>
{
    [SerializeField]
    private GameObject VanguardEffectPrefab;

    [SerializeField]
    private GameObject RearguardEffectPrefab;

    [SerializeField]
    private GameObject RetireEffectPrefab;

    [SerializeField]
    private GameObject ArrowEffectPrefab;
    private GameObject ArrowEffect;

    [SerializeField]
    private List<GameObject> AttackEffectPrefab;

    [SerializeField]
    private GameObject SheildEffectPrefab;
    private GameObject SheildEffect;

    private Sequence sequence = null;

    /// <summary>
    /// デッキからカードを引くアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask DeckToCard(Card card)
    {
        await DOTween.Sequence()
                     .Join(card.transform.DOLocalMoveY(-0.5F, 0.1F).SetEase(Ease.OutQuad))
                     .Join(card.transform.GetComponentsInChildren<MeshRenderer>()[0].material.DOFade(0, 0.1F))
                     .Join(card.transform.GetComponentsInChildren<MeshRenderer>()[1].material.DOFade(0, 0.1F))
                     .Play();
    }

    /// <summary>
    /// カードを手札に加えるアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask CardToHand(Card card)
    {
        card.transform.LocalMoveY(-0.5F);
        await DOTween.Sequence()
                     .Join(card.transform.DOLocalMoveY(0, 0.1F))
                     .Join(card.transform.GetComponentsInChildren<MeshRenderer>()[0].material.DOFade(1, 0.1F))
                     .Join(card.transform.GetComponentsInChildren<MeshRenderer>()[1].material.DOFade(1, 0.1F))
                     .Play();
    }

    /// <summary>
    /// カードを手札からサークルに出すアニメーションとパーティクル
    /// </summary>
    /// <param name="card">サークルに出すカード</param>
    /// <param name="cardCircle">移動先のサークル</param>
    public async UniTask HandToCircle(Card card, ICardCircle cardCircle)
    {
        card.transform.parent = null;

        // カードを手札からサークルに移動する
        await DOTween.Sequence()
                     .Join(card.transform.DOMove(new Vector3(cardCircle.transform.position.x, cardCircle.transform.position.y + 0.001F, cardCircle.transform.position.z), 0.2F))
                     .Join(card.transform.DORotate(new Vector3(cardCircle.transform.rotation.x - 90, cardCircle.transform.rotation.eulerAngles.y + 180, card.transform.rotation.z), 0.2F))
                     .Play();

        // パーティクルを用意し初期位置やemissionの設定
        GameObject effect;
        if (cardCircle.GetType() == typeof(Vanguard))
            effect = Instantiate(VanguardEffectPrefab);
        else if (cardCircle.GetType() == typeof(Rearguard))
            effect = Instantiate(RearguardEffectPrefab);
        else return;

        effect.transform.position = cardCircle.transform.position;

        Material material = effect.transform.GetComponent<MeshRenderer>().material;
        material.EnableKeyword("_EMISSION");
        int intensity = 2;
        float factor = Mathf.Pow(2, intensity);
        material.SetColor("_EmissionColor", new Color(0.0f * factor, 0.6f * factor, 0.6f * factor));

        // パーティクルを回転させる
        await DOTween.Sequence()
                     .Join(effect.transform.DOScale(new Vector3(0.1F, 0.1F, 0), 0.1F).SetRelative())
                     .Join(effect.transform.DORotate(new Vector3(0, 180, 0), 0.15F, RotateMode.WorldAxisAdd).SetRelative())
                     .Join(effect.transform.GetComponent<MeshRenderer>().material.DOFade(0, 0.15F).SetEase(Ease.InCubic))
                     .Play();

        Destroy(effect);
    }

    /// <summary>
    /// カードを手札からガーディアンサークルに出すアニメーションとパーティクル
    /// </summary>
    /// <param name="card">サークルに出すカード</param>
    /// <param name="cardCircle">移動先のサークル</param>
    public async UniTask HandToGuardian(Card card, Guardian guardian)
    {
        card.transform.parent = null;

        // カードを手札からサークルに移動する
        await DOTween.Sequence()
                     .Join(card.transform.DOMove(new Vector3(guardian.transform.position.x, guardian.transform.position.y + 0.001F, guardian.transform.position.z), 0.2F))
                     .Join(card.transform.DORotate(new Vector3(guardian.transform.rotation.x - 90, guardian.transform.rotation.eulerAngles.y + 180, card.transform.rotation.z), 0.2F))
                     .Play();

    }

    /// <summary>
    /// ドライブゾーンからカードをとるアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask DriveToCard(Card card)
    {
        await DeckToCard(card);
    }

    /// <summary>
    /// ダメージゾーンにカードを置くアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    /// <returns></returns>
    public async UniTask CardToDamage(Card card)
    {
        await CardToHand(card);
    }

    /// <summary>
    /// ドロップゾーンにカードを置くアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    /// <returns></returns>
    public async UniTask CardToDrop(Card card)
    {
        await CardToHand(card);
    }

    /// <summary>
    /// フィールド上のカードをめくるアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask RotateFieldCard(Card card)
    {
        var parent = card.transform.parent;
        card.transform.parent = null;

        await DOTween.Sequence()
                     .Join(card.transform.DORotate(new Vector3(0, -180, 0), 0.25F, RotateMode.LocalAxisAdd).SetRelative())
                     .Join(card.transform.DOMoveY(0.05F, 0.1F).SetRelative())
                     .Insert(0.2F, card.transform.DOMoveY(-0.05F, 0.15F).SetRelative())
                     .Play();

        card.transform.SetParent(parent);
    }

    /// <summary>
    /// カードをレストするアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask RestCard(Card card)
    {
        Vector3 defaultScale = card.transform.lossyScale;
        Vector3 lossyScale, localScale;

        await card.transform.DOLocalRotate(new Vector3(0, 180, 90), 0.25F)
                            .OnUpdate(() =>
                            {
                                lossyScale = card.transform.lossyScale;
                                localScale = card.transform.localScale;
                                card.transform.localScale = new Vector3(
                                    localScale.x / lossyScale.x * defaultScale.x,
                                    localScale.y / lossyScale.y * defaultScale.y,
                                    localScale.z / lossyScale.z * defaultScale.z
                                );
                            });

        //Quaternion q1 = card.transform.localRotation;
        //Quaternion q2 = Quaternion.Euler(180f, 0, 270f);
        //Vector3 defaultScale = card.transform.lossyScale;
        //Vector3 lossyScale, localScale;

        //for (int i = 0; i < frame; i++)
        //{
        //    await UniTask.NextFrame();
        //    card.transform.localRotation = Quaternion.Slerp(q1, q2, (float)i / frame); // 線形補間
        //    lossyScale = card.transform.lossyScale;
        //    localScale = card.transform.localScale;
        //    card.transform.localScale = new Vector3(
        //        localScale.x / lossyScale.x * defaultScale.x,
        //        localScale.y / lossyScale.y * defaultScale.y,
        //        localScale.z / lossyScale.z * defaultScale.z
        //    );
        //}
        //card.transform.localRotation = q2;
        //lossyScale = card.transform.lossyScale;
        //localScale = card.transform.localScale;
        //card.transform.localScale = new Vector3(
        //    localScale.x / lossyScale.x * defaultScale.x,
        //    localScale.y / lossyScale.y * defaultScale.y,
        //    localScale.z / lossyScale.z * defaultScale.z
        //);
    }

    /// <summary>
    /// カードをスタンドするアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask StandCard(Card card)
    {
        Vector3 defaultScale = card.transform.lossyScale;
        Vector3 lossyScale, localScale;

        await card.transform.DOLocalRotate(new Vector3(0, 180, 0), 0.25F)
                            .OnUpdate(() =>
                            {
                                lossyScale = card.transform.lossyScale;
                                localScale = card.transform.localScale;
                                card.transform.localScale = new Vector3(
                                    localScale.x / lossyScale.x * defaultScale.x,
                                    localScale.y / lossyScale.y * defaultScale.y,
                                    localScale.z / lossyScale.z * defaultScale.z
                                );
                            });


        //int frame = 15;
        //Quaternion q1 = card.transform.localRotation;
        //Quaternion q2 = Quaternion.Euler(180f, 0, 180f);
        //Vector3 defaultScale = card.transform.lossyScale;
        //Vector3 lossyScale, localScale;

        //for (int i = 0; i < frame; i++)
        //{
        //    await UniTask.NextFrame();
        //    card.transform.localRotation = Quaternion.Slerp(q1, q2, (float)i / frame); // 線形補間
        //    lossyScale = card.transform.lossyScale;
        //    localScale = card.transform.localScale;
        //    card.transform.localScale = new Vector3(
        //        localScale.x / lossyScale.x * defaultScale.x,
        //        localScale.y / lossyScale.y * defaultScale.y,
        //        localScale.z / lossyScale.z * defaultScale.z
        //    );
        //}
        //card.transform.localRotation = q2;
        //lossyScale = card.transform.lossyScale;
        //localScale = card.transform.localScale;
        //card.transform.localScale = new Vector3(
        //    localScale.x / lossyScale.x * defaultScale.x,
        //    localScale.y / lossyScale.y * defaultScale.y,
        //    localScale.z / lossyScale.z * defaultScale.z
        //);
    }

    /// <summary>
    /// デッキからドライブゾーンにカードをめくるアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    /// <param name="drive">移動先のドライブゾーン</param>
    public async UniTask DeckToDrive(Card card, Drive drive)
    {
        var pos1 = drive.transform.position;
        var pos2 = drive.transform.position;
        pos1.y += 0.1F;
        pos2.y += 0.001F;
        Vector3[] path = {
            pos1,
            pos2,
        };

        await DOTween.Sequence()
                     .Join(card.transform.DOPath(path, 0.25F, PathType.CatmullRom).SetEase(Ease.OutCubic))
                     .Join(card.transform.DORotate(new Vector3(180, 90, 0), 0.25F, RotateMode.FastBeyond360).SetRelative().SetEase(Ease.OutCubic))
                     .Play();
    }

    public async UniTask MoveSelectBox(GameObject selectBox, Transform targetTransform)
    {
        selectBox.transform.parent = null;
        await DOTween.Sequence()
                     .Join(selectBox.transform.DOMove(targetTransform.position, 0.15F))
                     .Join(selectBox.transform.DORotate(new Vector3(selectBox.transform.rotation.eulerAngles.x, targetTransform.rotation.eulerAngles.y, selectBox.transform.rotation.eulerAngles.z), 0.15F))
                     .Play();
    }

    /// <summary>
    /// カードを退却させるアニメーション（リアガードやガーディアンサークル）
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask RetireCard(Card card)
    {
        var effect = Instantiate(RetireEffectPrefab);
        effect.transform.position = card.transform.position;
        effect.transform.MoveY(0.01F);
        effect.transform.Rotate(90, card.transform.eulerAngles.y, 0); // カードの向きに合わせる
        Color color = card.Face.material.color;
        color.a = 0;
        card.Face.material.color = color;
        color = card.Back.material.color;
        color.a = 0;
        card.Back.material.color = color;
        //ParticleSystem[] particle = effect.GetComponentsInChildren<ParticleSystem>();
        Destroy(effect, 3);
        await UniTask.Delay(500);
    }

    public void SelectButtleCard(Transform sourceCircle, Transform targetCircle)
    {
        sequence = DOTween.Sequence();
        ArrowEffect = Instantiate(ArrowEffectPrefab);

        //effect.transform.position = sourceCircle.position;
        //effect.transform.MoveY(0.05F);
        ArrowEffect.transform.DOLookAt(targetCircle.position, 0).SetRelative();
        Vector3 center = (sourceCircle.position + targetCircle.position) * 0.5F;

        Material material = ArrowEffect.GetComponent<Renderer>().material;
        material.EnableKeyword("_EMISSION");

        sequence.Join(material.DOFade(0, 0))
                .Join(material.DOFade(1, 0.25F))
                .Join(ArrowEffect.transform.DOMove(sourceCircle.position.GetAddY(0.05F), 0))
                .Join(ArrowEffect.transform.DOMove(center.GetAddY(0.1F), 0.5F))
                .Join(ArrowEffect.transform.DOMove(targetCircle.position.GetAddY(0.05F), 0.5F))
                .Join(material.DOColor(new Color(12, 0, 10), "_EmissionColor", 0.5F))
                .Insert(0.25F, material.DOFade(0, 0.25F))
                .SetLoops(-1)
                .Play();
    }

    public void KillSequence()
    {
        sequence.Kill();
        Destroy(ArrowEffect);
    }

    public async UniTask AttackEffect(Transform sourceCircle, Transform targetCircle, int number)
    {
        var effect = Instantiate(AttackEffectPrefab[number]);
        effect.transform.LookAt(targetCircle);
        effect.transform.Rotate(0, 0, 60);
        await UniTask.WaitUntil(() => !effect.GetComponent<ParticleSystem>().isPlaying);
        Destroy(effect);
    }

    public async UniTask HitEffect(Transform card)
    {
        await card.DOShakePosition(0.5F, strength: 0.1F);
    }

    public void StartSheildEffect(Transform circle)
    {
        SheildEffect = Instantiate(SheildEffectPrefab);
        SheildEffect.transform.position = (circle.transform.position + Vector3.zero) / 2;
    }

    public void EndSheildEffect()
    {
        if (SheildEffect == null) return;
        Animator[] animator = SheildEffect.transform.Find("Effect").GetComponentsInChildren<Animator>();
        foreach (var anim in animator)
        {
            anim.SetBool("EndSheild", true);
        }

        ParticleSystem[] effect = SheildEffect.transform.Find("Indicator").GetComponentsInChildren<ParticleSystem>();
        foreach (var ef in effect)
        {
            ef.Simulate(4.5F);
            ef.Play();
        }
        Debug.Log(SheildEffect.transform.Find("Indicator").GetComponentInChildren<ParticleSystem>().time);

        Destroy(SheildEffect, 1);
    }

    public void ActivateSkill()
    {
        Debug.Log("Skill発動");
    }

    public async UniTask DeckShuffle(List<Card> cardList, float offset)
    {
        List<Transform> transformList = cardList.Select(card => card.transform).ToList();
        List<Transform> downCard = transformList.GetRange(0, cardList.Count / 2); // 引くカードより上にあるカード
        List<Transform> pullCard = transformList.GetRange(cardList.Count / 2, cardList.Count / 3); // 引くカード

        Sequence sequence = DOTween.Sequence();

        foreach (var card in downCard)
        {
            _ = sequence.Join(card.DOLocalMoveZ(offset * pullCard.Count, 0.25F).SetRelative());
        }

        foreach (var (card, index) in pullCard.Select((card, index) => (card, index)))
        {
            float z = (index - cardList.Count) * offset;
            Vector3[] path =
            {
                new Vector3(card.localPosition.x, card.localPosition.y - 1, z / 2),
                new Vector3(card.localPosition.x, card.localPosition.y, z),
            };
            _ = sequence.Join(card.DOLocalPath(path, 0.25F, pathType: PathType.CatmullRom));
        }

        await sequence.SetLoops(3).Play();
    }
}