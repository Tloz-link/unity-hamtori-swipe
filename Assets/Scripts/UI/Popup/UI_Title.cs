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

        BindObject(typeof(GameObjects));

        GetObject((int)GameObjects.StartButton).gameObject.BindEvent(OnStartButton);
        GetObject((int)GameObjects.ContinueButton).gameObject.BindEvent(OnContinueButton);
        GetObject((int)GameObjects.GachaButton).gameObject.BindEvent(OnGachaButton);

        return true;
    }

    void OnStartButton()
    {
        if (Managers.Game.LoadGame())
        {
            UI_Confirm confirm = Managers.UI.ShowPopupUI<UI_Confirm>();
            confirm.SetInfo("세이브 파일이 이미 존재합니다. \n새로 시작하시겠습니까?", () =>
            {
                Managers.UI.ClosePopupUI(this);

                Managers.Game.Init();
                Managers.UI.ShowPopupUI<UI_Game>().NewGame();
                Managers.UI.ShowPopupUI<UI_Tutorial>();
            });
        }
        else
        {
            Managers.UI.ClosePopupUI(this);

            Managers.Game.Init();
            Managers.UI.ShowPopupUI<UI_Game>().NewGame();
            Managers.UI.ShowPopupUI<UI_Tutorial>();
        }
    }

    void OnContinueButton()
    {
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
            Managers.UI.ShowPopupUI<UI_Tutorial>();
        }
    }

    void OnGachaButton()
    {
        Managers.UI.ShowPopupUI<UI_Gacha>();
    }
}
