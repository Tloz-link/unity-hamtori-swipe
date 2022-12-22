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

    private Queue<GameObject> _ballQueue = new Queue<GameObject>();
    public Vector3 BallDirection { get; set; }

    public void InitBall()
    {
        CurrentBallCount = 0;
        FullBallCount = 5;
        _shootRoot = GameObject.Find("@SHOOT");
        _waitRoot = GameObject.Find("@WAIT");

        for (int i = 1; i <= FullBallCount; ++i)
        {
            GameObject ball = Managers.Resource.Instantiate("Contents/Ball");
            AddBall(ball.GetComponent<Ball>(), true);
        }
    }

    public void ShootBall()
    {
        GameObject ball = _ballQueue.Dequeue();
        ball.transform.parent = _shootRoot.transform;
        ball.transform.localPosition = Hamster.transform.position.Get2D();
        ball.GetComponent<Ball>().Shoot(BallDirection);

        CurrentBallCount = _ballQueue.Count;
        ReadyBalls();
    }

    public void AddBall(Ball ball, bool init = false)
    {
        Debug.Assert(CurrentBallCount < FullBallCount);

        _ballQueue.Enqueue(ball.gameObject);
        ball.transform.parent = _waitRoot.transform;
        ball.transform.localPosition -= new Vector3(0, 0, ball.transform.localPosition.z);
        CurrentBallCount = _ballQueue.Count;

        if (CurrentBallCount == 1)
        {
            Vector3 dest = new Vector3(ball.transform.position.x, Hamster.transform.position.y, Hamster.transform.position.z);
            dest.x = Mathf.Clamp(dest.x, -380f, 380f);
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
        foreach (GameObject ball in _ballQueue)
        {
            ball.GetComponent<Ball>().Ready(new Vector3((Hamster.transform.localPosition.x - (50 * dir)) - (50 * idx * dir), Hamster.transform.position.y - 60, 0));
            idx++;
        }
    }
    #endregion

    #region Block
    private Dictionary<int, GameObject> _blocks = new Dictionary<int, GameObject>();
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

        Managers.Resource.Destroy(_blocks[id]);
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
        GameObject go = Managers.Resource.Instantiate("Contents/Block", BlockRoot.transform);
        go.transform.localPosition = pos;

        Block block = go.GetComponent<Block>();
        block.Hp = FullBallCount;
        block.Id = ++BlockId;
        _blocks.Add(block.Id, go);
    }

    private void MoveBlocks()
    {
        foreach (var go in _blocks)
        {
            Block block = go.Value.GetComponent<Block>();
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