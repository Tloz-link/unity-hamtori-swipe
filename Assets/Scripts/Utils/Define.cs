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
        PointerDown,
        PointerUp,
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
}
