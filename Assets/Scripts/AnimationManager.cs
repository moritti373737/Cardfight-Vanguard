using Cysharp.Threading.Tasks;
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
    /// <summary>
    /// デッキからカードを引くアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask DeckToCard(Card card)
    {
        //_ = ChangeAlphaCard(card, 50, 1, 0);
        //await MoveCard(card, 50, true, targetY: -1);
        await UniTask.WhenAll(MoveCard(card, 5, true, targetY: -0.3F), ChangeAlphaCard(card, 5, 1, 0));

    }

    /// <summary>
    /// カードを手札に加えるアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    public async UniTask CardToHand(Card card)
    {

        //Vector3 startPosition = card.transform.localPosition;
        //Vector3 endPosition = new Vector3(startPosition.x, startPosition.y - 1, startPosition.z);
        await UniTask.WhenAll(MoveCard(card, 10, true, targetY: 0.1F, offsetY: -0.1F), ChangeAlphaCard(card, 10, 0, 1));
    }

    public async UniTask HandToZone(Card card, ICardZone cardZone)
    {
        Vector3 startPosition = card.transform.position;
        Vector3 endPosition = cardZone.transform.position;
        for (int i = 0; i < 10; i++)
        {
            card.transform.position = Vector3.Lerp(startPosition, endPosition, (float)i / 10);
            await UniTask.NextFrame();
        }
    }

    public async UniTask DriveToCard(Card card)
    {
        //_ = ChangeAlphaCard(card, 50, 1, 0);
        //await MoveCard(card, 50, true, targetY: -1);
        await UniTask.WhenAll(MoveCard(card, 10, true, targetY: -0.3F), ChangeAlphaCard(card, 10, 1, 0));

    }

    public async UniTask CardToDamage(Card card)
    {
        //Vector3 startPosition = card.transform.localPosition;
        //Vector3 endPosition = new Vector3(startPosition.x, startPosition.y - 1, startPosition.z);
        await UniTask.WhenAll(MoveCard(card, 10, true, targetY: 0.1F, offsetY: -0.1F), ChangeAlphaCard(card, 10, 0, 1));
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

        await RotateCard(card, 60);
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
        int frame = 10;

        Vector3 startPosition = card.transform.position;
        Vector3 endPosition = drive.transform.position;
        endPosition.y += 0.18F;


        Quaternion q1 = card.transform.rotation;
        Quaternion q2 = Quaternion.Euler(200f, 0f, 30f);
        Quaternion q3 = Quaternion.Euler(270f, 0f, 90f);
        for (int i = 0; i <= frame; i++)
        {
            //var scale = card.transform.localScale;
            //card.transform.Rotate(0, 2, 1);
            card.transform.rotation = Quaternion.Slerp(q1, q2, i / 10.0F); // 線形補間
            //card.transform.localScale = scale;
            card.transform.position = Vector3.Slerp(startPosition, endPosition, (float)i / 20);
            await UniTask.NextFrame();
        }
        startPosition = card.transform.position;
        endPosition = drive.transform.position;
        endPosition.y += 0.001F;
        for (int i = 0; i <= frame; i++)
        {
            //var scale = card.transform.localScale;
            //card.transform.Rotate(0, 2, 1);
            card.transform.rotation = Quaternion.Slerp(q2, q3, i / 10.0F); // 線形補間
            //card.transform.localScale = scale;
            card.transform.position = Vector3.Slerp(startPosition, endPosition, (float)i / 10);
            await UniTask.NextFrame();
        }
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

    async UniTask ChangeAlphaCard(Card card, int frame, float source, float target)
    {
        MeshRenderer[] meshRenderer = card.transform.GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i <= frame; i++)
        {
            foreach (var mesh in meshRenderer)
            {
                Color color = mesh.material.color;
                color.a += (target - source) / frame;
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
