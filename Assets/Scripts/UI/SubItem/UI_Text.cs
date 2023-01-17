using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Text : UI_Base
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Sequence create = Utils.MakeIncreaseTextSequence(gameObject);
        create.OnComplete(() =>
        {
            Managers.Resource.Destroy(gameObject);
        });
        create.Restart();

        return true;
    }
}
