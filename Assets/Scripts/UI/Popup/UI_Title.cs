using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UI_Title : UI_Popup
{
    enum GameObjects
    {
        BackGround,

        StartButton,
        ContinueButton,
        GachaButton
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Sound.Play(Define.Sound.Bgm, "mainBGM");

        BindObject(typeof(GameObjects));

        AdjustUIByResolution();

        GetObject((int)GameObjects.StartButton).gameObject.BindEvent(OnStartButton);
        GetObject((int)GameObjects.ContinueButton).gameObject.BindEvent(OnContinueButton);
        GetObject((int)GameObjects.GachaButton).gameObject.BindEvent(OnGachaButton);

        return true;
    }

    public override void AdjustUIByResolution()
    {
        float scale = Screen.height / 2732.0f;
        GetObject((int)GameObjects.BackGround).transform.localScale *= scale / transform.localScale.x;
    }

    void OnStartButton()
    {
        Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

        if (File.Exists(Managers._savePath) == false)
        {
            Managers.UI.ClosePopupUI(this);

            Managers.Game.Init();
            Managers.UI.ShowPopupUI<UI_Game>().Tutorial();
        }
        else if (Managers.Game.LoadGame())
        {
            UI_Confirm confirm = Managers.UI.ShowPopupUI<UI_Confirm>();
            confirm.SetInfo("지난 기억을 잊고 새로 시작할까요?", () =>
            {
                Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

                Managers.UI.ClosePopupUI(confirm);
                Managers.UI.ClosePopupUI(this);

                Managers.Game.Init();
                Managers.UI.ShowPopupUI<UI_Game>().NewGame();
            });
        }
        else
        {
            Managers.UI.ClosePopupUI(this);

            Managers.Game.Init();
            Managers.UI.ShowPopupUI<UI_Game>().NewGame();
        }
    }

    void OnContinueButton()
    {
        Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

        if (File.Exists(Managers._savePath) == false)
        {
            Managers.UI.ClosePopupUI(this);

            Managers.Game.Init();
            Managers.UI.ShowPopupUI<UI_Game>().Tutorial();
        }
        else if (Managers.Game.LoadGame())
        {
            Managers.UI.ClosePopupUI(this);

            Managers.Game.Init();
            Managers.Game.LoadGame();
            Managers.UI.ShowPopupUI<UI_Game>().LoadGame();
        }
        else
        {
            Managers.UI.ClosePopupUI(this);

            Managers.Game.Init();
            Managers.UI.ShowPopupUI<UI_Game>().NewGame();
        }
    }

    void OnGachaButton()
    {
        Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

        UI_Confirm confirm = Managers.UI.ShowPopupUI<UI_Confirm>();
        confirm.SetInfo("세이브 파일을 삭제할까요?", () =>
        {
            Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

            string path = Application.persistentDataPath + "/SaveData.json";
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("SaveFile Deleted");
            }
            else
            {
                Debug.Log("No SaveFile Detected");
            }

            Managers.UI.ClosePopupUI(confirm);
        });

    }
}
