using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// �A�j���[�V�����i�ړ��ⓧ�߂Ȃǁj�̏����S��
/// </summary>
public class AnimationManager : SingletonMonoBehaviour<AnimationManager>
{
    /// <summary>
    /// �f�b�L����J�[�h�������A�j���[�V����
    /// </summary>
    /// <param name="card">�A�j���Ώۂ̃J�[�h</param>
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
    /// �J�[�h����D�ɉ�����A�j���[�V����
    /// </summary>
    /// <param name="card">�A�j���Ώۂ̃J�[�h</param>
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
    /// �t�B�[���h��̃J�[�h���߂���A�j���[�V����
    /// ���̏�ł߂���ƃt�B�[���h�ɂ߂荞�ނ��ߏ���������
    /// </summary>
    /// <param name="card">�A�j���Ώۂ̃J�[�h</param>
    /// <returns></returns>
    public IEnumerator RotateFieldCard(Card card)
    {
        //List<Coroutine> parallel = new List<Coroutine>();

        //parallel.Add(StartCoroutine(RotateCard(card, 60)));
        //parallel.Add(StartCoroutine(MoveCard(card, 0.01F, 20)));

        //// �S�ẴR���[�`�����I������̂�ҋ@
        //foreach (var c in parallel)
        //    yield return c;

        StartCoroutine(RotateCard(card, 60));
        yield return MoveCard(card, 20, false, targetY:0.01F);
        yield return MoveCard(card, 40, false, targetY:-0.01F);
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
            card.transform.localRotation = Quaternion.Slerp(q1, q2, (float)i / frame); // ���`���
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
            card.transform.localRotation = Quaternion.Slerp(q1, q2, (float)i / frame); // ���`���
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
            card.transform.rotation = Quaternion.Slerp(q1, q2, i / 10.0F); // ���`���
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
            card.transform.rotation = Quaternion.Slerp(q2, q3, i / 10.0F); // ���`���
            //card.transform.localScale = scale;
            card.transform.position = Vector3.Slerp(startPosition, endPosition, (float)i / 10);
            await UniTask.NextFrame();
        }
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
    /// �J�[�h�����ړ�������A�j���[�V����
    /// </summary>
    /// <param name="card">�A�j���Ώۂ̃J�[�h</param>
    /// <param name="frame">�A�j���̑��t���[����</param>
    /// <param name="local">���[�J�����W�Ōv�Z����Afalse�̏ꍇ�̓O���[�o�����W��p����</param>
    /// <param name="targetX">X�����̈ړ���</param>
    /// <param name="targetY">Y�����̈ړ���</param>
    /// <param name="targetZ">Z�����̈ړ���</param>
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
