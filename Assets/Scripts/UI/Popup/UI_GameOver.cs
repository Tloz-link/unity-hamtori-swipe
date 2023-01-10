using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GameOver : UI_Popup
{
    enum GameObjects
    {
        Panel
    }

    enum Buttons
    {
        RestartButton
    }

    Action _onRestartCallBack;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        return true;
    }

    public void SetInfo(Vector3 hamsterPos, Action onRestartCallBack)
    {
        Init();

        UI_Hamster hamster = Managers.UI.makeSubItem<UI_Hamster>(GetObject((int)GameObjects.Panel).transform);
        hamster.transform.position = hamsterPos;
        hamster.PlayAnimation(Managers.Data.Spine.hamsterGameover);

        _onRestartCallBack = onRestartCallBack;
        GetButton((int)Buttons.RestartButton).gameObject.BindEvent(onRestartCallBack);
    }
}
