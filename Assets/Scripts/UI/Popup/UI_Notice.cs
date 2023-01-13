using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Notice : UI_Popup
{
    enum Texts
    {
        NoticeText
    }

    enum Buttons
    {
        CorrectButton
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        return true;
    }

    public void SetInfo(string message, Action correctCallBack)
    {
        Init();
        GetText((int)Texts.NoticeText).text = message;
        GetButton((int)Buttons.CorrectButton).gameObject.BindEvent(correctCallBack);
    }
}
