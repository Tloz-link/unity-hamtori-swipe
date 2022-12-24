using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Spine.Unity;
using Spine;

public class UI_Game : UI_Scene
{
    enum GameObjects
    {
        Bottom,
        Floor
    }

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
        Bind<GameObject>((typeof(GameObjects)));
        OnIdle();

        Managers.Game.CanvasScale = transform.localScale.x;
        Managers.Game.BoardFloorPos = Get<GameObject>((int)GameObjects.Floor).transform.position;

        // Submit Event
        Managers.Game.OnIdleHandler -= OnIdle;
        Managers.Game.OnIdleHandler += OnIdle;
        Managers.Game.OnShootHandler -= OnShoot;
        Managers.Game.OnShootHandler += OnShoot;

        float scaleHeight = ((float)Screen.width / Screen.height) / ((float)9 / 16);

        if (scaleHeight > 1)
            return;

        float offsetY = Mathf.Abs(((Screen.width * 1080 / Screen.width) * (Screen.height * 1080 / Screen.width)) - (1080 * 1920)) / 2 / 1080;
        Get<GameObject>((int)(GameObjects.Bottom)).transform.localPosition -= new Vector3(0, offsetY, 0);
    }

    void Update()
    {
        GetText((int)Texts.BallCount).text = $"{Managers.Game.BallQueue.Count + Managers.Game.ReturnBallQueue.Count}";
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
