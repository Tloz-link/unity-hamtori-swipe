using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum Scene
    {
        Unknown,
        DevScene,
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount
    }

    public enum UIEvent
    {
        Click,
        Pressed,
        PressedLong,
        PointerDown,
        PointerUp,
        PointerExit
    }

    public enum AnimState
    {
        None,
        Idle,
        Charge,
        Shoot,
        Wait,
        Gameover
    }

    // 가로 7 : 세로 8
    public const int MAX_BLOCK_COUNT = 7 * 8;

    public const int MAX_VISIBLE_BALL_COUNT = 20;
    public const int MAX_BALL_COUNT = 99;
    public const int CLAMP_ANGLE = 2;
    public const float MAX_VOLUME = 0.5f;
    public const int MAX_VOLUME_COUNT = 6;

    public const int MAX_SOUND_OVERLAPPED = 2;

    public const float SHOOT_INTERVAL = 0.04f;

    public const int MAX_SCORE = 999999;
}
