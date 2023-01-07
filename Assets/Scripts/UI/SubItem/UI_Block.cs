using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Block : UI_Spine
{
    Action<UI_Block> _destroyCallBack;
    BlockInfo _info;

    enum Texts
    {
        HPText
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        return true;
    }

    public void SetInfo(BlockInfo info, Action<UI_Block> destroyCallBack)
    {
        Init();

        _info = info;
        _destroyCallBack = destroyCallBack;

        transform.localPosition = new Vector3(Managers.Game.BlockStartX + (info.x * Managers.Game.BlockGapX), Managers.Game.BlockStartY, 0);
        RefreshUI();
    }

    public void RefreshUI()
    {
        GetText((int)Texts.HPText).text = $"{_info.hp}";
    }

    public BlockInfo GetInfo()
    {
        return _info;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            _info.hp -= 1;
            RefreshUI();
            if (_info.hp <= 0)
            {
                _destroyCallBack?.Invoke(this);
            }
        }
    }
}
