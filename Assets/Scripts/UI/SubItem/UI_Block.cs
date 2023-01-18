using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Block : UI_Spine
{
    Action<UI_Block> _destroyCallBack;
    BlockInfo _info;
    StartData _startData;

    enum Texts
    {
        HPText
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));

        _startData = Managers.Data.Start;
        return true;
    }

    public void SetInfo(BlockInfo info, Action<UI_Block> destroyCallBack)
    {
        Init();

        _info = info;
        _destroyCallBack = destroyCallBack;

        transform.localPosition = new Vector3(_startData.blockStartX + (info.x * _startData.blockGapX), _startData.blockStartY - (info.y * _startData.blockGapY), 0);

        Sequence spawn = Utils.MakeSpawnSequence(gameObject);
        spawn.Restart();
        RefreshUI();
    }

    public void MoveNext()
    {
        _info.y += 1;
        Vector3 dest = new Vector3(_startData.blockStartX + (_info.x * _startData.blockGapX), _startData.blockStartY - (_info.y * _startData.blockGapY), 0);
        Move(dest);
    }

    public void RefreshUI()
    {
        GetText((int)Texts.HPText).text = $"{_info.hp}";
    }

    public BlockInfo GetInfo()
    {
        return _info;
    }

    public void Damaged(int attack)
    {
        _info.hp -= Managers.Game.BallDamage * attack;
        RefreshUI();
        if (_info.hp <= 0)
        {
            _destroyCallBack?.Invoke(this);
        }
    }
}
