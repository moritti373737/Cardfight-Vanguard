using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// アニメーション（移動や透過など）の処理全般
/// </summary>
public class AnimationManager : SingletonMonoBehaviour<AnimationManager>
{
    [SerializeField]
    GameObject VanguardEffectPrefab;

    [SerializeField]
    GameObject RearguardEffectPrefab;

    /// <summary>
    /// デッキからカードを引くアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask DeckToCard(Card card)
    {
        //_ = ChangeAlphaCard(card, 50, 1, 0);
        //await MoveCard(card, 50, true, targetY: -1);
        //await UniTask.WhenAll(MoveCard(card, 5, true, targetY: -0.3F), ChangeAlpha(card.transform, 5, 1, 0));
        var sequence = DOTween.Sequence();

        _ = sequence.Join(card.transform.DOLocalMoveY(-0.5F, 0.1F).SetEase(Ease.OutQuad));
        _ = sequence.Join(card.transform.GetComponentsInChildren<MeshRenderer>()[0].material.DOFade(0, 0.1F));
        _ = sequence.Join(card.transform.GetComponentsInChildren<MeshRenderer>()[1].material.DOFade(0, 0.1F));

        await sequence.Play();

        //await card.transform.DOLocalMoveY(-0.5F, 1);

    }

    /// <summary>
    /// カードを手札に加えるアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask CardToHand(Card card)
    {

        //Vector3 startPosition = card.transform.localPosition;
        //Vector3 endPosition = new Vector3(startPosition.x, startPosition.y - 1, startPosition.z);
        //await UniTask.WhenAll(MoveCard(card, 10, true, targetY: 0.1F, offsetY: -0.1F), ChangeAlpha(card.transform, 10, 0, 1));

        var sequence = DOTween.Sequence();
        card.transform.LocalMoveY(-0.5F);
        _ = sequence.Join(card.transform.DOLocalMoveY(0, 0.1F));
        _ = sequence.Join(card.transform.GetComponentsInChildren<MeshRenderer>()[0].material.DOFade(1, 0.1F));
        _ = sequence.Join(card.transform.GetComponentsInChildren<MeshRenderer>()[1].material.DOFade(1, 0.1F));

        await sequence.Play();

    }

    public async UniTask HandToCircle(Card card, ICardCircle cardCircle)
    {
        //async UniTask task1(GameObject effect)
        //{
        //    Vector3 startScale = effect.transform.localScale;
        //    Vector3 endScale = new Vector3(startScale.x + 0.1F, startScale.y + 0.1F, startScale.z);

        //    for (int i = 0; i < 20; i++)
        //    {
        //        effect.transform.localScale = Vector3.Lerp(startScale, endScale, (float)i / 30);
        //        effect.transform.Rotate(0, 0, -3);
        //        await UniTask.NextFrame();
        //    }
        //}

        //async UniTask task2(GameObject effect)
        //{
        //    await UniTask.Delay(70);
        //    await ChangeAlpha(effect.transform, 15, 1, 0);
        //    effect.SetActive(false);
        //}

        card.transform.parent = null;

        //Vector3 startPosition = card.transform.position;
        //Vector3 endPosition = cardCircle.transform.position;

        //Quaternion startRotation = card.transform.rotation;
        //Quaternion endRotation = Quaternion.Euler(270, 0, 180);

        //for (int i = 0; i < 10; i++)
        //{
        //    card.transform.position = Vector3.Lerp(startPosition, endPosition, (float)i / 10);
        //    card.transform.rotation = Quaternion.Lerp(startRotation, endRotation, (float)i / 10);
        //    await UniTask.NextFrame();
        //}

        //card.transform.position = cardCircle.transform.position;
        //card.transform.rotation = Quaternion.Euler(270, 0, 180);
        var sequence = DOTween.Sequence();
        _ = sequence.Join(card.transform.DOMove(new Vector3(cardCircle.transform.position.x, cardCircle.transform.position.y + 0.001F, cardCircle.transform.position.z), 0.2F));
        _ = sequence.Join(card.transform.DORotate(new Vector3(cardCircle.transform.rotation.x - 90, cardCircle.transform.rotation.eulerAngles.y + 180, card.transform.rotation.z), 0.2F));
        await sequence.Play();

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

        //await UniTask.WhenAll(task1(effect), task2(effect));

        sequence = DOTween.Sequence();
        _ = sequence.Join(effect.transform.DOScale(new Vector3(0.1F, 0.1F, 0), 0.1F).SetRelative());
        _ = sequence.Join(effect.transform.DORotate(new Vector3(0, 180, 0), 0.15F, RotateMode.WorldAxisAdd).SetRelative());
        _ = sequence.Join(effect.transform.GetComponent<MeshRenderer>().material.DOFade(0, 0.15F).SetEase(Ease.InCubic));

        await sequence.Play();

        Destroy(effect);
    }

    public async UniTask DriveToCard(Card card)
    {
        //_ = ChangeAlphaCard(card, 50, 1, 0);
        //await MoveCard(card, 50, true, targetY: -1);
        //await UniTask.WhenAll(MoveCard(card, 10, true, targetY: -0.3F), ChangeAlpha(card.transform, 10, 1, 0));
        await DeckToCard(card);

    }

    public async UniTask CardToDamage(Card card)
    {
        //Vector3 startPosition = card.transform.localPosition;
        //Vector3 endPosition = new Vector3(startPosition.x, startPosition.y - 1, startPosition.z);
        //await UniTask.WhenAll(MoveCard(card, 10, true, targetY: 0.1F, offsetY: -0.1F), ChangeAlpha(card.transform, 10, 0, 1));
        await CardToHand(card);
    }

    /// <summary>
    /// フィールド上のカードをめくるアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask RotateFieldCard(Card card)
    {
        //List<Coroutine> parallel = new List<Coroutine>();

        //parallel.Add(StartCoroutine(RotateCard(card, 60)));
        //parallel.Add(StartCoroutine(MoveCard(card, 0.01F, 20)));

        //// 全てのコルーチンが終了するのを待機
        //foreach (var c in parallel)
        //    yield return c;
        var sequence = DOTween.Sequence();
        _ = sequence.Join(card.transform.DORotate(new Vector3(0, 0, -180), 0.25F, RotateMode.WorldAxisAdd).SetRelative());
        _ = sequence.Join(card.transform.DOMoveY(0.05F, 0.1F).SetRelative());
        _ = sequence.Insert(0.2F, card.transform.DOMoveY(-0.05F, 0.15F).SetRelative());

        await sequence.Play();
        //await RotateCard(card, 60);
        //await MoveCard(card, 20, false, targetY:0.01F);
        //await MoveCard(card, 40, false, targetY:-0.01F);
    }
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
        //_ = sequence.Join(card.transform.DORotate(new Vector3(0, 0, -180), 0.25F, RotateMode.WorldAxisAdd).SetRelative());
        //_ = sequence.Join(card.transform.DOMove(drive.transform.position, 1F));
        _ = sequence.Join(card.transform.DOPath(path, 0.3F, PathType.CatmullRom).SetEase(Ease.OutCubic));
        _ = sequence.Join(card.transform.DORotate(new Vector3(270, 0, 90), 0.3F).SetEase(Ease.OutCubic));

        await sequence.Play();


        //int frame = 10;

        //Vector3 startPosition = card.transform.position;
        //Vector3 endPosition = drive.transform.position;
        //endPosition.y += 0.18F;


        //Quaternion q1 = card.transform.rotation;
        //Quaternion q2 = Quaternion.Euler(200f, 0f, 30f);
        //Quaternion q3 = Quaternion.Euler(270f, 0f, 90f);
        //for (int i = 0; i <= frame; i++)
        //{
        //    //var scale = card.transform.localScale;
        //    //card.transform.Rotate(0, 2, 1);
        //    card.transform.rotation = Quaternion.Slerp(q1, q2, i / 10.0F); // 線形補間
        //    //card.transform.localScale = scale;
        //    card.transform.position = Vector3.Slerp(startPosition, endPosition, (float)i / 20);
        //    await UniTask.NextFrame();
        //}
        //startPosition = card.transform.position;
        //endPosition = drive.transform.position;
        //endPosition.y += 0.001F;
        //for (int i = 0; i <= frame; i++)
        //{
        //    //var scale = card.transform.localScale;
        //    //card.transform.Rotate(0, 2, 1);
        //    card.transform.rotation = Quaternion.Slerp(q2, q3, i / 10.0F); // 線形補間
        //    //card.transform.localScale = scale;
        //    card.transform.position = Vector3.Slerp(startPosition, endPosition, (float)i / 10);
        //    await UniTask.NextFrame();
        //}
    }

    public async UniTask MoveCircle(GameObject circle, Transform targetTransform)
    {
        circle.transform.parent = null;
        var sequence = DOTween.Sequence();
        _ = sequence.Join(circle.transform.DOMove(targetTransform.position, 0.15F));
        _ = sequence.Join(circle.transform.DORotate(targetTransform.rotation.eulerAngles, 0.15F));
        await sequence.Play();
    }


    async UniTask RotateCard(Card card, int frame)
    {
        for (int i = 0; i < frame; i++)
        {
            card.transform.Rotate(0, -180 / frame, 0);
            await UniTask.NextFrame();
        }
    }

    /// <summary>
    /// カード直線移動させるアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    /// <param name="frame">アニメの総フレーム数</param>
    /// <param name="local">ローカル座標で計算する、falseの場合はグローバル座標を用いる</param>
    /// <param name="targetX">X方向の移動量</param>
    /// <param name="targetY">Y方向の移動量</param>
    /// <param name="targetZ">Z方向の移動量</param>
    /// <returns></returns>
    async UniTask MoveCard(Card card, int frame, bool local, float targetX = 0, float targetY = 0, float targetZ = 0, float offsetX = 0, float offsetY = 0, float offsetZ = 0)
    {
        Vector3 startPosition = local ? card.transform.localPosition : card.transform.position;
        startPosition = new Vector3(startPosition.x + offsetX, startPosition.y + offsetY, startPosition.z + offsetZ);
        Vector3 endPosition = new Vector3(startPosition.x + targetX, startPosition.y + targetY, startPosition.z + targetZ);
        for (int i = 0; i <= frame; i++)
        {
            if (local)
                card.transform.localPosition = Vector3.Lerp(startPosition, endPosition, (float)i / frame);
            else
                card.transform.position = Vector3.Lerp(startPosition, endPosition, (float)i / frame);
            //Debug.Log(i);
            await UniTask.NextFrame();
        }
    }

    async UniTask ChangeAlpha(Transform transform, int frame, float source, float target)
    {
        MeshRenderer[] meshRenderer = transform.GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i <= frame; i++)
        {
            foreach (var mesh in meshRenderer)
            {
                Color color = mesh.material.color;
                color.a += (target - source) / frame;
                if (color.a < 0) color.a = 0;
                else if (color.a > 1) color.a = 1;
                mesh.material.color = color;

            }
            await UniTask.NextFrame();
        }

        foreach (var mesh in meshRenderer)
        {
            Color color = mesh.material.color;
            color.a = target;
            mesh.material.color = color;
        }

    }
}
