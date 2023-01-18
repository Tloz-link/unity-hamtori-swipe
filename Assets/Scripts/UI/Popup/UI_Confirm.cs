using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Confirm : UI_Popup
{
    enum GameObjects
    {
        ConfirmButton,
        CanselButton,
        Rect
    }

    enum Texts
    {
        Text
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));

        Sequence open = Utils.MakePopupOpenSequence(GetObject((int)GameObjects.Rect));
        open.OnComplete(() =>
        {
            GetObject((int)GameObjects.CanselButton).gameObject.BindEvent(OnCanselButton);
        });
        open.Restart();

        return true;
    }

    public void SetInfo(string message, Action correctCallBack)
    {
        Init();
        GetText((int)Texts.Text).text = message;
        GetObject((int)GameObjects.ConfirmButton).BindEvent(correctCallBack);
    }

    void OnCanselButton()
    {
        Destroy(GetObject((int)GameObjects.ConfirmButton).GetComponent<UI_EventHandler>());
        Destroy(GetObject((int)GameObjects.CanselButton).GetComponent<UI_EventHandler>());

        Sequence close = Utils.MakePopupCloseSequence(GetObject((int)GameObjects.Rect));
        close.OnComplete(() =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
}
