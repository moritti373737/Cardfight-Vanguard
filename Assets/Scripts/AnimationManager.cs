using System.Collections;
using System.Collections.Generic;
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
    /// <returns></returns>
    public IEnumerator DeckToCard(Card card)
    {

        StartCoroutine(MoveCard(card, 50, true, targetY:-1));
        for (int i = 0; i <= 50; i++)
        {
            MeshRenderer[] meshRenderer = card.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (var mesh in meshRenderer)
            {
                Color color = mesh.material.color;
                color.a -= 0.02F;
                mesh.material.color = color;

            }
            yield return null;
        }
    }

    /// <summary>
    /// カードを手札に加えるアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    /// <returns></returns>
    public IEnumerator CardToHand(Card card)
    {

        Vector3 startPosition = card.transform.localPosition;
        Vector3 endPosition = new Vector3(startPosition.x, startPosition.y - 1, startPosition.z);
        for (int i = 0; i < 20; i++)
        {
            MeshRenderer[] meshRenderer = card.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (var mesh in meshRenderer)
            {
                Color color = mesh.material.color;
                color.a += 0.05F;
                mesh.material.color = color;

            }
            Vector3 position = card.transform.localPosition;
            if (i == 0) position.y -= 0.1F;
            position.y += 0.005F;
            card.transform.localPosition = position;
            yield return null;
        }
    }

    /// <summary>
    /// フィールド上のカードをめくるアニメーション
    /// その場でめくるとフィールドにめり込むため少し浮かす
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    /// <returns></returns>
    public IEnumerator RotateFieldCard(Card card)
    {
        //List<Coroutine> parallel = new List<Coroutine>();

        //parallel.Add(StartCoroutine(RotateCard(card, 60)));
        //parallel.Add(StartCoroutine(MoveCard(card, 0.01F, 20)));

        //// 全てのコルーチンが終了するのを待機
        //foreach (var c in parallel)
        //    yield return c;

        StartCoroutine(RotateCard(card, 60));
        yield return MoveCard(card, 20, false, targetY:0.01F);
        yield return MoveCard(card, 40, false, targetY:-0.01F);
    }

    IEnumerator RotateCard(Card card, int frame)
    {
        for (int i = 0; i < frame; i++)
        {
            card.transform.Rotate(0, -3, 0);
            yield return null;
        }
    }

    /// <summary>
    /// カード直線移動させるアニメーション
    /// </summary>
    /// <param name="card">アニメ対象のカード</param>
    /// <param name="frame">アニメの総フレーム数</param>
    /// <param name="local">ローカル座標で計算する</param>
    /// <param name="targetX">X方向の移動量</param>
    /// <param name="targetY">Y方向の移動量</param>
    /// <param name="targetZ">Z方向の移動量</param>
    /// <returns></returns>
    IEnumerator MoveCard(Card card, int frame, bool local, float targetX = 0, float targetY = 0, float targetZ = 0)
    {
        Vector3 startPosition = local ? card.transform.localPosition : card.transform.position;
        Vector3 endPosition = new Vector3(startPosition.x + targetX, startPosition.y + targetY, startPosition.z + targetZ);
        for (int i = 0; i <= frame; i++)
        {
            if(local)
                card.transform.localPosition = Vector3.Lerp(startPosition, endPosition, (float)i / frame);
            else
                card.transform.position = Vector3.Lerp(startPosition, endPosition, (float)i / frame);
            //Debug.Log(i);
            yield return null;
        }
    }
}
