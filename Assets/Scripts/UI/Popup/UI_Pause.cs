using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Pause : UI_Popup
{
    enum GameObjects
    {
        Image,
        CanselButton,
        BackGround,
        Panel
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));

        GetObject((int)GameObjects.CanselButton).gameObject.BindEvent(OnCanselButton);
        GetObject((int)GameObjects.BackGround).gameObject.BindEvent(OnCanselButton);
        GetObject((int)GameObjects.Panel).gameObject.BindEvent(OnCanselButton);

        return true;
    }

    void OnCanselButton()
    {
        Managers.UI.ClosePopupUI(this);
        Time.timeScale = 1;
    }
}
