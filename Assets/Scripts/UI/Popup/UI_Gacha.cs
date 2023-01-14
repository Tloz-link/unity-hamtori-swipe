using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Gacha : UI_Popup
{
    enum GameObjects
    {
        GachaButton,
        CanselButton
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

        GetObject((int)GameObjects.GachaButton).gameObject.BindEvent(OnGachaButton);
        GetObject((int)GameObjects.CanselButton).gameObject.BindEvent(OnCanselButton);

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
        Managers.UI.ClosePopupUI(this);
    }
}
