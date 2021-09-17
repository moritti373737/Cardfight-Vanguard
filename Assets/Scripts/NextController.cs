using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �l�b�g���[�N���ɂ����đ����҂��߂̏������s���V���O���g��
/// </summary>
public class NextController
{
    private bool singleNext = false;

    [Flags]
    public enum Next
    {
        One = 1 << 0,    //2�i������01�@(10�i������1)
        Two = 1 << 1,    //2�i������10�@(10�i������2)
    }

    public Next next { get; set; } = 0;

    /// <summary>
    /// next�̒l��Ԃ��A������true�̏ꍇ��next��false�ɏ���������
    /// </summary>
    /// <returns>next�̒l</returns>
    public bool IsNext()
    {
        if (!singleNext) return false;
        singleNext = false;
        return true;
    }

    /// <summary>
    /// next�̃Z�b�^�[
    /// </summary>
    public void SetNext(bool next) => singleNext = next;

    public void SetNext(Next newNext, bool b)
    {
        if (b) next |= newNext;
        else next &= ~newNext;
    }

    public void SetNext(int nextInt, bool b)
    {
        Next newNext = nextInt == 1 ? Next.One : Next.Two;
        if (b) next |= newNext;
        else next &= ~newNext;
    }

    /// <summary>
    /// �w�肵��next�̔�����s��
    /// </summary>
    /// <param name="judgeNext">���ׂ���next</param>
    /// <returns>���茋��</returns>
    public bool JudgeNext(Next judgeNext) => next == (next | judgeNext);

    public bool JudgeAllNext()
    {
        var ret = next == (next | Next.One | Next.Two);
        if (!ret) return false;
        next = 0;
        return true;
    }

    private static NextController _singleInstance = new NextController();

    public static NextController Instance { get => _singleInstance; }

    private NextController() { }

    //public static NextController GetInstance()
    //{
    //    return _singleInstance;
    //}

}
