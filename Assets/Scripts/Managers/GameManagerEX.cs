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

public enum GameState
{
    idle,
    shoot
}

[Serializable]
public class GameData
{
    public GameState gameState;

    public int score;
    public int fullBallCount;
    public int ballSpeed;

    //½ºÅ³
    public int glassesCooltime;
    public int lineCount;
    public int powerUpCooltime;
    public int ballDamage;

    public float hamsterPosX;
    public BlockInfo[] blockList = new BlockInfo[MAX_BLOCK_COUNT];
    public ItemInfo[] itemList = new ItemInfo[MAX_BLOCK_COUNT];
}


public class GameManagerEX
{
    GameData _gameData = new GameData();

    public GameState State
    {
        get { return _gameData.gameState; }
        set { _gameData.gameState = value; }
    }

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

    public int GlassesCooltime
    {
        get { return _gameData.glassesCooltime; }
        set { _gameData.glassesCooltime = value; }
    }

    public int LineCount
    {
        get { return _gameData.lineCount; }
        set { _gameData.lineCount = value; }
    }

    public int PowerUpCooltime
    {
        get { return _gameData.powerUpCooltime; }
        set { _gameData.powerUpCooltime = value; }
    }

    public int BallDamage
    {
        get { return _gameData.ballDamage; }
        set { _gameData.ballDamage = value; }
    }

    public void Init()
    {
        StartData data = Managers.Data.Start;

        State = GameState.idle;
        Score = data.score;
        FullBallCount = data.fullBallCount;
        BallSpeed = data.ballSpeed;
        HamsterPosX = data.hamsterPosX;
        GlassesCooltime = 0;
        PowerUpCooltime = 0;
        LineCount = data.lineCount;
        BallDamage = data.ballDamage;
    }
}