using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextController
{
    private bool singleNext = false;

    [Flags]
    public enum Next
    {
        One = 1 << 0,    //2進数だと01　(10進数だと1)
        Two = 1 << 1,    //2進数だと10　(10進数だと2)
    }

    public Next next { get; set; } = 0;

    public bool IsNext()
    {
        if (!singleNext) return false;
        singleNext = false;
        return true;
    }

    public bool SetNext(bool next) => singleNext = next;

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
