using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Pause : UI_Popup
{
    enum GameObjects
    {
        bg,
        CanselButton,
        VibrationButton,
        SoundButton,
        SoundLeftButton,
        SoundRightButton,
        MainScreenButton
    }

    enum Images
    {
        SoundImage,
        SoundIStack1,
        SoundIStack2,
        SoundIStack3,
        SoundIStack4,
        SoundIStack5,
        SoundIStack6,
        VibrationImage
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));

        Sequence open = Utils.MakePopupOpenSequence(GetObject((int)GameObjects.bg));
        open.SetUpdate(true);
        open.OnComplete(() =>
        {
            GetObject((int)GameObjects.CanselButton).gameObject.BindEvent(OnCanselButton);
            GetObject((int)GameObjects.MainScreenButton).gameObject.BindEvent(OnMainScreenButton);
        });
        open.Restart();

        return true;
    }

    void OnCanselButton()
    {
        Destroy(GetObject((int)GameObjects.CanselButton).GetComponent<UI_EventHandler>());
        Destroy(GetObject((int)GameObjects.MainScreenButton).GetComponent<UI_EventHandler>());
        Managers.Sound.Play(Define.Sound.Effect, "popup");

        Sequence close = Utils.MakePopupCloseSequence(GetObject((int)GameObjects.bg));
        close.SetUpdate(true);
        close.OnComplete(() =>
        {
            Managers.UI.ClosePopupUI(this);
            Time.timeScale = 1;
        });
    }

    void OnMainScreenButton()
    {
        Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

        UI_Confirm confirm = Managers.UI.ShowPopupUI<UI_Confirm>();
        confirm.SetInfo("메인 화면으로 돌아가시겠습니까?", () =>
        {
            Managers.Sound.Play(Define.Sound.Effect, "uiTouch");
            Managers.UI.CloseAllPopupUI();

            Time.timeScale = 1;
            DOTween.KillAll();
            Managers.UI.ShowPopupUI<UI_Title>();
        });
    }
}
