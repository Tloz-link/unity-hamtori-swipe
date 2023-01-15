using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Gacha : UI_Popup
{
    enum GameObjects
    {
        GachaButton,
        CanselButton,
        Rect
    }

    enum Images
    {
        ResultImage
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));

        Sequence open = Utils.MakePopupOpenSequence(GetObject((int)GameObjects.Rect));
        open.OnComplete(() =>
        {
            GetObject((int)GameObjects.GachaButton).gameObject.BindEvent(OnGachaButton);
            GetObject((int)GameObjects.CanselButton).gameObject.BindEvent(OnCanselButton);
        });
        open.Restart();

        return true;
    }

    void OnGachaButton()
    {
        int rand = UnityEngine.Random.Range(0, 58);
        Sprite result = Managers.Resource.Load<Sprite>($"Sprites/Gacha/{rand}");
        GetImage((int)Images.ResultImage).sprite = result;
    }

    void OnCanselButton()
    {
        Destroy(GetObject((int)GameObjects.GachaButton).GetComponent<UI_EventHandler>());
        Destroy(GetObject((int)GameObjects.CanselButton).GetComponent<UI_EventHandler>());

        Sequence close = Utils.MakePopupCloseSequence(GetObject((int)GameObjects.Rect));
        close.OnComplete(() =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
}
