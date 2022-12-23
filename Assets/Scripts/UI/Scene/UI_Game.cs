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
        Bind<SkeletonGraphic>(typeof(SkeletonAnimations));
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
        SkeletonGraphic bubble = Get<SkeletonGraphic>((int)SkeletonAnimations.Bubble);
        bubble.AnimationState.Complete -= OnCompleteAnimation;
        bubble.AnimationState.Complete += OnCompleteAnimation;
    }

    private void OnShoot()
    {
        SkeletonGraphic bubble = Get<SkeletonGraphic>((int)SkeletonAnimations.Bubble);
        bubble.AnimationState.Complete -= OnCompleteAnimation;
        bubble.AnimationState.SetAnimation(0, "animation", true);
        bubble.AnimationState.TimeScale = 1;
    }

    private void OnCompleteAnimation(TrackEntry entry)
    {
        entry.Complete -= OnCompleteAnimation;
        Get<SkeletonGraphic>((int)SkeletonAnimations.Bubble).AnimationState.TimeScale = 0;
    }
}
