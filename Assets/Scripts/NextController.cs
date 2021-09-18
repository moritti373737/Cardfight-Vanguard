using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �l�b�g���[�N���ɂ����đ����҂��߂̏������s���V���O���g��
/// </summary>
public class NextController
{
    public bool local = true;

    [Flags]
    public enum Next
    {
        One = 1 << 0,    //2�i������01�@(10�i������1)
        Two = 1 << 1,    //2�i������10�@(10�i������2)
    }

    /// <summary>
    /// ���炩�̏�����҂��Ȃ���s�����߂̕ϐ��i2�䂪���ꂼ�ꎩ�R�Ɏg�p�j
    /// </summary>
    private Next ProcessNext { get; set; } = 0;

    /// <summary>
    /// �t�F�C�Y�𓯊������邽�߂̕ϐ��i�g�p�^�C�~���O�͌����ɋK��j
    /// </summary>
    private Next SyncNext { get; set; } = 0;

    /// <summary>
    /// next�̒l��Ԃ��A������true�̏ꍇ��next��false�ɏ���������
    /// </summary>
    /// <returns>next�̒l</returns>
    //public bool IsNext()
    //{
    //    if (!singleNext) return false;
    //    singleNext = false;
    //    return true;
    //}

    /// <summary>
    /// next�̃Z�b�^�[
    /// </summary>
    //public void SetNext(bool next) => singleNext = next;
    //private void SetNext(Next newNext, Next next, bool b)
    //{
    //    if (b) next |= newNext;
    //    else next &= ~newNext;
    //}

    public void SetProcessNext(Next newNext, bool b)
    {
        if (b) ProcessNext |= newNext;
        else ProcessNext &= ~newNext;
    }

    public void SetProcessNext(int newNextInt, bool b)
    {
        SetProcessNext(newNextInt == 0 ? Next.One : Next.Two, b);
    }

    public void SetSyncNext(Next newNext, bool b)
    {
        if (b) SyncNext |= newNext;
        else SyncNext &= ~newNext;
    }

    public void SetSyncNext(int newNextInt, bool b)
    {
        SetSyncNext(newNextInt == 0 ? Next.One : Next.Two, b);
    }

    /// <summary>
    /// �w�肵��next�̔�����s��
    /// </summary>
    /// <param name="judgeNext">���ׂ���next</param>
    /// <returns>���茋��</returns>
    public bool JudgeProcessNext(Next judgeNext)
    {
        if (!(ProcessNext == (ProcessNext | judgeNext))) return false;
        SetProcessNext(judgeNext, false);
        return true;
    }

    public bool JudgeProcessNext(int newNextInt)
    {
        Next newNext = newNextInt == 0 ? Next.One : Next.Two;
        return JudgeProcessNext(newNext);
    }

    //public bool JudgeAllNext()
    //{
    //    var ret = processNext == (processNext | Next.One | Next.Two);
    //    if (!ret) return false;
    //    processNext = 0;
    //    return true;
    //}

    public bool JudgeAllProcessNext()
    {
        bool ret = ProcessNext == (ProcessNext | Next.One | Next.Two);
        if (!ret) return false;
        ProcessNext = 0;
        return true;
    }

    public bool JudgeAllSyncNext()
    {
        if (local) return true;
        bool ret = SyncNext == (SyncNext | Next.One | Next.Two);
        if (!ret) return false;
        SyncNext = 0;
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
