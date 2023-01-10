using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Item_Star : UI_Spine
{
    Action<UI_Item_Star> _destroyCallBack;
    ItemInfo _info;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        return true;
    }

    public void SetInfo(ItemInfo info, Action<UI_Item_Star> destroyCallBack)
    {
        Init();

        _info = info;
        _destroyCallBack = destroyCallBack;

        transform.localPosition = new Vector3(Managers.Game.BlockStartX + (info.x * Managers.Game.BlockGapX), Managers.Game.BlockStartY, 0);
    }

    public ItemInfo GetInfo()
    {
        return _info;
    }

    public void TouchStar()
    {
        _destroyCallBack.Invoke(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            TouchStar();
        }
    }
}
