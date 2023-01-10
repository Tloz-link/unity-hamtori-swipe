using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[Serializable]
public class ItemInfo
{
    public int x;
    public int y;
}

[Serializable]
public class BlockInfo
{
    public int x;
    public int y;
    public int hp;
}

[Serializable]
public class GameData
{
    public int score;
    public int fullBallCount;
    public int ballSpeed;

    public float hamsterPosX;
    public BlockInfo[] blockList = new BlockInfo[MAX_BLOCK_COUNT];
    public ItemInfo[] itemList = new ItemInfo[MAX_BLOCK_COUNT];
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

    public float BlockGapX
    {
        get { return Managers.Data.Start.blockGapX; }
    }

    public float BlockGapY
    {
        get { return Managers.Data.Start.blockGapY; }
    }

    public float BlockStartX
    {
        get { return Managers.Data.Start.blockStartX; }
    }

    public float BlockStartY
    {
        get { return Managers.Data.Start.blockStartY; }
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