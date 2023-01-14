using System.Collections;
using System.Collections.Generic;
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
        Managers.UI.ClosePopupUI(this);
        Managers.UI.ShowPopupUI<UI_Game>();
        Managers.UI.ShowPopupUI<UI_Tutorial>();
    }

    void OnContinueButton()
    {
        UI_Notice notice = Managers.UI.ShowPopupUI<UI_Notice>();
        notice.SetInfo("아직 세이브 로드 구현이 안 됐습니다. ㅠㅠ", () =>
        {
            Managers.UI.ClosePopupUI(notice);
        });
    }

    void OnGachaButton()
    {
        Managers.UI.ShowPopupUI<UI_Gacha>();
    }
}
