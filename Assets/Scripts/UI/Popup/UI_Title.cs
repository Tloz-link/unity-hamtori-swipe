using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UI_Title : UI_Popup
{
    enum GameObjects
    {
        BackGround,

        PressAnyButton,
        Touch,
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Sound.Play(Define.Sound.Bgm, "mainBGM");

        BindObject(typeof(GameObjects));

        AdjustUIByResolution();

        GetObject((int)GameObjects.Touch).gameObject.BindEvent(OnStartButton);

        return true;
    }

    public override void AdjustUIByResolution()
    {
        float scale = Screen.height / 2300.0f;
        GetObject((int)GameObjects.BackGround).transform.localScale *= scale / transform.localScale.x;
    }

    void OnStartButton()
    {
        Managers.Sound.Play(Define.Sound.Effect, "uiTouch");
        GetObject((int)GameObjects.PressAnyButton).SetActive(false);

        if (File.Exists(Managers._savePath) == false)
        {
            Managers.UI.CloseAllPopupUI();

            Managers.Game.Init();
            Managers.UI.ShowPopupUI<UI_Game>().Tutorial();
        }
        else
        {
            UI_Confirm confirm = Managers.UI.ShowPopupUI<UI_Confirm>();
            confirm.SetInfo("이어서 시작할까요?", () =>
            {
                Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

                Managers.UI.CloseAllPopupUI();

                Managers.Game.Init();
                if (Managers.Game.LoadGame())
                    Managers.UI.ShowPopupUI<UI_Game>().LoadGame();
                else
                {
                    Managers.Game.Init();
                    Managers.UI.ShowPopupUI<UI_Game>().NewGame();
                }
                
            }, () =>
            {
                Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

                UI_Confirm caution = Managers.UI.ShowPopupUI<UI_Confirm>();
                caution.SetInfo("기억이 사라져요. 정말 괜찮아요?!", () =>
                {
                    Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

                    Managers.UI.CloseAllPopupUI();
                    Managers.Game.Init();
                    Managers.UI.ShowPopupUI<UI_Game>().NewGame();
                }, null, true);
            });
        }
    }
}
