using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Tutorial : UI_Popup
{
    enum GameObjects
    {
        Tutorial1
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        GetObject((int)GameObjects.Tutorial1).gameObject.BindEvent(OnClickTutorial);

        return true;
    }

    void OnClickTutorial()
    {
        Managers.UI.ClosePopupUI(this);
    }
}
