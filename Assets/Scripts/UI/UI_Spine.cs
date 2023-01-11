using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Spine : UI_Base
{
    protected SkeletonGraphic _anim = null;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _anim = GetComponent<SkeletonGraphic>();
        return true;
    }

    #region Spine Animation
    public void SetSkeletonAsset(string path)
    {
        Init();
        _anim.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(path);
        _anim.Initialize(true);
    }

    public float GetAnimationLength(string name)
    {
        return _anim.skeletonDataAsset.GetSkeletonData(true).FindAnimation(name).Duration;
    }

    public void PlayAnimation(string name, bool loop = true)
    {
        Init();
        if (_anim.startingAnimation == name)
            return;

        _anim.startingAnimation = name;
        _anim.startingLoop = loop;
        _anim.Initialize(true);
    }

    public void PlayAnimationForce(string name, bool loop = true)
    {
        Init();
        _anim.startingAnimation = name;
        _anim.startingLoop = loop;
        _anim.Initialize(true);
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
        _anim.Initialize(true);

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
