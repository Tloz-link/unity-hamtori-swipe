using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GameOver : UI_Popup
{
    enum Buttons
    {
        RestartButton
    }

    Action _onRestartCallBack;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButton(typeof(Buttons));
        return true;
    }

    public void SetInfo(Action onRestartCallBack)
    {
        Init();

        _onRestartCallBack = onRestartCallBack;
        GetButton((int)Buttons.RestartButton).gameObject.BindEvent(onRestartCallBack);
    }
}
