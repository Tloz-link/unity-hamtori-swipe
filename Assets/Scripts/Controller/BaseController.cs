using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : UI_Base
{
    protected SkeletonGraphic _anim = null;

    protected Vector3 _dest;
    protected bool _move = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _anim = GetComponent<SkeletonGraphic>();
        _move = false;
        return true;
    }

    public virtual void Move(Vector3 dest)
    {
        Init();

        _dest = dest;
        _move = true;
    }

    void Update()
    {
        if (_move == false)
            return;

        Vector3 dir = _dest - transform.localPosition;
        if (dir.magnitude < 0.0001f)
        {
            _move = false;
        }
        else
        {
            float delta = 1 + dir.magnitude / 400f;
            float moveDist = Mathf.Clamp(1200f * Time.deltaTime * delta, 0, dir.magnitude);
            transform.localPosition += dir.normalized * moveDist;
        }
    }

    #region Spine Animation
    public void SetSkeletonAsset(string path)
    {
        Init();
        _anim.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(path);
        _anim.Initialize(true);
    }

    public void PlayAnimation(string name, bool loop = true)
    {
        Init();
        _anim.startingAnimation = name;
        _anim.startingLoop = loop;
    }

    public void ChangeSkin(string name)
    {
        Init();
        _anim.initialSkinName = name;
        _anim.Initialize(true);
    }

    public void Refresh()
    {
        Init();
        _anim.Initialize(true);
    }

    public void PlayAnimationOnce(string name)
    {
        StartCoroutine(CoPlayAnimationOnce(name));
    }

    IEnumerator CoPlayAnimationOnce(string name)
    {
        bool defaultLoop = _anim.startingLoop;
        string defaultName = _anim.startingAnimation;

        _anim.startingLoop = false;
        _anim.startingAnimation = name;

        float length = _anim.skeletonDataAsset.GetSkeletonData(true).FindAnimation(name).Duration;
        yield return new WaitForSeconds(length); // 애니 시간만큼 대기

        // 기존 애니 복원
        PlayAnimation(defaultName, defaultLoop);
    }

    public void PlayAnimationOnce(string skin, string name)
    {
        StartCoroutine(CoPlayAnimationOnce(skin, name));
    }

    IEnumerator CoPlayAnimationOnce(string skin, string name)
    {
        bool defaultLoop = _anim.startingLoop;
        string defaultSkin = _anim.initialSkinName;
        string defaultName = _anim.startingAnimation;

        _anim.startingLoop = false;
        _anim.startingAnimation = name;
        ChangeSkin(skin);

        float length = _anim.skeletonDataAsset.GetSkeletonData(true).FindAnimation(name).Duration;
        yield return new WaitForSeconds(length); // 애니 시간만큼 대기

        // 기존 애니 복원
        PlayAnimation(defaultName, defaultLoop);
        ChangeSkin(defaultSkin);
    }
    #endregion
}
