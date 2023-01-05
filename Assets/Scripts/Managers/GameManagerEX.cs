using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public int score;
    public int fullBallCount;
    public int ballSpeed;

    public float hamsterPosX;
}


public class GameManagerEX
{
    GameData _gameData = new GameData();

    public int Score
    {
        get { return _gameData.score; }
        set { _gameData.score = value; }
    }

    public int FullBallCount
    {
        get { return _gameData.fullBallCount; }
        set { _gameData.fullBallCount = value; }
    }

    public int BallSpeed
    {
        get { return _gameData.ballSpeed; }
        set { _gameData.ballSpeed = value; }
    }

    public float HamsterPosX
    {
        get { return _gameData.hamsterPosX; }
        set { _gameData.hamsterPosX = value; }
    }

    public void Init()
    {
        StartData data = Managers.Data.Start;

        Score = data.score;
        FullBallCount = data.fullBallCount;
        BallSpeed = data.ballSpeed;
        HamsterPosX = data.hamsterPosX;

    }
}