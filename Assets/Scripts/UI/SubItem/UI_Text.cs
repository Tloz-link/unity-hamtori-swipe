using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Text : UI_Base
{
    enum Texts
    {
        UI_Text
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        return true;
    }

    public void SetInfo(string text, float distance)
    {
        Init();

        GetText((int)Texts.UI_Text).text = text;
        Sequence create = Utils.MakeIncreaseTextSequence(gameObject, distance);
        create.OnComplete(() =>
        {
            Managers.Resource.Destroy(gameObject);
        });

        create.Restart();
    }
}
