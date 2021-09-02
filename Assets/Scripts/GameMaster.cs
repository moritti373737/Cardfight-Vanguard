using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    //public Player[] playerList;
    public Fighter fighter1;
    public Fighter fighter2;
    public SelectManager selectManager;


    private Fighter AttackFighter;
    private Fighter DefenceFighter;

    //[SerializeField] Animator animator;

    public void ResetScene()
    {
        // 現在のScene名を取得する
        Scene loadScene = SceneManager.GetActiveScene();
        // Sceneの読み直し
        SceneManager.LoadScene(loadScene.name);

        Debug.Log("RESET!");
    }

    private enum Phase
    {
        INIT,
        STAND,
        DRAW,
        BATTLE,
        END,
    };

    //Phase phase;


    void Start()
    {
        //phase = Phase.INIT;
        fighter1.CreateDeck();
        fighter2.CreateDeck();
        StartCoroutine(InitPhase());
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Return))
        {
            ResetScene();
        }


        //switch (phase)
        //{
        //    case Phase.INIT:
        //        //InitPhase();
        //        break;
        //    case Phase.STAND:
        //        DrawPhase();
        //        break;
        //    case Phase.DRAW:
        //        StandPhase();
        //        break;
        //    case Phase.BATTLE:
        //        BattlePhase();
        //        break;
        //    case Phase.END:
        //        EndPhase();
        //        break;
        //}
    }

    private IEnumerator InitPhase()
    {
        TextManager.Instance.SetPhaseText("準備");

        AttackFighter = fighter1;
        DefenceFighter = fighter2;

        StartCoroutine(fighter1.SetFirstVanguard());
        fighter1.deck.Shuffle();

        yield return new WaitUntil(() => Input.GetButtonDown("Enter"));

        for (int i = 0; i < 5; i++)
        {
            yield return StartCoroutine(fighter1.DrawCard());
            //yield return new WaitForSeconds(2f);

        }

        yield return new WaitUntil(() => Input.GetButtonDown("Enter"));

        yield return StartCoroutine(fighter1.StandUpVanguard());

        yield return null;
        //while (true)
        //{
        //yield return new WaitUntil(() => Input.GetButtonDown("Enter"));
        //yield return new WaitUntil(() => Input.GetButtonDown("Enter") && selectManager.SingleSelected());
        //selectManager.SingleSelected();
        //yield return null;

        //yield return new WaitUntil(() => Input.GetButtonDown("Enter") && selectManager.SingleConfirm("Vanguard"));
        //selectManager.SingleConfirm();
        //yield return null;
        //}

        yield return StartCoroutine(StandPhase());
        //phase = Phase.DRAW;
        TextManager.Instance.SetPhaseText("エンドフェイズ");


    }

    //Player currentPlayer;
    //Player waitPlayer;
    //void InitPhase()
    //{
    //    Debug.Log("InitPhase");

    //    // 現在のプレイヤー
    //    //currentPlayer = player1;
    //    //waitPlayer = player1;

    //}

    IEnumerator StandPhase()
    {
        //phase = Phase.DRAW;

        TextManager.Instance.SetPhaseText("スタンドフェイズ");

        yield return new WaitUntil(() => Input.GetButtonDown("Enter"));
        yield return StartCoroutine(DrawPhase());
    }


    IEnumerator DrawPhase()
    {
        //Debug.Log("DrawPhase");
        TextManager.Instance.SetPhaseText("ドローフェイズ");


        // カードのドロー
        yield return new WaitUntil(() => Input.GetButtonDown("Enter"));
        yield return StartCoroutine(AttackFighter.DrawCard());
        //yield return new WaitUntil(() => Input.GetButtonDown("Enter"));
        //yield return StartCoroutine(StandPhase());



        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //phase = Phase.STANDBY;
            //currentPlayer.CardDraw();
        }
        /*
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("EndState"))
        {
            currentPlayer.CardDraw();
            //phase = Phase.STANDBY;
        }*/

        //phase = Phase.STANDBY;
        yield return StartCoroutine(RidePhase());
    }

    IEnumerator RidePhase()
    {
        TextManager.Instance.SetPhaseText("ライドフェイズ");

        yield return new WaitUntil(() => Input.GetButtonDown("Enter") && selectManager.SingleSelected(Tag.Hand, AttackFighter.ID));
        yield return null;

        yield return new WaitUntil(() => Input.GetButtonDown("Enter") && selectManager.SingleConfirm(Tag.Vanguard));
        yield return null;

        yield return StartCoroutine(MainPhase());
    }

    IEnumerator MainPhase()
    {
        IEnumerator Call()
        {
            yield return null;
            while (true)
            {
                if (Input.GetButtonDown("Enter") && selectManager.SingleConfirm(Tag.Rearguard))
                    yield break;
                else if (Input.GetButtonDown("Cancel"))
                {
                    selectManager.SingleCansel();
                    yield break;
                }
                yield return null;
            }
        }

        IEnumerator Move()
        {
            yield return null;
            while (true)
            {
                if (Input.GetButtonDown("Enter") && selectManager.SingleConfirm(Tag.Rearguard))
                    yield break;
                else if (Input.GetButtonDown("Cancel"))
                {
                    selectManager.SingleCansel();
                    yield break;
                }
                yield return null;
            }
        }

        TextManager.Instance.SetPhaseText("メインフェイズ");

        //Coroutine call = StartCoroutine(Call());


        //yield return new WaitUntil(() => CanNext && Input.GetButtonDown("Submit"));
        //StopCoroutine(call);

        while (true)
        {
            if (Input.GetButtonDown("Enter"))
            {
                if(selectManager.SingleSelected(Tag.Hand, AttackFighter.ID))
                    yield return StartCoroutine(Call());
                else if (selectManager.SingleSelected(Tag.Rearguard, AttackFighter.ID))
                    yield return StartCoroutine(Move());
            }
            if (Input.GetButtonDown("Submit"))
            {
                break;

            }
            yield return null;
        }

        yield return null;

    }

    //void StandbyPhase()
    //{
    //    Debug.Log("StandbyPhase");
    //    // 手札のカードを場に出す
    //    //currentPlayer.StandbyPhaseAction();
    //    phase = Phase.BATTLE;
    //}
    /*
    void BattlePhase()
    {
        Debug.Log("BattlePhase");

        currentPlayer.ButtlePhaseAction(waitPlayer);
        phase = Phase.END;
    }

    void CheckFieldCardHP()
    {
        currentPlayer.CheckFieldCardHP();
        waitPlayer.CheckFieldCardHP();
    }
    void EndPhase()
    {
        Debug.Log("EndPhase");
        (currentPlayer, waitPlayer) = (waitPlayer, currentPlayer);
        //phase = Phase.DRAW;
    }*/
}