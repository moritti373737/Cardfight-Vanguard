using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ネットワーク環境において相手を待つための処理を行うシングルトン
/// </summary>
public class NextController
{
    public bool local = true;

    [Flags]
    public enum Next
    {
        One = 1 << 0,    //2進数だと01　(10進数だと1)
        Two = 1 << 1,    //2進数だと10　(10進数だと2)
    }

    /// <summary>
    /// 何らかの処理を待ちながら行うための変数（2台がそれぞれ自由に使用）
    /// </summary>
    private Next ProcessNext { get; set; } = 0;

    /// <summary>
    /// フェイズを同期させるための変数（使用タイミングは厳密に規定）
    /// </summary>
    private Next SyncNext { get; set; } = 0;

    /// <summary>
    /// nextの値を返す、ただしtrueの場合はnextをfalseに書き換える
    /// </summary>
    /// <returns>nextの値</returns>
    //public bool IsNext()
    //{
    //    if (!singleNext) return false;
    //    singleNext = false;
    //    return true;
    //}

    /// <summary>
    /// nextのセッター
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
    /// 指定したnextの判定を行う
    /// </summary>
    /// <param name="judgeNext">調べたいnext</param>
    /// <returns>判定結果</returns>
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
