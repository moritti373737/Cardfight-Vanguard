using Cysharp.Threading.Tasks;
using DG.Tweening;
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

    /// <summary>
    /// デッキからカードを引くアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask DeckToCard(Card card)
    {
        var sequence = DOTween.Sequence();

        _ = sequence.Join(card.transform.DOLocalMoveY(-0.5F, 0.1F).SetEase(Ease.OutQuad));
        _ = sequence.Join(card.transform.GetComponentsInChildren<MeshRenderer>()[0].material.DOFade(0, 0.1F));
        _ = sequence.Join(card.transform.GetComponentsInChildren<MeshRenderer>()[1].material.DOFade(0, 0.1F));

        await sequence.Play();
    }

    /// <summary>
    /// カードを手札に加えるアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask CardToHand(Card card)
    {
        var sequence = DOTween.Sequence();
        card.transform.LocalMoveY(-0.5F);
        _ = sequence.Join(card.transform.DOLocalMoveY(0, 0.1F));
        _ = sequence.Join(card.transform.GetComponentsInChildren<MeshRenderer>()[0].material.DOFade(1, 0.1F));
        _ = sequence.Join(card.transform.GetComponentsInChildren<MeshRenderer>()[1].material.DOFade(1, 0.1F));

        await sequence.Play();
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
        var sequence = DOTween.Sequence();
        _ = sequence.Join(card.transform.DOMove(new Vector3(cardCircle.transform.position.x, cardCircle.transform.position.y + 0.001F, cardCircle.transform.position.z), 0.2F));
        _ = sequence.Join(card.transform.DORotate(new Vector3(cardCircle.transform.rotation.x - 90, cardCircle.transform.rotation.eulerAngles.y + 180, card.transform.rotation.z), 0.2F));
        await sequence.Play();

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
        sequence = DOTween.Sequence();
        _ = sequence.Join(effect.transform.DOScale(new Vector3(0.1F, 0.1F, 0), 0.1F).SetRelative());
        _ = sequence.Join(effect.transform.DORotate(new Vector3(0, 180, 0), 0.15F, RotateMode.WorldAxisAdd).SetRelative());
        _ = sequence.Join(effect.transform.GetComponent<MeshRenderer>().material.DOFade(0, 0.15F).SetEase(Ease.InCubic));

        await sequence.Play();

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
        var sequence = DOTween.Sequence();
        _ = sequence.Join(card.transform.DOMove(new Vector3(guardian.transform.position.x, guardian.transform.position.y + 0.001F, guardian.transform.position.z), 0.2F));
        _ = sequence.Join(card.transform.DORotate(new Vector3(guardian.transform.rotation.x - 90, guardian.transform.rotation.eulerAngles.y + 180, card.transform.rotation.z), 0.2F));
        await sequence.Play();

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
        var sequence = DOTween.Sequence();
        _ = sequence.Join(card.transform.DORotate(new Vector3(0, 0, -180), 0.25F, RotateMode.WorldAxisAdd).SetRelative());
        _ = sequence.Join(card.transform.DOMoveY(0.05F, 0.1F).SetRelative());
        _ = sequence.Insert(0.2F, card.transform.DOMoveY(-0.05F, 0.15F).SetRelative());

        await sequence.Play();
    }

    /// <summary>
    /// カードをレストするアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    /// <param name="frame">アニメーションの動作フレーム数</param>
    public async UniTask RestCard(Card card, int frame)
    {
        Quaternion q1 = card.transform.localRotation;
        Quaternion q2 = Quaternion.Euler(180f, 0, 270f);
        Vector3 defaultScale = card.transform.lossyScale;
        Vector3 lossyScale, localScale;

        for (int i = 0; i < frame; i++)
        {
            await UniTask.NextFrame();
            card.transform.localRotation = Quaternion.Slerp(q1, q2, (float)i / frame); // 線形補間
            lossyScale = card.transform.lossyScale;
            localScale = card.transform.localScale;
            card.transform.localScale = new Vector3(
                localScale.x / lossyScale.x * defaultScale.x,
                localScale.y / lossyScale.y * defaultScale.y,
                localScale.z / lossyScale.z * defaultScale.z
            );
        }
        card.transform.localRotation = q2;
        lossyScale = card.transform.lossyScale;
        localScale = card.transform.localScale;
        card.transform.localScale = new Vector3(
            localScale.x / lossyScale.x * defaultScale.x,
            localScale.y / lossyScale.y * defaultScale.y,
            localScale.z / lossyScale.z * defaultScale.z
        );
    }

    /// <summary>
    /// カードをスタンドするアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    /// <param name="frame">アニメーションの動作フレーム数</param>
    public async UniTask StandCard(Card card, int frame)
    {
        Quaternion q1 = card.transform.localRotation;
        Quaternion q2 = Quaternion.Euler(180f, 0, 180f);
        Vector3 defaultScale = card.transform.lossyScale;
        Vector3 lossyScale, localScale;

        for (int i = 0; i < frame; i++)
        {
            await UniTask.NextFrame();
            card.transform.localRotation = Quaternion.Slerp(q1, q2, (float)i / frame); // 線形補間
            lossyScale = card.transform.lossyScale;
            localScale = card.transform.localScale;
            card.transform.localScale = new Vector3(
                localScale.x / lossyScale.x * defaultScale.x,
                localScale.y / lossyScale.y * defaultScale.y,
                localScale.z / lossyScale.z * defaultScale.z
            );
        }
        card.transform.localRotation = q2;
        lossyScale = card.transform.lossyScale;
        localScale = card.transform.localScale;
        card.transform.localScale = new Vector3(
            localScale.x / lossyScale.x * defaultScale.x,
            localScale.y / lossyScale.y * defaultScale.y,
            localScale.z / lossyScale.z * defaultScale.z
        );
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

        var sequence = DOTween.Sequence();
        _ = sequence.Join(card.transform.DOPath(path, 0.25F, PathType.CatmullRom).SetEase(Ease.OutCubic));
        _ = sequence.Join(card.transform.DORotate(new Vector3(180, 90, 0), 0.25F, RotateMode.FastBeyond360).SetRelative().SetEase(Ease.OutCubic));

        await sequence.Play();
    }

    public async UniTask MoveCircle(GameObject circle, Transform targetTransform)
    {
        circle.transform.parent = null;
        var sequence = DOTween.Sequence();
        _ = sequence.Join(circle.transform.DOMove(targetTransform.position, 0.15F));
        _ = sequence.Join(circle.transform.DORotate(targetTransform.rotation.eulerAngles, 0.15F));
        await sequence.Play();
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
        ParticleSystem[] particle = effect.GetComponentsInChildren<ParticleSystem>();
        Destroy(effect, 3);
        await UniTask.Delay(500);
    }
}