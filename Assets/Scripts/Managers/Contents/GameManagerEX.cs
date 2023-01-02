using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{

}


public class GameManagerEX
{
    public float CanvasScale { get; set; }
    public Vector3 BoardFloorPos { get; set; }

    public GameObject Hamster { get; private set; }
    private GameObject _hamsterRoot;

    public Action OnIdleHandler = null;
    public Action OnShootHandler = null;
    public Action OnCleanHandler = null;
    public Action OnOverHandler = null;
    public enum GameState
    {
        Idle,
        Shoot,
        Clean,

        Over
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
                case GameState.Over:
                    if (OnOverHandler != null)
                        OnOverHandler.Invoke();
                    break;
            }
        }
    }

    #region Ball
    public int FullBallCount { get; private set; }
    public int ShootBallCount { get; set; }
    public int ReturnBallCount { get; set; }
    private GameObject _shootRoot;
    private GameObject _waitRoot;

    public Queue<UI_Ball> BallQueue { get; private set; } = new Queue<UI_Ball>();
    public Queue<UI_Ball> ReturnBallQueue { get; private set; } = new Queue<UI_Ball>();
    public Vector3 BallDirection { get; set; }
    public Vector3 ShootPos { get; set; }

    public void InitBall()
    {
        FullBallCount = 5;
        ShootBallCount = 0;
        ReturnBallCount = 0;
        _shootRoot = GameObject.Find("@SHOOT");
        _waitRoot = GameObject.Find("@WAIT");
        ShootPos = Hamster.transform.localPosition;

        for (int i = 1; i <= FullBallCount; ++i)
        {
            UI_Ball ball = Managers.UI.makeSubItem<UI_Ball>();
            AddBall(ball, true);
        }
    }

    public void ShootBall()
    {
        UI_Ball ball = BallQueue.Dequeue();
        ball.transform.SetParent(_shootRoot.transform);
        ball.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        ball.transform.localPosition = ShootPos.Get2D();
        ball.Shoot(BallDirection);
        ++ShootBallCount;

        ReadyBalls();
    }

    public void AddBall(UI_Ball ball, bool init = false)
    {
        if (init == true)
        {
            BallQueue.Enqueue(ball);
        }
        else
        {
            ReturnBallQueue.Enqueue(ball);
        }

        ball.transform.SetParent(_waitRoot.transform);
        ball.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        ball.transform.localPosition -= new Vector3(0, 0, ball.transform.localPosition.z);

        if (init == true)
        {
            if (BallQueue.Count == FullBallCount)
            {
                ReadyBalls(true);
            }
            return;
        }

        ++ReturnBallCount;
        if (ReturnBallCount == 1)
        {
            Vector3 dest = new Vector3(ball.transform.localPosition.x, Hamster.transform.localPosition.y, Hamster.transform.localPosition.z);
            dest.x = Mathf.Clamp(dest.x, -380f, 380f);
            Hamster.GetComponent<Hamster>().ChangePos(dest);
        }
        else if (ReturnBallCount == FullBallCount)
        {
            while (ReturnBallQueue.Count > 0)
                BallQueue.Enqueue(ReturnBallQueue.Dequeue());

            ShootPos = Hamster.transform.localPosition;
            ReadyBalls();
            GenerateBlock();
            MoveBlocks();
            State = GameState.Clean;
        }
    }

    private void ReadyBalls(bool isInit = false)
    {
        int dir = (ShootPos.x >= 0) ? -1 : 1;
        int delta = 50;
        float maxX = ShootPos.x + (50 * dir) + (delta * (FullBallCount - 1) * dir);
        if (maxX < -450f || maxX > 450f)
        {
            float range = (dir == 1) ? 450f - (ShootPos.x + 50f) : 450f + (ShootPos.x - 50f);
            delta = (int)range / FullBallCount;
        }

        int idx = 0;
        foreach (UI_Ball ball in BallQueue)
        {
            float posX = ShootPos.x + (50 * dir) + (delta * idx * dir);
            if (isInit == true)
            {
                ball.transform.localPosition = new Vector3(posX, ShootPos.y - 60, 0);
            }
            ball.Ready(new Vector3(posX, ShootPos.y - 60, 0));
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

    public bool CheckBlocks()
    {
        foreach (var go in _blocks)
        {
            UI_Block block = go.Value;
            if (block.transform.localPosition.y < -400)
            {
                GameOver();
                return false;
            }
        }

        return true;
    }

    #endregion

    public void Init()
    {
        Hamster = GameObject.Find("Hamster");
        _hamsterRoot = GameObject.Find("@Hamster");

        InitBall();
        InitBlock();
    }

    public void GameOver()
    {
        State = GameState.Over;

        Managers.UI.ShowPopupUI<UI_GameOver>();
    }

    public void Restart()
    {
        foreach (UI_Ball ball in BallQueue)
        {
            Managers.Resource.Destroy(ball.gameObject);
        }
        BallQueue.Clear();

        foreach (var pair in _blocks)
        {
            UI_Block block = pair.Value;
            Managers.Resource.Destroy(block.gameObject);
        }
        _blocks.Clear();

        Hamster.transform.SetParent(_hamsterRoot.transform);
        Hamster.transform.localPosition = new Vector3(0f, -560f, 0f);
        Init();
        State = GameState.Idle;
    }
}