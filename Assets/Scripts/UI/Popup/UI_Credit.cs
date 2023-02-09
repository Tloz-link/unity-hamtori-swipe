using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Credit : UI_Popup
{
    enum GameObjects
    {
        bg,
        CanselButton,
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));

        Sequence open = Utils.MakePopupOpenSequence(GetObject((int)GameObjects.bg));
        open.SetUpdate(true);
        open.OnComplete(() =>
        {
            GetObject((int)GameObjects.CanselButton).BindEvent(OnCanselButton);
        });
        open.Restart();

        return true;
    }

    void OnCanselButton()
    {
        Destroy(GetObject((int)GameObjects.CanselButton).GetComponent<UI_EventHandler>());

        Managers.Sound.Play(Define.Sound.Effect, "popup");

        Sequence close = Utils.MakePopupCloseSequence(GetObject((int)GameObjects.bg));
        close.SetUpdate(true);
        close.OnComplete(() =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
}
