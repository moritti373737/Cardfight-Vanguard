using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : SingletonMonoBehaviour<AnimationManager>
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator DeckToCard(Card card)
    {

        Vector3 startPosition = card.transform.localPosition;
        Vector3 endPosition = new Vector3(startPosition.x, startPosition.y - 1, startPosition.z);
        for (int i = 0; i <= 50; i++)
        {
            MeshRenderer[] meshRenderer = card.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (var mesh in meshRenderer)
            {
                Color color = mesh.material.color;
                color.a -= 0.02F;
                mesh.material.color = color;

            }
            //Vector3 position = card.transform.localPosition;
            //position.y -= 0.01F;
            card.transform.localPosition = Vector3.Lerp(startPosition, endPosition, i / 50F);
            yield return null;
        }
    }

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

    public IEnumerator RotateFieldCard(Card card)
    {
        //List<Coroutine> parallel = new List<Coroutine>();

        //parallel.Add(StartCoroutine(RotateCard(card, 60)));
        //parallel.Add(StartCoroutine(MoveCard(card, 0.01F, 20)));

        //// 全てのコルーチンが終了するのを待機
        //foreach (var c in parallel)
        //    yield return c;

        StartCoroutine(RotateCard(card, 60));
        yield return MoveCard(card, 0.01F, 20);
        yield return MoveCard(card, -0.01F, 40);
    }

    IEnumerator RotateCard(Card card, int frame)
    {
        for (int i = 0; i < frame; i++)
        {
            card.transform.Rotate(0, -3, 0);
            yield return null;
        }
    }

    IEnumerator MoveCard(Card card, float target, int frame)
    {
        Vector3 startPosition = card.transform.position;
        Vector3 endPosition = new Vector3(startPosition.x, startPosition.y + target, startPosition.z);
        for (int i = 0; i <= frame; i++)
        {
            card.transform.position = Vector3.Lerp(startPosition, endPosition, (float)i / frame);
            Debug.Log(i);
            yield return null;
        }
    }
}
