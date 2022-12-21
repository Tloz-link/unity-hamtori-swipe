using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerEX
{
    public int CurrentBallCount { get; private set; }
    public int FullBallCount { get; private set; }
    private GameObject _shootRoot;
    private GameObject _waitRoot;

    public GameObject Hamster { get; private set; }

    private Queue<GameObject> _ballQueue = new Queue<GameObject>();
    public Vector3 BallDirection { get; set; }

    public Action OnIdleHandler = null;
    public Action OnShootHandler = null;
    public enum GameState
    {
        Idle,
        Shoot,
        Wait
    }
    private GameState _state;
    public GameState State {
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
            }
        }
    }

    public void Init()
    {
        State = GameState.Idle;
        CurrentBallCount = 0;
        FullBallCount = 5;
        Hamster = GameObject.Find("Hamster");
        _shootRoot = GameObject.Find("@SHOOT");
        _waitRoot = GameObject.Find("@WAIT");
        //todo : 못찾을 경우 자동으로 추가하기 (프로퍼티 쓰는것도 생각해야함)

        for (int i = 1; i <= FullBallCount; ++i)
        {
            GameObject ball = Managers.Resource.Instantiate("Contents/Ball");
            AddBall(ball.GetComponent<Ball>());
        }
    }

    public void ShootBall()
    {
        GameObject ball = _ballQueue.Dequeue();
        ball.transform.parent = _shootRoot.transform;
        ball.transform.position = Hamster.transform.position;
        ball.transform.localPosition -= new Vector3(0, 0, ball.transform.localPosition.z);
        ball.GetComponent<Ball>().Shoot(BallDirection);

        CurrentBallCount = _ballQueue.Count;
    }

    public void AddBall(Ball ball)
    {
        if (CurrentBallCount >= FullBallCount)
            return;

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

        if (CurrentBallCount == FullBallCount)
        {
            ReadyBalls();
            State = GameState.Idle;
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
}