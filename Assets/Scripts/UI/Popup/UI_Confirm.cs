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

    Action _rejectCallback = null;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));

        Sequence open = Utils.MakePopupOpenSequence(GetObject((int)GameObjects.Rect));
        open.SetUpdate(true);
        open.OnComplete(() =>
        {
            GetObject((int)GameObjects.CanselButton).gameObject.BindEvent(OnCanselButton);
        });
        open.Restart();

        return true;
    }

    public void SetInfo(string message, Action correctCallback, Action rejectCallback = null, bool caution = false)
    {
        Init();
        GetText((int)Texts.Text).text = message;
        GetObject((int)GameObjects.ConfirmButton).BindEvent(correctCallback);

        if (caution)
            GetText((int)Texts.Text).color = new Color(0.6196079f, 0.1176471f, 0.1176471f);

        _rejectCallback = rejectCallback;
    }

    void OnCanselButton()
    {
        if (_rejectCallback != null)
        {
            _rejectCallback.Invoke();
            return;
        }

        Managers.Sound.Play(Define.Sound.Effect, "uiTouch");
        Destroy(GetObject((int)GameObjects.ConfirmButton).GetComponent<UI_EventHandler>());
        Destroy(GetObject((int)GameObjects.CanselButton).GetComponent<UI_EventHandler>());

        Sequence close = Utils.MakePopupCloseSequence(GetObject((int)GameObjects.Rect));
        close.SetUpdate(true);
        close.OnComplete(() =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
}
