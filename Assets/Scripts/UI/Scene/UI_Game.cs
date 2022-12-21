using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Spine.Unity;
using Spine;

public class UI_Game : UI_Scene
{
    enum Texts
    {
        BallCount
    }

    enum SkeletonAnimations
    {
        Bubble
    }

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        Bind<Text>(typeof(Texts));
        Bind<SkeletonAnimation>(typeof(SkeletonAnimations));
        OnIdle();

        // Submit Event
        Managers.Game.OnIdleHandler -= OnIdle;
        Managers.Game.OnIdleHandler += OnIdle;
        Managers.Game.OnShootHandler -= OnShoot;
        Managers.Game.OnShootHandler += OnShoot;
    }

    void Update()
    {
        GetText((int)Texts.BallCount).text = $"{Managers.Game.CurrentBallCount}";
    }

    private void OnIdle()
    {
        SkeletonAnimation bubble = Get<SkeletonAnimation>((int)SkeletonAnimations.Bubble);
        bubble.AnimationState.Complete -= OnCompleteAnimation;
        bubble.AnimationState.Complete += OnCompleteAnimation;
    }

    private void OnShoot()
    {
        SkeletonAnimation bubble = Get<SkeletonAnimation>((int)SkeletonAnimations.Bubble);
        bubble.AnimationState.Complete -= OnCompleteAnimation;
        bubble.AnimationName = "animation";
    }

    private void OnCompleteAnimation(TrackEntry entry)
    {
        entry.Complete -= OnCompleteAnimation;
        Get<SkeletonAnimation>((int)SkeletonAnimations.Bubble).AnimationName = null;
    }
}
