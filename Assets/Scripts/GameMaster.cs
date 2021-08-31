using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    //public Player[] playerList;
    public Player player1;
    public DeckGenerater deckGenerater;
    public SelectManager selectManager;

    [SerializeField] Animator animator;

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

    Phase phase;


    void Start()
    {
        deckGenerater = GetComponent<DeckGenerater>();
        phase = Phase.INIT;
        deckGenerater.Generate(player1.deck);
        StartCoroutine(InitPhase());
    }

    void Update()
    {
        if (Input.GetButtonDown("Enter"))
        {

            Debug.Log("Enter");
        }

        if (Input.GetKey(KeyCode.Return))
        {
            SceneManager.LoadScene("MainScene");
        }


        switch (phase)
        {
            case Phase.INIT:
                //InitPhase();
                break;
            case Phase.STAND:
                DrawPhase();
                break;
            case Phase.DRAW:
                StandPhase();
                break;
                /*
            case Phase.BATTLE:
                BattlePhase();
                break;
            case Phase.END:
                EndPhase();
                break;
                */
        }
    }

    private IEnumerator InitPhase()
    {
        TextManager.Instance.SetPhaseText("準備");

        StartCoroutine(player1.SetFirstVanguard());
        player1.deck.Shuffle();

        yield return new WaitUntil(() => Input.GetButtonDown("Enter"));

        for (int i = 0; i < 5; i++)
        {
            yield return StartCoroutine(player1.DrawCard());
            //yield return new WaitForSeconds(2f);

        }

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


        yield return new WaitUntil(() => Input.GetButtonDown("Enter"));
        yield return StartCoroutine(player1.DrawCard());
        //yield return new WaitUntil(() => Input.GetButtonDown("Enter"));
        //yield return StartCoroutine(StandPhase());



        // カードのドロー
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

        yield return new WaitUntil(() => Input.GetButtonDown("Enter") && selectManager.SingleSelected());
        yield return null;

        yield return new WaitUntil(() => Input.GetButtonDown("Enter") && selectManager.SingleConfirm("Vanguard"));
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