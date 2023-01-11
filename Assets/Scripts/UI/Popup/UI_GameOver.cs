using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GameOver : UI_Popup
{
    enum GameObjects
    {
        Hamster
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

        GetObject((int)GameObjects.Hamster).transform.position = hamsterPos;
        GetObject((int)GameObjects.Hamster).GetOrAddComponent<UI_Spine>().PlayAnimation(Managers.Data.Spine.hamsterGameover);

        _onRestartCallBack = onRestartCallBack;
        GetButton((int)Buttons.RestartButton).gameObject.BindEvent(onRestartCallBack);
    }
}
