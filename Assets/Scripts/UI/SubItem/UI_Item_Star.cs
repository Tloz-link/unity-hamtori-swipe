using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Item_Star : UI_Spine
{
    Action<UI_Item_Star> _destroyCallBack;
    ItemInfo _info;
    StartData _startData;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _startData = Managers.Data.Start;
        return true;
    }

    public void SetInfo(ItemInfo info, Action<UI_Item_Star> destroyCallBack)
    {
        Init();

        _info = info;
        _destroyCallBack = destroyCallBack;
        transform.localPosition = new Vector3(_startData.blockStartX + (info.x * _startData.blockGapX), _startData.blockStartY - (info.y * _startData.blockGapY), 0);

        Sequence spawn = Utils.MakeSpawnSequence(gameObject);
        spawn.OnComplete(() =>
        {
            Sequence idle = DOTween.Sequence()
                .Append(transform.DOScale(0.9f, 1f).SetEase(Ease.InBack))
                .Append(transform.DOScale(1.0f, 1f).SetEase(Ease.OutBack))
                .SetLoops(-1, LoopType.Restart);
            idle.Restart();
        });
        spawn.Restart();
    }

    public void MoveNext()
    {
        _info.y += 1;
        Vector3 dest = new Vector3(_startData.blockStartX + (_info.x * _startData.blockGapX), _startData.blockStartY - (_info.y * _startData.blockGapY), 0);
        Move(dest);
    }

    public ItemInfo GetInfo()
    {
        return _info;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            _destroyCallBack.Invoke(this);
        }
    }
}
