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
        //�L���ȃC�x���g�V�X�e�����擾
        var eventSystem = EventSystem.current;

        //�����ꂽ�{�^���̃I�u�W�F�N�g���C�x���g�V�X�e����currentSelectedGameObject�֐�����擾
        var button_ob = eventSystem.currentSelectedGameObject;

        //�{�^���̎q�̃e�L�X�g�I�u�W�F�N�g�𖼑O�w��Ŏ擾 ���̏ꍇText�Ɩ��O���t���Ă���e�L�X�g�I�u�W�F�N�g��T��
        //var NameText_ob = button_ob.transform.Find("Text").gameObject;

        ////�e�L�X�g�I�u�W�F�N�g�̃e�L�X�g���擾
        //var name_text = NameText_ob.GetComponent<Text>();

        ////�e�L�X�g��ύX
        //name_text.text = "�ύX��̕�����";

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
