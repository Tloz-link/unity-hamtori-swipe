using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UI_Title : UI_Popup
{
    enum GameObjects
    {
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

        GetObject((int)GameObjects.StartButton).gameObject.BindEvent(OnStartButton);
        GetObject((int)GameObjects.ContinueButton).gameObject.BindEvent(OnContinueButton);
        GetObject((int)GameObjects.GachaButton).gameObject.BindEvent(OnGachaButton);

        return true;
    }

    void OnStartButton()
    {
        Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

        if (Managers.Game.LoadGame())
        {
            UI_Confirm confirm = Managers.UI.ShowPopupUI<UI_Confirm>();
            confirm.SetInfo("지난 기억을 잊고 새로 시작할까요?", () =>
            {
                Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

                Managers.UI.ClosePopupUI(confirm);
                Managers.UI.ClosePopupUI(this);

                Managers.Game.Init();
                Managers.UI.ShowPopupUI<UI_Game>().NewGame();
                //Managers.UI.ShowPopupUI<UI_Tutorial>();
            });
        }
        else
        {
            Managers.UI.ClosePopupUI(this);

            Managers.Game.Init();
            Managers.UI.ShowPopupUI<UI_Game>().NewGame();
            //Managers.UI.ShowPopupUI<UI_Tutorial>();
        }
    }

    void OnContinueButton()
    {
        Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

        if (Managers.Game.LoadGame())
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
            //Managers.UI.ShowPopupUI<UI_Tutorial>();
        }
    }

    void OnGachaButton()
    {
        Managers.UI.ShowPopupUI<UI_Gacha>();
    }
}
