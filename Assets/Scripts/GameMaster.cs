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

    //[SerializeField] Animator animator;

    public void ResetScene()
    {
        // ���݂�Scene�����擾����
        Scene loadScene = SceneManager.GetActiveScene();
        // Scene�̓ǂݒ���
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
        deckGenerater = GetComponent<DeckGenerater>();
        //phase = Phase.INIT;
        deckGenerater.Generate(player1.deck);
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
        TextManager.Instance.SetPhaseText("����");

        StartCoroutine(player1.SetFirstVanguard());
        player1.deck.Shuffle();

        yield return new WaitUntil(() => Input.GetButtonDown("Enter"));

        for (int i = 0; i < 5; i++)
        {
            yield return StartCoroutine(player1.DrawCard());
            //yield return new WaitForSeconds(2f);

        }

        yield return new WaitUntil(() => Input.GetButtonDown("Enter"));

        yield return StartCoroutine(player1.StandUpVanguard());

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
        TextManager.Instance.SetPhaseText("�G���h�t�F�C�Y");


    }

    //Player currentPlayer;
    //Player waitPlayer;
    //void InitPhase()
    //{
    //    Debug.Log("InitPhase");

    //    // ���݂̃v���C���[
    //    //currentPlayer = player1;
    //    //waitPlayer = player1;

    //}

    IEnumerator StandPhase()
    {
        //phase = Phase.DRAW;

        TextManager.Instance.SetPhaseText("�X�^���h�t�F�C�Y");

        yield return new WaitUntil(() => Input.GetButtonDown("Enter"));
        yield return StartCoroutine(DrawPhase());
    }


    IEnumerator DrawPhase()
    {
        //Debug.Log("DrawPhase");
        TextManager.Instance.SetPhaseText("�h���[�t�F�C�Y");


        yield return new WaitUntil(() => Input.GetButtonDown("Enter"));
        yield return StartCoroutine(player1.DrawCard());
        //yield return new WaitUntil(() => Input.GetButtonDown("Enter"));
        //yield return StartCoroutine(StandPhase());



        // �J�[�h�̃h���[
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
        TextManager.Instance.SetPhaseText("���C�h�t�F�C�Y");

        yield return new WaitUntil(() => Input.GetButtonDown("Enter") && selectManager.SingleSelected());
        yield return null;

        yield return new WaitUntil(() => Input.GetButtonDown("Enter") && selectManager.SingleConfirm(Tag.Vanguard.ToString()));
        yield return null;

        yield return StartCoroutine(MainPhase());
    }

    IEnumerator MainPhase()
    {
        IEnumerator Call()
        {
            while (true)
            {
                if (Input.GetButtonDown("Enter") && selectManager.SingleConfirm(Tag.Rearguard.ToString()))
                    yield break;
                else if (Input.GetButtonDown("Cancel"))
                {
                    selectManager.SingleCansel();
                    yield break;
                }
                yield return null;
            }
        }

        TextManager.Instance.SetPhaseText("���C���t�F�C�Y");

        //Coroutine call = StartCoroutine(Call());


        //yield return new WaitUntil(() => CanNext && Input.GetButtonDown("Submit"));
        //StopCoroutine(call);

        while (true)
        {
            if (Input.GetButtonDown("Enter"))
            {
                if(selectManager.SingleSelected())
                    yield return StartCoroutine(Call());
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
    //    // ��D�̃J�[�h����ɏo��
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