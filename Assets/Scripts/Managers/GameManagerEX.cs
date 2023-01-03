using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Vec2Pos
{
    public float x;
    public float y;
}

[Serializable]
public class GameData
{
    public int score;
    public int fullBallCount;
    public int ballSpeed;

    public Vec2Pos hamsterPos;
    public Vec2Pos[] blockPosList;
}


public class GameManagerEX
{
    GameData _gameData;

    public int FullBallCount
    {
        get { return _gameData.fullBallCount; }
        set { _gameData.fullBallCount = value; }
    }

    public Vec2Pos HamsterPos
    {
        get { return _gameData.hamsterPos; }
        set { _gameData.hamsterPos = value; }
    }

    public void Init()
    {
        // TODO : 데이터 시트
        FullBallCount = 5;
        HamsterPos = new Vec2Pos
        {
            x = 100f,
            y = 100f
        };
    }
}