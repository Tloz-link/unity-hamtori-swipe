using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerEX
{
    public GameObject Hamster { get; private set; }

    public Action OnIdleHandler = null;
    public Action OnShootHandler = null;
    public Action OnCleanHandler = null;
    public enum GameState
    {
        Idle,
        Shoot,
        Wait,
        Clean
    }
    private GameState _state;
    public GameState State
    {
        get { return _state; }
        set
        {
            _state = value;
            switch (_state)
            {
                case GameState.Idle:
                    if (OnIdleHandler != null)
                        OnIdleHandler.Invoke();
                    break;
                case GameState.Shoot:
                    if (OnShootHandler != null)
                        OnShootHandler.Invoke();
                    break;
                case GameState.Clean:
                    if (OnCleanHandler != null)
                        OnCleanHandler.Invoke();
                    break;
            }
        }
    }

    #region Ball
    public int CurrentBallCount { get; private set; }
    public int FullBallCount { get; private set; }
    private GameObject _shootRoot;
    private GameObject _waitRoot;

    private Queue<UI_Ball> _ballQueue = new Queue<UI_Ball>();
    public Vector3 BallDirection { get; set; }

    public void InitBall()
    {
        CurrentBallCount = 0;
        FullBallCount = 5;
        _shootRoot = GameObject.Find("@SHOOT");
        _waitRoot = GameObject.Find("@WAIT");

        for (int i = 1; i <= FullBallCount; ++i)
        {
            UI_Ball ball = Managers.UI.makeSubItem<UI_Ball>();
            AddBall(ball, true);
        }
    }

    public void ShootBall()
    {
        UI_Ball ball = _ballQueue.Dequeue();
        ball.transform.SetParent(_shootRoot.transform);
        ball.transform.position = Hamster.transform.position.Get2D();
        ball.Shoot(BallDirection);

        CurrentBallCount = _ballQueue.Count;
        ReadyBalls();
    }

    public void AddBall(UI_Ball ball, bool init = false)
    {
        Debug.Assert(CurrentBallCount < FullBallCount);

        _ballQueue.Enqueue(ball);
        ball.transform.SetParent(_waitRoot.transform);
        ball.transform.localPosition -= new Vector3(0, 0, ball.transform.localPosition.z);
        CurrentBallCount = _ballQueue.Count;

        if (CurrentBallCount == 1)
        {
            Vector3 dest = new Vector3(ball.transform.position.x, Hamster.transform.position.y, Hamster.transform.position.z);
            dest.x = Mathf.Clamp(dest.x, 160f, 920f);
            Hamster.GetComponent<Hamster>().ChangePos(dest);
        }
        else if (CurrentBallCount == FullBallCount)
        {
            ReadyBalls();
            if (init == false)
            {
                GenerateBlock();
                MoveBlocks();
            }
            State = GameState.Clean;
        }
    }

    private void ReadyBalls()
    {
        int idx = 1;
        int dir = (Hamster.transform.localPosition.x >= 0) ? 1 : -1;
        foreach (UI_Ball ball in _ballQueue)
        {
            ball.Ready(new Vector3((Hamster.transform.localPosition.x - (50 * dir)) - (50 * idx * dir), Hamster.transform.localPosition.y - 60, 0));
            idx++;
        }
    }
    #endregion

    #region Block
    private Dictionary<int, UI_Block> _blocks = new Dictionary<int, UI_Block>();
    public int BlockId { get; private set; } = 0;

    private GameObject _blockRoot;
    public GameObject BlockRoot
    {
        get
        {
            if (_blockRoot == null)
            {
                _blockRoot = GameObject.Find("@BlockList");
            }
            return _blockRoot;
        }
    }

    public void InitBlock()
    {
        int startX = -422;
        int deltaX = 140;
        int startY = 636;
        int deltaY = 142;

        CreateBlock(new Vector3(startX + (deltaX * 0), startY - (deltaY * 2), 0));
        CreateBlock(new Vector3(startX + (deltaX * 0), startY - (deltaY * 3), 0));
        CreateBlock(new Vector3(startX + (deltaX * 6), startY - (deltaY * 2), 0));
        CreateBlock(new Vector3(startX + (deltaX * 6), startY - (deltaY * 3), 0));
        CreateBlock(new Vector3(startX + (deltaX * 1), startY - (deltaY * 2), 0));
        CreateBlock(new Vector3(startX + (deltaX * 1), startY - (deltaY * 3), 0));
        CreateBlock(new Vector3(startX + (deltaX * 5), startY - (deltaY * 2), 0));
        CreateBlock(new Vector3(startX + (deltaX * 5), startY - (deltaY * 3), 0));
    }

    public void RemoveBlock(int id)
    {
        Debug.Assert(_blocks.ContainsKey(id) == true);

        Managers.Resource.Destroy(_blocks[id].gameObject);
        _blocks.Remove(id);
    }

    public void GenerateBlock()
    {
        int count = UnityEngine.Random.Range(1, 5);
        List<Vector3> spawnList = new List<Vector3>();

        for (int i = 0; i < 7; ++i)
        {
            spawnList.Add(new Vector3(-422 + (i * 140), 636, 0));
        }

        for (int i = 0; i < count; ++i)
        {
            int rand = UnityEngine.Random.Range(0, spawnList.Count);
            CreateBlock(spawnList[rand]);
            spawnList.RemoveAt(rand);
        }
    }

    private void CreateBlock(Vector3 pos)
    {
        UI_Block block = Managers.UI.makeSubItem<UI_Block>(BlockRoot.transform);
        block.transform.localPosition = pos;

        block.Hp = FullBallCount;
        block.Id = ++BlockId;
        _blocks.Add(block.Id, block);
    }

    private void MoveBlocks()
    {
        foreach (var go in _blocks)
        {
            UI_Block block = go.Value;
            block.MoveNextPos();
        }
    }
    #endregion

    public void Init()
    {
        Hamster = GameObject.Find("Hamster");

        InitBall();
        InitBlock();
    }
}