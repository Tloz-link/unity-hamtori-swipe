using DG.Tweening;
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

        Sequence open = Utils.MakePopupOpenSequence(GetObject((int)GameObjects.Image));
        open.SetUpdate(true);
        open.OnComplete(() =>
        {
            GetObject((int)GameObjects.CanselButton).gameObject.BindEvent(OnCanselButton);
            GetObject((int)GameObjects.BackGround).gameObject.BindEvent(OnCanselButton);
            GetObject((int)GameObjects.Panel).gameObject.BindEvent(OnCanselButton);
        });
        open.Restart();

        return true;
    }

    void OnCanselButton()
    {
        Destroy(GetObject((int)GameObjects.Panel).GetComponent<UI_EventHandler>());
        Destroy(GetObject((int)GameObjects.CanselButton).GetComponent<UI_EventHandler>());
        Destroy(GetObject((int)GameObjects.BackGround).GetComponent<UI_EventHandler>());

        Sequence close = Utils.MakePopupCloseSequence(GetObject((int)GameObjects.Image));
        close.SetUpdate(true);
        close.OnComplete(() =>
        {
            Managers.UI.ClosePopupUI(this);
            Time.timeScale = 1;
        });
    }
}
