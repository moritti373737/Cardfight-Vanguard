using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField]
    private PlayerInput input;

    [SerializeField]
    private PhotonConnection photonConnection;

    private FightMode fightMode;
    private bool local = true;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        //photonConnection.StartConnection();
    }

    private void SceneManager_sceneLoaded(Scene next, LoadSceneMode mode)
    {
        print("loaded");
        GameMaster.Instance.fightMode = fightMode;
        GameMaster.Instance.local = local;
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

    }

    // Update is called once per frame
    void Update()
    {
        //if(input.GetDown("Enter")) SceneManager.LoadScene("MainScene");
    }

    public void OnButtonClick()
    {
        //有効なイベントシステムを取得
        var eventSystem = EventSystem.current;

        //押されたボタンのオブジェクトをイベントシステムのcurrentSelectedGameObject関数から取得
        var button_ob = eventSystem.currentSelectedGameObject;

        //ボタンの子のテキストオブジェクトを名前指定で取得 この場合Textと名前が付いているテキストオブジェクトを探す
        //var NameText_ob = button_ob.transform.Find("Text").gameObject;

        ////テキストオブジェクトのテキストを取得
        //var name_text = NameText_ob.GetComponent<Text>();

        ////テキストを変更
        //name_text.text = "変更後の文字列";

        Debug.Log(button_ob.GetComponentInChildren<Text>().text);
        if (button_ob.name == "Button1")
        {
            fightMode = FightMode.PVP;
            local = true;
            SceneManager.LoadScene("MainScene");
        }
        else if (button_ob.name == "Button2")
        {
            fightMode = FightMode.PVE;
            local = true;
            SceneManager.LoadScene("MainScene");
        }
        else if (button_ob.name == "Button3")
        {
            fightMode = FightMode.EVE;
            local = true;
            SceneManager.LoadScene("MainScene");
        }
        else if (button_ob.name == "Button4")
        {
            fightMode = FightMode.PVP;
            local = false;
            photonConnection.StartConnection();
        }
    }
}
